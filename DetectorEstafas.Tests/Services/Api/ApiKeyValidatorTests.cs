using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.Api;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Tests.Services.Api;

[TestClass]
public sealed class ApiKeyValidatorTests
{
    [TestMethod]
    public async Task ValidarYRegistrarConsumoAsync_ClaveConfigurada_ImportaPruebaYRegistraConsumo()
    {
        await using DetectorEstafasDbContext dbContext =
            CrearDbContext();

        ApiKeyValidator validator = new(
            dbContext,
            Options.Create(new ApiComercialOptions
            {
                Enabled = true,
                Keys =
                [
                    new ApiKeyOptions
                    {
                        Name = "cliente-test",
                        Key = "clave-segura-de-prueba",
                        Enabled = true
                    }
                ]
            }));

        ResultadoValidacionApiKey result =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-segura-de-prueba",
                CancellationToken.None);

        Assert.AreEqual(
            EstadoValidacionApiKey.Valida,
            result.Estado);

        Assert.AreEqual(
            PeriodoCuotaApi.Diario,
            result.Periodo);

        Assert.AreEqual(
            ApiPlanes.CuotaDiariaPrueba,
            result.Limite);

        Assert.AreEqual(
            1,
            result.ConsumidasPeriodo);

        ApiCliente client =
            await dbContext.ApiClientes.SingleAsync();

        Assert.AreEqual(
            ApiPlanes.Prueba,
            client.Plan);

        Assert.AreEqual(
            ApiPlanes.CuotaDiariaPrueba,
            client.CuotaDiaria);

        Assert.IsNull(client.CuotaMensual);

        Assert.AreEqual(
            1,
            await dbContext.ApiClaves.CountAsync());

        Assert.AreEqual(
            32,
            (await dbContext.ApiClaves
                .SingleAsync()).Hash.Length);

