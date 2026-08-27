using System.Security.Cryptography;
using System.Text;
using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.Comercial;
using DetectorEstafas.Web.Services.Comercial.MercadoPago;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Tests.Services.Comercial;

[TestClass]
public sealed class CommercialAutomation230Tests
{
    [TestMethod]
    public void Prueba_TieneCuotaDiariaDe20()
    {
        Assert.AreEqual(20, ApiPlanes.CuotaDiariaPrueba);
    }

    [TestMethod]
    public async Task PruebaDuplicada_NoActivaSegundoAcceso()
    {
        await using DetectorEstafasDbContext db = CrearDb();
        FakeProvisionamiento provisionamiento = new()
        {
            ResultadoPrueba = new ProvisionamientoApiResultado(
                EstadoProvisionamientoApi.YaExistia,
                7,
                null,
                false)
        };
        FakeMercadoPago mercadoPago = new();
        FakeCorreo correo = new();
        ComercializacionApiService service = CrearService(
            db,
            provisionamiento,
            mercadoPago,
            correo);

        ActivacionPruebaComercialResultado resultado =
            await service.ActivarPruebaAsync(
                "Cliente",
                "cliente@example.com",
                "https://example.test",
                CancellationToken.None);

        Assert.IsFalse(resultado.Exito);
        Assert.IsTrue(resultado.YaExistia);
        Assert.AreEqual(1, provisionamiento.CrearPruebaCalls);
        Assert.AreEqual(0, correo.AccesoListoCalls);
    }

    [TestMethod]
    public void WebhookSignature_AceptaFirmaValidaYRechazaFirmaInvalida()
    {
        const string secret = "secret-webhook-230";
        const string dataId = "123456";
        const string requestId = "request-230";
        const string timestamp = "1770000000000";

        string manifest =
            $"id:{dataId};request-id:{requestId};ts:{timestamp};";

        using HMACSHA256 hmac = new(
            Encoding.UTF8.GetBytes(secret));

        string hash = Convert.ToHexString(
                hmac.ComputeHash(
                    Encoding.UTF8.GetBytes(manifest)))
            .ToLowerInvariant();

        MercadoPagoWebhookSignatureValidator validator = new(
            Microsoft.Extensions.Options.Options.Create(
                new MercadoPagoOptions
                {
                    Enabled = true,
                    WebhookSecret = secret
                }));

        Assert.IsTrue(
            validator.EsValida(
                $"ts={timestamp},v1={hash}",
                requestId,
                dataId));

        Assert.IsFalse(
            validator.EsValida(
                $"ts={timestamp},v1={new string('0', 64)}",
                requestId,
                dataId));
    }

    [TestMethod]
    public async Task PagoRechazado_MarcaImpagaYDefineGracia()
    {
        await using DetectorEstafasDbContext db = CrearDb();
        SuscripcionComercial suscripcion = new()
        {
            Nombre = "Cliente",
            Email = "cliente@example.com",
            Plan = ApiPlanes.Starter,
            Estado = EstadosSuscripcionComercial.Activa,
            MercadoPagoPreapprovalId = "plan-1",
            Monto = 100,
            Moneda = "ARS"
        };
        db.SuscripcionesComerciales.Add(suscripcion);
        await db.SaveChangesAsync();

        FakeMercadoPago mercadoPago = new()
        {
            PagoAutorizado = new MercadoPagoPagoAutorizadoDetalle(
                "auth-1",
                "plan-1",
                string.Empty,
                "rejected",
                DateTime.UtcNow)
        };
        FakeCorreo correo = new();
        ComercializacionApiService service = CrearService(
            db,
            new FakeProvisionamiento(),
            mercadoPago,
            correo,
            graceDays: 3);

        await service.ProcesarWebhookAsync(
            "evento-rechazado",
            "subscription_authorized_payment",
            "created",
            "auth-1",
            "https://example.test",
            CancellationToken.None);

        SuscripcionComercial actual =
            await db.SuscripcionesComerciales.SingleAsync();

        Assert.AreEqual(
            EstadosSuscripcionComercial.Impaga,
            actual.Estado);
        Assert.IsNotNull(actual.PeriodoGraciaHastaUtc);
        Assert.IsTrue(
            actual.PeriodoGraciaHastaUtc > DateTime.UtcNow.AddDays(2));
        Assert.AreEqual(1, correo.ProblemaPagoCalls);
    }

