namespace DetectorEstafas.Web.Options;

public sealed class MercadoPagoOptions
{
    public const string SectionName = "MercadoPago";

    public bool Enabled { get; set; }

    public string AccessToken { get; set; } = string.Empty;

    public string WebhookSecret { get; set; } = string.Empty;

    public string CurrencyId { get; set; } = "ARS";

    public decimal StarterAmount { get; set; }

    public decimal GrowthAmount { get; set; }

    public int GraceDays { get; set; } = 3;
}