        Assert.AreEqual(
            1,
            await dbContext.ApiConsumosDiarios
                .CountAsync());
    }

    [TestMethod]
    public async Task ValidarYRegistrarConsumoAsync_PruebaConCuotaDiariaAlcanzada_DevuelveCuotaAgotada()
    {
        await using DetectorEstafasDbContext dbContext =
            CrearDbContext();

        ApiCliente client = new()
        {
            Nombre = "cliente-cuota",
            Plan = ApiPlanes.Prueba,
            CuotaDiaria = 1,
            CuotaMensual = null,
            Habilitado = true,
            FechaInicioPlanUtc = DateTime.UtcNow
        };

        dbContext.ApiClientes.Add(client);
        await dbContext.SaveChangesAsync();

        ApiKeyValidator validator =
            CrearValidator(
                dbContext,
                "cliente-cuota",
                "clave-cuota");

        ResultadoValidacionApiKey first =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-cuota",
                CancellationToken.None);

        ResultadoValidacionApiKey second =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-cuota",
                CancellationToken.None);

        Assert.AreEqual(
            EstadoValidacionApiKey.Valida,
            first.Estado);

        Assert.AreEqual(
            EstadoValidacionApiKey.CuotaAgotada,
            second.Estado);

        Assert.AreEqual(
            PeriodoCuotaApi.Diario,
            second.Periodo);
    }

    [TestMethod]
    public async Task ValidarYRegistrarConsumoAsync_StarterUsaCuotaMensualYNoCuotaDiaria()
    {
        await using DetectorEstafasDbContext dbContext =
            CrearDbContext();

        ApiCliente client = new()
        {
            Nombre = "cliente-starter",
            Plan = ApiPlanes.Starter,
            CuotaDiaria = 0,
            CuotaMensual = 2,
            Habilitado = true,
            FechaInicioPlanUtc =
                DateTime.UtcNow.AddDays(-1)
        };

        dbContext.ApiClientes.Add(client);
        await dbContext.SaveChangesAsync();

        ApiKeyValidator validator =
            CrearValidator(
                dbContext,
                "cliente-starter",
                "clave-starter");

        ResultadoValidacionApiKey first =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-starter",
                CancellationToken.None);

        ResultadoValidacionApiKey second =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-starter",
                CancellationToken.None);

        ResultadoValidacionApiKey third =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-starter",
                CancellationToken.None);

        Assert.AreEqual(
            EstadoValidacionApiKey.Valida,
            first.Estado);

        Assert.AreEqual(
            EstadoValidacionApiKey.Valida,
            second.Estado);

        Assert.AreEqual(
            EstadoValidacionApiKey.CuotaAgotada,
            third.Estado);

        Assert.AreEqual(
            PeriodoCuotaApi.Mensual,
            third.Periodo);

        Assert.AreEqual(
            2,
            third.Limite);

        Assert.AreEqual(
            2,
            third.ConsumidasPeriodo);
    }

    [TestMethod]
    public async Task ValidarYRegistrarConsumoAsync_ConsumoMesAnterior_NoAfectaCuotaMensualActual()
    {
        await using DetectorEstafasDbContext dbContext =
            CrearDbContext();

        ApiCliente client = new()
        {
            Nombre = "cliente-reset-mensual",
            Plan = ApiPlanes.Starter,
            CuotaDiaria = 0,
            CuotaMensual = 1,
            Habilitado = true,
            FechaInicioPlanUtc =
                DateTime.UtcNow.AddMonths(-2)
        };

        dbContext.ApiClientes.Add(client);
        await dbContext.SaveChangesAsync();

        DateOnly todayUtc =
            DateOnly.FromDateTime(DateTime.UtcNow);

        DateOnly inicioMesUtc =
            new(todayUtc.Year, todayUtc.Month, 1);

        dbContext.ApiConsumosDiarios.Add(
            new ApiConsumoDiario
            {
                ApiClienteId =
                    client.ApiClienteId,
                FechaUtc =
                    inicioMesUtc.AddDays(-1),
                CantidadSolicitudes = 1,
                UltimaSolicitudUtc =
                    DateTime.UtcNow.AddDays(-1)
            });

        await dbContext.SaveChangesAsync();

        ApiKeyValidator validator =
            CrearValidator(
                dbContext,
                "cliente-reset-mensual",
                "clave-reset-mensual");

        ResultadoValidacionApiKey result =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-reset-mensual",
                CancellationToken.None);

        Assert.AreEqual(
            EstadoValidacionApiKey.Valida,
            result.Estado);

        Assert.AreEqual(
            PeriodoCuotaApi.Mensual,
            result.Periodo);

        Assert.AreEqual(
            1,
            result.ConsumidasPeriodo);
    }

    [TestMethod]
    public async Task ValidarYRegistrarConsumoAsync_PruebaExpirada_DevuelvePruebaExpirada()
    {
        await using DetectorEstafasDbContext dbContext =
            CrearDbContext();

        DateTime startUtc =
            DateTime.UtcNow.AddDays(-20);

        ApiCliente client = new()
        {
            Nombre = "cliente-expirado",
            Plan = ApiPlanes.Prueba,
            CuotaDiaria = 10,
            Habilitado = true,
            FechaCreacionUtc = startUtc,
            FechaInicioPlanUtc = startUtc
        };

        dbContext.ApiClientes.Add(client);
        await dbContext.SaveChangesAsync();

        ApiKeyValidator validator = new(
            dbContext,
            Options.Create(new ApiComercialOptions
            {
                Enabled = true,
                TrialDays = 14,
                Keys =
                [
                    new ApiKeyOptions
                    {
                        Name =
                            "cliente-expirado",
                        Key =
                            "clave-expirada",
                        Enabled = true
                    }
                ]
            }));

        ResultadoValidacionApiKey result =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-expirada",
                CancellationToken.None);

        Assert.AreEqual(
            EstadoValidacionApiKey.PruebaExpirada,
            result.Estado);
    }

    [TestMethod]
    public async Task ValidarYRegistrarConsumoAsync_ClienteDeshabilitado_NoSeReactivaDesdeConfiguracion()
    {
        await using DetectorEstafasDbContext dbContext =
            CrearDbContext();

        ApiCliente client = new()
        {
            Nombre = "cliente-deshabilitado",
            Plan = ApiPlanes.Starter,
            CuotaDiaria = 0,
            CuotaMensual =
                ApiPlanes.CuotaMensualStarter,
            Habilitado = false
        };

        dbContext.ApiClientes.Add(client);
        await dbContext.SaveChangesAsync();

        ApiKeyValidator validator =
            CrearValidator(
                dbContext,
                "cliente-deshabilitado",
                "clave-deshabilitada");

        ResultadoValidacionApiKey result =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-deshabilitada",
                CancellationToken.None);

        Assert.AreEqual(
            EstadoValidacionApiKey.Invalida,
            result.Estado);
    }

    private static ApiKeyValidator CrearValidator(
        DetectorEstafasDbContext dbContext,
        string clientName,
        string key)
    {
        return new ApiKeyValidator(
            dbContext,
            Options.Create(new ApiComercialOptions
            {
                Enabled = true,
                Keys =
                [
                    new ApiKeyOptions
                    {
                        Name = clientName,
                        Key = key,
                        Enabled = true
                    }
                ]
            }));
    }

    private static DetectorEstafasDbContext
        CrearDbContext()
    {
        DbContextOptions<DetectorEstafasDbContext>
            options =
                new DbContextOptionsBuilder<
                    DetectorEstafasDbContext>()
                    .UseInMemoryDatabase(
                        $"ApiKeyTests-{Guid.NewGuid()}")
                    .Options;

        return new DetectorEstafasDbContext(
            options);
    }
}
