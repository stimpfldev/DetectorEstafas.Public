using DetectorEstafas.Web.Options;

namespace DetectorEstafas.Tests.Services.Comercial;

[TestClass]
public sealed class MercadoPagoOptionsTests
{
    [TestMethod]
    public void TipoCambio_Configurable_CalculaImportesArs()
    {
        MercadoPagoOptions options = new()
        {
            UsdToArsExchangeRate = 1_500m
        };

        Assert.AreEqual(28_500m, options.StarterAmount);
        Assert.AreEqual(52_500m, options.GrowthAmount);
    }

    [TestMethod]
    public void ImportesExplicitos_TienenPrioridadSobreTipoCambio()
    {
        MercadoPagoOptions options = new()
        {
            UsdToArsExchangeRate = 1_500m,
            StarterAmount = 30_000m,
            GrowthAmount = 55_000m
        };

        Assert.AreEqual(30_000m, options.StarterAmount);
        Assert.AreEqual(55_000m, options.GrowthAmount);
    }
}
