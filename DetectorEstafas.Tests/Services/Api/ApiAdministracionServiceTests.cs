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
        await using DetectorEstafasDbContext context = CrearContexto();
        ApiCliente client = new() { Nombre = "cliente", Habilitado = true };
        context.ApiClientes.Add(client);
        await context.SaveChangesAsync();

        ApiAdministracionService service = new(context);

        bool changed = await service.CambiarEstadoClienteAsync(
            client.ApiClienteId,
            CancellationToken.None);

        Assert.IsTrue(changed);
        Assert.IsFalse(client.Habilitado);
    }


    [TestMethod]
    public async Task ActualizarPlanClienteAsync_PasaDePruebaAComercialYCambiaCuota()
    {
        await using DetectorEstafasDbContext context = CrearContexto();
        ApiCliente client = new()
        {
            Nombre = "cliente-plan",
            Plan = "Prueba",
            CuotaDiaria = 10
        };

        context.ApiClientes.Add(client);
        await context.SaveChangesAsync();

        ApiAdministracionService service = new(context);

        bool updated = await service.ActualizarPlanClienteAsync(
            client.ApiClienteId,
            "Comercial",
            500,
            CancellationToken.None);

        Assert.IsTrue(updated);
        Assert.AreEqual("Comercial", client.Plan);
        Assert.AreEqual(500, client.CuotaDiaria);
    }

    [TestMethod]
    public async Task RevocarClaveAsync_DeshabilitaYRegistraFecha()
    {
        await using DetectorEstafasDbContext context = CrearContexto();
        ApiCliente client = new() { Nombre = "cliente" };
        ApiClave key = new()
        {
            Cliente = client,
            Prefijo = "12345678",
            Hash = new byte[32],
            Habilitada = true
        };

        context.ApiClaves.Add(key);
        await context.SaveChangesAsync();

        ApiAdministracionService service = new(context);

        bool revoked = await service.RevocarClaveAsync(
            key.ApiClaveId,
            CancellationToken.None);

        Assert.IsTrue(revoked);
        Assert.IsFalse(key.Habilitada);
        Assert.IsNotNull(key.FechaRevocacionUtc);
    }

    private static DetectorEstafasDbContext CrearContexto()
    {
        DbContextOptions<DetectorEstafasDbContext> options =
            new DbContextOptionsBuilder<DetectorEstafasDbContext>()
                .UseInMemoryDatabase($"ApiAdmin-{Guid.NewGuid()}")
                .Options;

        return new DetectorEstafasDbContext(options);
    }
}
