namespace DetectorEstafas.Web.Models.ApiComercial;

public sealed class SuscripcionComercial
{
    public int SuscripcionComercialId { get; set; }

    public Guid ReferenciaPublica { get; set; } = Guid.NewGuid();

    public string Nombre { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Plan { get; set; } = string.Empty;

    public string Estado { get; set; } =
        EstadosSuscripcionComercial.Pendiente;

    public string? MercadoPagoPreapprovalId { get; set; }

    public string? MercadoPagoInitPoint { get; set; }

    public decimal Monto { get; set; }

    public string Moneda { get; set; } = string.Empty;

    public int? ApiClienteId { get; set; }

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public DateTime FechaActualizacionUtc { get; set; } = DateTime.UtcNow;

    public DateTime? FechaUltimoPagoUtc { get; set; }

    public DateTime? ProximaRenovacionUtc { get; set; }

    public DateTime? PeriodoGraciaHastaUtc { get; set; }

    public DateTime? FechaCancelacionUtc { get; set; }

    public DateTime? FechaFinAccesoUtc { get; set; }

    public ApiCliente? Cliente { get; set; }

    public ICollection<WebhookComercialEvento> Eventos { get; set; } =
        new List<WebhookComercialEvento>();
}
