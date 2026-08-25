namespace DetectorEstafas.Web.Models.ApiComercial;

public sealed class WebhookComercialEvento
{
    public int WebhookComercialEventoId { get; set; }

    public int? SuscripcionComercialId { get; set; }

    public string Proveedor { get; set; } = "MercadoPago";

    public string EventoProveedorId { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string RecursoId { get; set; } = string.Empty;

    public string Accion { get; set; } = string.Empty;

    public DateTime FechaProcesadoUtc { get; set; } = DateTime.UtcNow;

    public SuscripcionComercial? Suscripcion { get; set; }
}
