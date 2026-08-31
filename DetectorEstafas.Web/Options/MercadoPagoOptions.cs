using DetectorEstafas.Web.Models.ApiComercial;

namespace DetectorEstafas.Web.Options;

public sealed class MercadoPagoOptions
{
    public const string SectionName = "MercadoPago";

    private decimal _starterAmount;
    private decimal _growthAmount;

    public bool Enabled { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public string CurrencyId { get; set; } = "ARS";

    public decimal UsdToArsExchangeRate { get; set; }

    public decimal StarterAmount
    {
        get => _starterAmount > 0
            ? _starterAmount
            : CalcularImporteArs(ApiPlanes.PrecioReferenciaUsdStarter);
        set => _starterAmount = value;
    }

    public decimal GrowthAmount
    {
        get => _growthAmount > 0
            ? _growthAmount
            : CalcularImporteArs(ApiPlanes.PrecioReferenciaUsdGrowth);
        set => _growthAmount = value;
    }

    public int GraceDays { get; set; } = 3;

    private decimal CalcularImporteArs(decimal precioUsd)
    {
        if (UsdToArsExchangeRate <= 0)
        {
            return 0;
        }

        return Math.Round(
            precioUsd * UsdToArsExchangeRate,
            0,
            MidpointRounding.AwayFromZero);
    }
}
