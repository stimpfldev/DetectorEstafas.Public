using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.Comercial.MercadoPago;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Comercial;

public sealed class ComercializacionApiService :
    IComercializacionApiService
{
    private readonly DetectorEstafasDbContext _dbContext;
    private readonly IProvisionamientoApiComercialService _provisionamiento;
    private readonly IMercadoPagoSuscripcionService _mercadoPago;
    private readonly ICorreoComercialService _correo;
    private readonly MercadoPagoOptions _options;

    public ComercializacionApiService(
        DetectorEstafasDbContext dbContext,
        IProvisionamientoApiComercialService provisionamiento,
        IMercadoPagoSuscripcionService mercadoPago,
        ICorreoComercialService correo,
        IOptions<MercadoPagoOptions> options)
    {
        _dbContext = dbContext;
        _provisionamiento = provisionamiento;
        _mercadoPago = mercadoPago;
        _correo = correo;
        _options = options.Value;
    }

    public async Task<ActivacionPruebaComercialResultado>
        ActivarPruebaAsync(
            string nombre,
            string email,
            string baseUrl,
            CancellationToken cancellationToken)
    {
        ProvisionamientoApiResultado resultado =
            await _provisionamiento.CrearPruebaAsync(
                nombre,
                email,
                cancellationToken);

        if (resultado.Estado ==
            EstadoProvisionamientoApi.YaExistia)
        {
            return new ActivacionPruebaComercialResultado(
                false,
                true,
                null,
                "Ya existe una prueba o acceso API asociado a ese correo.");
        }

        string? enlace = CrearEnlaceEntrega(
            baseUrl,
            resultado.TokenEntrega);

        await _correo.EnviarAccesoListoAsync(
            email.Trim().ToLowerInvariant(),
            ApiPlanes.Prueba,
            enlace,
            false,
            cancellationToken);

        return new ActivacionPruebaComercialResultado(
            true,
            false,
            resultado.TokenEntrega,
            null);
    }

    public async Task<InicioSuscripcionComercialResultado>
        IniciarSuscripcionAsync(
            string nombre,
            string email,
            string plan,
            string baseUrl,
            CancellationToken cancellationToken)
    {
        string? planNormalizado = ApiPlanes.Normalizar(plan);

        if (planNormalizado is null ||
            (!string.Equals(
                 planNormalizado,
                 ApiPlanes.Starter,
                 StringComparison.Ordinal) &&
             !string.Equals(
                 planNormalizado,
                 ApiPlanes.Growth,
                 StringComparison.Ordinal)))
        {
            return FalloInicio(
                "El plan seleccionado no es válido.");
        }

        if (!_options.Enabled ||
            string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            return FalloInicio(
                "Los pagos todavía no están habilitados.");
        }

        decimal monto = string.Equals(
            planNormalizado,
            ApiPlanes.Starter,
            StringComparison.Ordinal)
                ? _options.StarterAmount
                : _options.GrowthAmount;

        string moneda = _options.CurrencyId.Trim().ToUpperInvariant();

        if (monto <= 0 || string.IsNullOrWhiteSpace(moneda))
        {
            return FalloInicio(
                "El precio de cobro del plan no está configurado.");
        }

        string emailNormalizado =
            email.Trim().ToLowerInvariant();

        SuscripcionComercial? activa =
            await _dbContext.SuscripcionesComerciales
                .AsNoTracking()
                .Where(item =>
                    item.Email == emailNormalizado &&
                    (item.Estado ==
                         EstadosSuscripcionComercial.Activa ||
                     item.Estado ==
                         EstadosSuscripcionComercial.Pendiente))
                .OrderByDescending(item =>
                    item.FechaCreacionUtc)
                .FirstOrDefaultAsync(cancellationToken);

        if (activa is not null)
        {
            if (activa.Estado ==
                    EstadosSuscripcionComercial.Pendiente &&
                string.Equals(
                    activa.Plan,
                    planNormalizado,
                    StringComparison.Ordinal) &&
                !string.IsNullOrWhiteSpace(
                    activa.MercadoPagoInitPoint))
            {
                return new InicioSuscripcionComercialResultado(
                    true,
                    activa.MercadoPagoInitPoint,
                    activa.ReferenciaPublica,
                    null);
            }

            return FalloInicio(
                "Ya existe una suscripción activa o pendiente para ese correo.");
        }

        DateTime nowUtc = DateTime.UtcNow;

        SuscripcionComercial suscripcion = new()
        {
            ReferenciaPublica = Guid.NewGuid(),
            Nombre = nombre.Trim(),
            Email = emailNormalizado,
            Plan = planNormalizado,
            Estado = EstadosSuscripcionComercial.Pendiente,
            Monto = monto,
            Moneda = moneda,
            FechaCreacionUtc = nowUtc,
            FechaActualizacionUtc = nowUtc
        };

        _dbContext.SuscripcionesComerciales.Add(
            suscripcion);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        string referencia =
            suscripcion.ReferenciaPublica.ToString("N");

        string backUrl =
            $"{baseUrl.TrimEnd('/')}/planes/retorno?referencia={referencia}";

        try
        {
            MercadoPagoSuscripcionCreada creada =
                await _mercadoPago.CrearPendienteAsync(
                    suscripcion.Email,
                    suscripcion.Plan,
                    referencia,
                    monto,
                    moneda,
                    backUrl,
                    cancellationToken);

            suscripcion.MercadoPagoPreapprovalId =
                creada.Id;
            suscripcion.MercadoPagoInitPoint =
                creada.InitPoint;
            suscripcion.ProximaRenovacionUtc =
                creada.ProximaRenovacionUtc;
            suscripcion.FechaActualizacionUtc =
                DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return new InicioSuscripcionComercialResultado(
                true,
                creada.InitPoint,
                suscripcion.ReferenciaPublica,
                null);
        }
        catch
        {
            suscripcion.Estado =
                EstadosSuscripcionComercial.Error;
            suscripcion.FechaActualizacionUtc =
                DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            return FalloInicio(
                "No fue posible iniciar el pago en este momento.");
        }
    }

    public async Task ProcesarWebhookAsync(
        string eventoProveedorId,
        string tipo,
        string accion,
        string recursoId,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        bool procesado = await _dbContext.WebhookComercialEventos
            .AnyAsync(
                item =>
                    item.Proveedor == "MercadoPago" &&
                    item.EventoProveedorId ==
                        eventoProveedorId,
                cancellationToken);

        if (procesado)
        {
            return;
        }

        SuscripcionComercial? suscripcion =
            tipo switch
            {
                "subscription_preapproval" =>
                    await ProcesarPreapprovalAsync(
                        recursoId,
                        cancellationToken),
                "subscription_authorized_payment" =>
                    await ProcesarPagoAutorizadoAsync(
                        recursoId,
                        baseUrl,
                        cancellationToken),
                "payment" =>
                    await ProcesarPagoAsync(
                        recursoId,
                        baseUrl,
                        cancellationToken),
                _ => null
            };

        _dbContext.WebhookComercialEventos.Add(
            new WebhookComercialEvento
            {
                SuscripcionComercialId =
                    suscripcion?.SuscripcionComercialId,
                Proveedor = "MercadoPago",
                EventoProveedorId = eventoProveedorId,
                Tipo = tipo,
                RecursoId = recursoId,
                Accion = accion,
                FechaProcesadoUtc = DateTime.UtcNow
            });

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task<int> AplicarSuspensionesVencidasAsync(
        CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;

        List<SuscripcionComercial> vencidas =
            await _dbContext.SuscripcionesComerciales
                .Include(item => item.Cliente)
                .Where(item =>
                    (item.Estado ==
                         EstadosSuscripcionComercial.Impaga &&
                     item.PeriodoGraciaHastaUtc.HasValue &&
                     item.PeriodoGraciaHastaUtc <= nowUtc) ||
                    (item.Estado ==
                         EstadosSuscripcionComercial.Cancelada &&
                     item.FechaFinAccesoUtc.HasValue &&
                     item.FechaFinAccesoUtc <= nowUtc))
                .ToListAsync(cancellationToken);

        foreach (SuscripcionComercial item in vencidas)
        {
            if (item.Cliente is not null)
            {
                item.Cliente.Habilitado = false;
            }

            if (item.Estado ==
                EstadosSuscripcionComercial.Impaga)
            {
                item.Estado =
                    EstadosSuscripcionComercial.Suspendida;
            }

            item.FechaActualizacionUtc = nowUtc;
        }

        if (vencidas.Count > 0)
        {
            await _dbContext.SaveChangesAsync(
                cancellationToken);
        }

        return vencidas.Count;
    }

    private async Task<SuscripcionComercial?>
        ProcesarPreapprovalAsync(
            string recursoId,
            CancellationToken cancellationToken)
    {
        MercadoPagoSuscripcionDetalle? detail =
            await _mercadoPago.ObtenerSuscripcionAsync(
                recursoId,
                cancellationToken);

        if (detail is null)
        {
            return null;
        }

        SuscripcionComercial? suscripcion =
            await BuscarSuscripcionAsync(
                detail.Id,
                detail.ReferenciaExterna,
                cancellationToken);

        if (suscripcion is null)
        {
            return null;
        }

        suscripcion.MercadoPagoPreapprovalId = detail.Id;
        suscripcion.ProximaRenovacionUtc =
            detail.ProximaRenovacionUtc;
        suscripcion.FechaActualizacionUtc = DateTime.UtcNow;

        if (string.Equals(
                detail.Estado,
                "canceled",
                StringComparison.OrdinalIgnoreCase))
        {
            bool yaCancelada = suscripcion.Estado ==
                EstadosSuscripcionComercial.Cancelada;

            suscripcion.Estado =
                EstadosSuscripcionComercial.Cancelada;
            suscripcion.FechaCancelacionUtc ??=
                DateTime.UtcNow;
            suscripcion.FechaFinAccesoUtc =
                detail.ProximaRenovacionUtc.HasValue &&
                detail.ProximaRenovacionUtc > DateTime.UtcNow
                    ? detail.ProximaRenovacionUtc
                    : DateTime.UtcNow;

            if (!yaCancelada)
            {
                await _correo.EnviarCancelacionAsync(
                    suscripcion.Email,
                    suscripcion.Plan,
                    suscripcion.FechaFinAccesoUtc,
                    cancellationToken);
            }
        }
        else if (string.Equals(
                     detail.Estado,
                     "paused",
                     StringComparison.OrdinalIgnoreCase))
        {
            suscripcion.Estado =
                EstadosSuscripcionComercial.Suspendida;

            if (suscripcion.Cliente is not null)
            {
                suscripcion.Cliente.Habilitado = false;
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return suscripcion;
    }

    private async Task<SuscripcionComercial?>
        ProcesarPagoAutorizadoAsync(
            string recursoId,
            string baseUrl,
            CancellationToken cancellationToken)
    {
        MercadoPagoPagoAutorizadoDetalle? pago =
            await _mercadoPago.ObtenerPagoAutorizadoAsync(
                recursoId,
                cancellationToken);

        if (pago is null)
        {
            return null;
        }

        SuscripcionComercial? suscripcion =
            await BuscarSuscripcionAsync(
                pago.PreapprovalId,
                pago.ReferenciaExterna,
                cancellationToken);

        if (suscripcion is null)
        {
            return null;
        }

        if (string.Equals(
                pago.EstadoPago,
                "approved",
                StringComparison.OrdinalIgnoreCase))
        {
            await ActivarPorPagoAprobadoAsync(
                suscripcion,
                pago.FechaUtc,
                baseUrl,
                cancellationToken);
        }
        else if (string.Equals(
                     pago.EstadoPago,
                     "rejected",
                     StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                     pago.EstadoPago,
                     "cancelled",
                     StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                     pago.EstadoPago,
                     "canceled",
                     StringComparison.OrdinalIgnoreCase))
        {
            await MarcarPagoImpagoAsync(
                suscripcion,
                cancellationToken);
        }

        return suscripcion;
    }

    private async Task<SuscripcionComercial?> ProcesarPagoAsync(
        string recursoId,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        MercadoPagoPagoDetalle? pago =
            await _mercadoPago.ObtenerPagoAsync(
                recursoId,
                cancellationToken);

        if (pago is null ||
            string.IsNullOrWhiteSpace(
                pago.ReferenciaExterna))
        {
            return null;
        }

        SuscripcionComercial? suscripcion =
            await BuscarSuscripcionAsync(
                null,
                pago.ReferenciaExterna,
                cancellationToken);

        if (suscripcion is null)
        {
            return null;
        }

        if (string.Equals(
                pago.Estado,
                "approved",
                StringComparison.OrdinalIgnoreCase))
        {
            await ActivarPorPagoAprobadoAsync(
                suscripcion,
                pago.FechaAprobacionUtc,
                baseUrl,
                cancellationToken);
        }
        else if (string.Equals(
                     pago.Estado,
                     "rejected",
                     StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                     pago.Estado,
                     "cancelled",
                     StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(
                     pago.Estado,
                     "canceled",
                     StringComparison.OrdinalIgnoreCase))
        {
            await MarcarPagoImpagoAsync(
                suscripcion,
                cancellationToken);
        }

        return suscripcion;
    }

    private async Task ActivarPorPagoAprobadoAsync(
        SuscripcionComercial suscripcion,
        DateTime? fechaPagoUtc,
        string baseUrl,
        CancellationToken cancellationToken)
    {
        bool primeraActivacion = suscripcion.Estado !=
            EstadosSuscripcionComercial.Activa;

        ProvisionamientoApiResultado provisionado =
            await _provisionamiento.ActivarPlanPagadoAsync(
                suscripcion.Nombre,
                suscripcion.Email,
                suscripcion.Plan,
                cancellationToken);

        suscripcion.ApiClienteId =
            provisionado.ApiClienteId;
        suscripcion.Estado =
            EstadosSuscripcionComercial.Activa;
        suscripcion.FechaUltimoPagoUtc =
            fechaPagoUtc ?? DateTime.UtcNow;
        suscripcion.PeriodoGraciaHastaUtc = null;
        suscripcion.FechaFinAccesoUtc = null;
        suscripcion.FechaActualizacionUtc =
            DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(
                suscripcion.MercadoPagoPreapprovalId))
        {
            MercadoPagoSuscripcionDetalle? detail =
                await _mercadoPago.ObtenerSuscripcionAsync(
                    suscripcion.MercadoPagoPreapprovalId,
                    cancellationToken);

            if (detail is not null)
            {
                suscripcion.ProximaRenovacionUtc =
                    detail.ProximaRenovacionUtc;
            }
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        if (primeraActivacion)
        {
            string? enlace = CrearEnlaceEntrega(
                baseUrl,
                provisionado.TokenEntrega);

            await _correo.EnviarAccesoListoAsync(
                suscripcion.Email,
                suscripcion.Plan,
                enlace,
                !provisionado.ClaveNueva,
                cancellationToken);
        }
    }

    private async Task MarcarPagoImpagoAsync(
        SuscripcionComercial suscripcion,
        CancellationToken cancellationToken)
    {
        bool notificar = suscripcion.Estado !=
            EstadosSuscripcionComercial.Impaga;

        suscripcion.Estado =
            EstadosSuscripcionComercial.Impaga;
        suscripcion.PeriodoGraciaHastaUtc =
            DateTime.UtcNow.AddDays(
                Math.Clamp(_options.GraceDays, 1, 30));
        suscripcion.FechaActualizacionUtc =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        if (notificar)
        {
            await _correo.EnviarProblemaPagoAsync(
                suscripcion.Email,
                suscripcion.Plan,
                cancellationToken);
        }
    }

    private async Task<SuscripcionComercial?>
        BuscarSuscripcionAsync(
            string? preapprovalId,
            string? referenciaExterna,
            CancellationToken cancellationToken)
    {
        SuscripcionComercial? suscripcion = null;

        if (!string.IsNullOrWhiteSpace(preapprovalId))
        {
            suscripcion = await _dbContext
                .SuscripcionesComerciales
                .Include(item => item.Cliente)
                .SingleOrDefaultAsync(
                    item =>
                        item.MercadoPagoPreapprovalId ==
                            preapprovalId,
                    cancellationToken);
        }

        if (suscripcion is not null ||
            string.IsNullOrWhiteSpace(referenciaExterna))
        {
            return suscripcion;
        }

        if (!Guid.TryParseExact(
                referenciaExterna,
                "N",
                out Guid referencia))
        {
            return null;
        }

        return await _dbContext.SuscripcionesComerciales
            .Include(item => item.Cliente)
            .SingleOrDefaultAsync(
                item =>
                    item.ReferenciaPublica == referencia,
                cancellationToken);
    }

    private static string? CrearEnlaceEntrega(
        string baseUrl,
        string? token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        return $"{baseUrl.TrimEnd('/')}/acceso-api/clave?token={Uri.EscapeDataString(token)}";
    }

    private static InicioSuscripcionComercialResultado FalloInicio(
        string mensaje)
    {
        return new InicioSuscripcionComercialResultado(
            false,
            null,
            null,
            mensaje);
    }
}
