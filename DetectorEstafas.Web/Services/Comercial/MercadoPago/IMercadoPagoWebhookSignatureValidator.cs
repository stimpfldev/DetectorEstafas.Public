namespace DetectorEstafas.Web.Services.Comercial.MercadoPago;

public interface IMercadoPagoWebhookSignatureValidator
{
    bool EsValida(
        string? xSignature,
        string? xRequestId,
        string? dataId);
}