    [TestMethod]
    public async Task GraciaVencida_SuspendeYDeshabilitaCliente()
    {
        await using DetectorEstafasDbContext db = CrearDb();
        ApiCliente cliente = new()
        {
            Nombre = "Cliente",
            Email = "cliente@example.com",
            Plan = ApiPlanes.Starter,
            CuotaMensual = ApiPlanes.CuotaMensualStarter,
            Habilitado = true
        };
        db.ApiClientes.Add(cliente);
        await db.SaveChangesAsync();

        db.SuscripcionesComerciales.Add(
            new SuscripcionComercial
            {
                Nombre = "Cliente",
                Email = "cliente@example.com",
                Plan = ApiPlanes.Starter,
                Estado = EstadosSuscripcionComercial.Impaga,
                ApiClienteId = cliente.ApiClienteId,
                Cliente = cliente,
                PeriodoGraciaHastaUtc = DateTime.UtcNow.AddMinutes(-1),
                Monto = 100,
                Moneda = "ARS"
            });
        await db.SaveChangesAsync();

        ComercializacionApiService service = CrearService(
            db,
            new FakeProvisionamiento(),
            new FakeMercadoPago(),
            new FakeCorreo());

        int afectadas = await service.AplicarSuspensionesVencidasAsync(
            CancellationToken.None);

        SuscripcionComercial actual =
            await db.SuscripcionesComerciales
                .Include(x => x.Cliente)
                .SingleAsync();

        Assert.AreEqual(1, afectadas);
        Assert.AreEqual(
            EstadosSuscripcionComercial.Suspendida,
            actual.Estado);
        Assert.IsFalse(actual.Cliente!.Habilitado);
    }

    [TestMethod]
    public async Task Cancelacion_MantieneAccesoHastaFinYLuegoDeshabilita()
    {
        await using DetectorEstafasDbContext db = CrearDb();
        ApiCliente cliente = new()
        {
            Nombre = "Cliente",
            Email = "cliente@example.com",
            Plan = ApiPlanes.Starter,
            CuotaMensual = ApiPlanes.CuotaMensualStarter,
            Habilitado = true
        };
        db.ApiClientes.Add(cliente);
        await db.SaveChangesAsync();

        db.SuscripcionesComerciales.Add(
            new SuscripcionComercial
            {
                Nombre = "Cliente",
                Email = "cliente@example.com",
                Plan = ApiPlanes.Starter,
                Estado = EstadosSuscripcionComercial.Activa,
                MercadoPagoPreapprovalId = "plan-cancel",
                ApiClienteId = cliente.ApiClienteId,
                Cliente = cliente,
                Monto = 100,
                Moneda = "ARS"
            });
        await db.SaveChangesAsync();

        DateTime finAcceso = DateTime.UtcNow.AddDays(10);
        FakeMercadoPago mercadoPago = new()
        {
            Suscripcion = new MercadoPagoSuscripcionDetalle(
                "plan-cancel",
                "canceled",
                string.Empty,
                finAcceso)
        };
        FakeCorreo correo = new();
        ComercializacionApiService service = CrearService(
            db,
            new FakeProvisionamiento(),
            mercadoPago,
            correo);

        await service.ProcesarWebhookAsync(
            "evento-cancelacion",
            "subscription_preapproval",
            "updated",
            "plan-cancel",
            "https://example.test",
            CancellationToken.None);

        SuscripcionComercial cancelada =
            await db.SuscripcionesComerciales
                .Include(x => x.Cliente)
                .SingleAsync();

        Assert.AreEqual(
            EstadosSuscripcionComercial.Cancelada,
            cancelada.Estado);
        Assert.IsTrue(cancelada.Cliente!.Habilitado);
        Assert.IsNotNull(cancelada.FechaFinAccesoUtc);
        Assert.AreEqual(1, correo.CancelacionCalls);

        cancelada.FechaFinAccesoUtc = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();

        int afectadas = await service.AplicarSuspensionesVencidasAsync(
            CancellationToken.None);

        Assert.AreEqual(1, afectadas);
        Assert.IsFalse(cancelada.Cliente.Habilitado);
    }

    [TestMethod]
    public async Task GrowthAprobado_ActivaEIdempotenciaEvitaDuplicados()
    {
        await using DetectorEstafasDbContext db = CrearDb();
        db.SuscripcionesComerciales.Add(
            new SuscripcionComercial
            {
                Nombre = "Cliente Growth",
                Email = "growth@example.com",
                Plan = ApiPlanes.Growth,
                Estado = EstadosSuscripcionComercial.Pendiente,
                MercadoPagoPreapprovalId = "plan-growth",
                Monto = 200,
                Moneda = "ARS"
            });
        await db.SaveChangesAsync();

        FakeProvisionamiento provisionamiento = new()
        {
            ResultadoPago = new ProvisionamientoApiResultado(
                EstadoProvisionamientoApi.Creado,
                99,
                "token-entrega",
                true)
        };
        FakeMercadoPago mercadoPago = new()
        {
            PagoAutorizado = new MercadoPagoPagoAutorizadoDetalle(
                "auth-growth",
                "plan-growth",
                string.Empty,
                "approved",
                DateTime.UtcNow),
            Suscripcion = new MercadoPagoSuscripcionDetalle(
                "plan-growth",
                "authorized",
                string.Empty,
                DateTime.UtcNow.AddMonths(1))
        };
        FakeCorreo correo = new();
        ComercializacionApiService service = CrearService(
            db,
            provisionamiento,
            mercadoPago,
            correo);

        for (int i = 0; i < 2; i++)
        {
            await service.ProcesarWebhookAsync(
                "evento-growth-aprobado",
                "subscription_authorized_payment",
                "created",
                "auth-growth",
                "https://example.test",
                CancellationToken.None);
        }

        SuscripcionComercial actual =
            await db.SuscripcionesComerciales.SingleAsync();

        Assert.AreEqual(
            EstadosSuscripcionComercial.Activa,
            actual.Estado);
        Assert.AreEqual(99, actual.ApiClienteId);
        Assert.AreEqual(ApiPlanes.Growth, provisionamiento.UltimoPlanPagado);
        Assert.AreEqual(1, provisionamiento.ActivarPagoCalls);
        Assert.AreEqual(1, correo.AccesoListoCalls);
        Assert.AreEqual(1, await db.WebhookComercialEventos.CountAsync());
    }

