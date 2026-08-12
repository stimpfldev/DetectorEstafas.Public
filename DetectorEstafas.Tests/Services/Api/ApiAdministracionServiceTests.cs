using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Services.Api.Administracion;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafas.Tests.Services.Api;

[TestClass]
public sealed class ApiAdministracionServiceTests
{
    [TestMethod]
    public async Task CambiarEstadoClienteAsync_InvierteEstado()
    {
        await using DetectorEstafasDbContext context =
            CrearContexto();

        ApiCliente client = new()
        {
            Nombre = "cliente",
            Habilitado = true
        };

        context.ApiClientes.Add(client);
        await context.SaveChangesAsync();

        ApiAdministracionService service =
            new(context);

        bool changed =
            await service.CambiarEstadoClienteAsync(
                client.ApiClienteId,
                CancellationToken.None);

        Assert.IsTrue(changed);
        Assert.IsFalse(client.Habilitado);
    }

    [TestMethod]
    public async Task ActualizarPlanClienteAsync_PruebaAStarter_Aplica5000MensualesYReiniciaConsumoDelDia()
    {
        await using DetectorEstafasDbContext context =
            CrearContexto();

        DateTime nowUtc = DateTime.UtcNow;

        ApiCliente client = new()
        {
            Nombre = "cliente-plan",
            Plan = ApiPlanes.Prueba,
            CuotaDiaria =
                ApiPlanes.CuotaDiariaPrueba,
            FechaInicioPlanUtc =
                nowUtc.AddDays(-2)
        };

        context.ApiClientes.Add(client);
        await context.SaveChangesAsync();

        context.ApiConsumosDiarios.Add(
            new ApiConsumoDiario
            {
                ApiClienteId =
                    client.ApiClienteId,
                FechaUtc =
                    DateOnly.FromDateTime(nowUtc),
                CantidadSolicitudes = 25,
                UltimaSolicitudUtc = nowUtc
            });

        await context.SaveChangesAsync();

        ApiAdministracionService service =
            new(context);

        bool updated =
            await service.ActualizarPlanClienteAsync(
                client.ApiClienteId,
                ApiPlanes.Starter,
                null,
                CancellationToken.None);

        Assert.IsTrue(updated);

        Assert.AreEqual(
            ApiPlanes.Starter,
            client.Plan);

        Assert.AreEqual(
            0,
            client.CuotaDiaria);

        Assert.AreEqual(
            ApiPlanes.CuotaMensualStarter,
            client.CuotaMensual);

        Assert.IsNotNull(
            client.FechaInicioPlanUtc);

        Assert.AreEqual(
            0,
            await context.ApiConsumosDiarios
                .CountAsync());
    }

    [TestMethod]
    public async Task ActualizarPlanClienteAsync_StarterAGrowth_ConservaConsumoYAplica25000Mensuales()
    {
        await using DetectorEstafasDbContext context =
            CrearContexto();

        DateTime nowUtc = DateTime.UtcNow;

        ApiCliente client = new()
        {
            Nombre = "cliente-growth",
            Plan = ApiPlanes.Starter,
            CuotaDiaria = 0,
            CuotaMensual =
                ApiPlanes.CuotaMensualStarter,
            FechaInicioPlanUtc =
                nowUtc.AddDays(-3)
        };

        context.ApiClientes.Add(client);
        await context.SaveChangesAsync();

        context.ApiConsumosDiarios.Add(
            new ApiConsumoDiario
            {
                ApiClienteId =
                    client.ApiClienteId,
                FechaUtc =
                    DateOnly.FromDateTime(nowUtc),
                CantidadSolicitudes = 100,
                UltimaSolicitudUtc = nowUtc
            });

        await context.SaveChangesAsync();

        ApiAdministracionService service =
            new(context);

        bool updated =
            await service.ActualizarPlanClienteAsync(
                client.ApiClienteId,
                ApiPlanes.Growth,
                null,
                CancellationToken.None);

        Assert.IsTrue(updated);

        Assert.AreEqual(
            ApiPlanes.Growth,
            client.Plan);

        Assert.AreEqual(
            ApiPlanes.CuotaMensualGrowth,
            client.CuotaMensual);

        Assert.AreEqual(
            1,
            await context.ApiConsumosDiarios
                .CountAsync());
    }

    [TestMethod]
    public async Task ActualizarPlanClienteAsync_AMedida_AceptaCuotaMensualPersonalizada()
    {
        await using DetectorEstafasDbContext context =
            CrearContexto();

        ApiCliente client = new()
        {
            Nombre = "cliente-medida",
            Plan = ApiPlanes.Starter,
            CuotaDiaria = 0,
            CuotaMensual =
                ApiPlanes.CuotaMensualStarter
        };

        context.ApiClientes.Add(client);
        await context.SaveChangesAsync();

        ApiAdministracionService service =
            new(context);

        bool updated =
            await service.ActualizarPlanClienteAsync(
                client.ApiClienteId,
                ApiPlanes.AMedida,
                75_000,
                CancellationToken.None);

        Assert.IsTrue(updated);

        Assert.AreEqual(
            ApiPlanes.AMedida,
            client.Plan);

        Assert.AreEqual(
            75_000,
            client.CuotaMensual);
    }

    [TestMethod]
    public async Task ActualizarPlanClienteAsync_AMedidaSinCuota_DevuelveFalse()
    {
        await using DetectorEstafasDbContext context =
            CrearContexto();

        ApiCliente client = new()
        {
            Nombre = "cliente-medida-invalida"
        };

        context.ApiClientes.Add(client);
        await context.SaveChangesAsync();

        ApiAdministracionService service =
            new(context);

        bool updated =
            await service.ActualizarPlanClienteAsync(
                client.ApiClienteId,
                ApiPlanes.AMedida,
                null,
                CancellationToken.None);

        Assert.IsFalse(updated);
    }

    [TestMethod]
    public async Task RevocarClaveAsync_DeshabilitaYRegistraFecha()
    {
        await using DetectorEstafasDbContext context =
            CrearContexto();

        ApiCliente client = new()
        {
            Nombre = "cliente"
        };

        ApiClave key = new()
        {
            Cliente = client,
            Prefijo = "12345678",
            Hash = new byte[32],
            Habilitada = true
        };

        context.ApiClaves.Add(key);
        await context.SaveChangesAsync();

        ApiAdministracionService service =
            new(context);

        bool revoked =
            await service.RevocarClaveAsync(
                key.ApiClaveId,
                CancellationToken.None);

        Assert.IsTrue(revoked);
        Assert.IsFalse(key.Habilitada);
        Assert.IsNotNull(
            key.FechaRevocacionUtc);
    }

    private static DetectorEstafasDbContext
        CrearContexto()
    {
        DbContextOptions<DetectorEstafasDbContext>
            options =
                new DbContextOptionsBuilder<
                    DetectorEstafasDbContext>()
                    .UseInMemoryDatabase(
                        $"ApiAdmin-{Guid.NewGuid()}")
                    .Options;

        return new DetectorEstafasDbContext(
            options);
    }
}
