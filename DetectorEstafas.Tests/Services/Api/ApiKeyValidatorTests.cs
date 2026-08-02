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
    public async Task ValidarYRegistrarConsumoAsync_ClaveConfigurada_ImportaHashYRegistraConsumo()
    {
        await using DetectorEstafasDbContext dbContext = CrearDbContext();

        ApiKeyValidator validator = new(
            dbContext,
            Options.Create(new ApiComercialOptions
            {
                Enabled = true,
                DefaultDailyQuota = 5,
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

        Assert.AreEqual(EstadoValidacionApiKey.Valida, result.Estado);
        Assert.AreEqual(1, result.ConsumidasHoy);
        Assert.AreEqual(1, await dbContext.ApiClientes.CountAsync());
        Assert.AreEqual(1, await dbContext.ApiClaves.CountAsync());
        Assert.AreEqual(32, (await dbContext.ApiClaves.SingleAsync()).Hash.Length);
        Assert.AreEqual(1, await dbContext.ApiConsumosDiarios.CountAsync());
    }

    [TestMethod]
    public async Task ValidarYRegistrarConsumoAsync_CuotaAlcanzada_DevuelveCuotaAgotada()
    {
        await using DetectorEstafasDbContext dbContext = CrearDbContext();

        ApiCliente client = new()
        {
            Nombre = "cliente-cuota",
            Plan = "Prueba",
            CuotaDiaria = 1,
            Habilitado = true
        };

        dbContext.ApiClientes.Add(client);
        await dbContext.SaveChangesAsync();

        ApiKeyValidator validator = new(
            dbContext,
            Options.Create(new ApiComercialOptions
            {
                Enabled = true,
                DefaultDailyQuota = 1,
                Keys =
                [
                    new ApiKeyOptions
                    {
                        Name = "cliente-cuota",
                        Key = "clave-cuota",
                        Enabled = true
                    }
                ]
            }));

        ResultadoValidacionApiKey first =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-cuota",
                CancellationToken.None);

        ResultadoValidacionApiKey second =
            await validator.ValidarYRegistrarConsumoAsync(
                "clave-cuota",
                CancellationToken.None);

        Assert.AreEqual(EstadoValidacionApiKey.Valida, first.Estado);
        Assert.AreEqual(EstadoValidacionApiKey.CuotaAgotada, second.Estado);
    }

    private static DetectorEstafasDbContext CrearDbContext()
    {
        DbContextOptions<DetectorEstafasDbContext> options =
            new DbContextOptionsBuilder<DetectorEstafasDbContext>()
                .UseInMemoryDatabase($"ApiKeyTests-{Guid.NewGuid()}")
                .Options;

        return new DetectorEstafasDbContext(options);
    }
}