    private static DetectorEstafasDbContext CrearDb()
    {
        DbContextOptions<DetectorEstafasDbContext> options =
            new DbContextOptionsBuilder<DetectorEstafasDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
                .Options;

        return new DetectorEstafasDbContext(options);
    }

    private static ComercializacionApiService CrearService(
        DetectorEstafasDbContext db,
        FakeProvisionamiento provisionamiento,
        FakeMercadoPago mercadoPago,
        FakeCorreo correo,
        int graceDays = 3)
    {
        IOptions<MercadoPagoOptions> options =
            Microsoft.Extensions.Options.Options.Create(
                new MercadoPagoOptions
                {
                    Enabled = true,
                    AccessToken = "token-test",
                    WebhookSecret = "secret-test",
                    CurrencyId = "ARS",
                    StarterAmount = 100,
                    GrowthAmount = 200,
                    GraceDays = graceDays
                });

        return new ComercializacionApiService(
            db,
            provisionamiento,
            mercadoPago,
            correo,
            options);
    }

    private sealed class FakeProvisionamiento :
        IProvisionamientoApiComercialService
    {
        public ProvisionamientoApiResultado ResultadoPrueba { get; set; } =
            new(
                EstadoProvisionamientoApi.Creado,
                10,
                "token-prueba",
                true);

        public ProvisionamientoApiResultado ResultadoPago { get; set; } =
            new(
                EstadoProvisionamientoApi.Creado,
                20,
                "token-pago",
                true);

        public int CrearPruebaCalls { get; private set; }
        public int ActivarPagoCalls { get; private set; }
        public string? UltimoPlanPagado { get; private set; }

        public Task<ProvisionamientoApiResultado> CrearPruebaAsync(
            string nombre,
            string email,
            CancellationToken cancellationToken)
        {
            CrearPruebaCalls++;
            return Task.FromResult(ResultadoPrueba);
        }

        public Task<ProvisionamientoApiResultado> ActivarPlanPagadoAsync(
            string nombre,
            string email,
            string plan,
            CancellationToken cancellationToken)
        {
            ActivarPagoCalls++;
            UltimoPlanPagado = plan;
            return Task.FromResult(ResultadoPago);
        }

        public Task<string?> ConsumirEntregaClaveAsync(
            string token,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(null);
        }
    }

    private sealed class FakeMercadoPago : IMercadoPagoSuscripcionService
    {
        public MercadoPagoSuscripcionDetalle? Suscripcion { get; set; }
        public MercadoPagoPagoAutorizadoDetalle? PagoAutorizado { get; set; }
        public MercadoPagoPagoDetalle? Pago { get; set; }

        public Task<MercadoPagoSuscripcionCreada> CrearPendienteAsync(
            string email,
            string plan,
            string referenciaExterna,
            decimal monto,
            string moneda,
            string backUrl,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new MercadoPagoSuscripcionCreada(
                    "plan-test",
                    "https://www.mercadopago.com.ar/test",
                    "active",
                    referenciaExterna,
                    null));
        }

        public Task<MercadoPagoSuscripcionDetalle?> ObtenerSuscripcionAsync(
            string preapprovalId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Suscripcion);
        }

        public Task<MercadoPagoPagoAutorizadoDetalle?> ObtenerPagoAutorizadoAsync(
            string authorizedPaymentId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(PagoAutorizado);
        }

        public Task<MercadoPagoPagoDetalle?> ObtenerPagoAsync(
            string paymentId,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Pago);
        }
    }

    private sealed class FakeCorreo : ICorreoComercialService
    {
        public int AccesoListoCalls { get; private set; }
        public int ProblemaPagoCalls { get; private set; }
        public int CancelacionCalls { get; private set; }

        public Task EnviarAccesoListoAsync(
            string destinatario,
            string plan,
            string? enlaceEntregaClave,
            bool mantieneClaveExistente,
            CancellationToken cancellationToken = default)
        {
            AccesoListoCalls++;
            return Task.CompletedTask;
        }

        public Task EnviarProblemaPagoAsync(
            string destinatario,
            string plan,
            CancellationToken cancellationToken = default)
        {
            ProblemaPagoCalls++;
            return Task.CompletedTask;
        }

        public Task EnviarCancelacionAsync(
            string destinatario,
            string plan,
            DateTime? accesoHastaUtc,
            CancellationToken cancellationToken = default)
        {
            CancelacionCalls++;
            return Task.CompletedTask;
        }
    }
}
