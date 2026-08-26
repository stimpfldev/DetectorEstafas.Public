namespace DetectorEstafas.Web.Services.Comercial.MercadoPago;

public sealed record MercadoPagoPlanSuscripcionCreado(
    string Id,
    string InitPoint,
    string Estado);

public sealed record MercadoPagoSuscripcionDetalle(
    string Id,
    string Estado,
    string ReferenciaExterna,
    string? PreapprovalPlanId,
    DateTime? ProximaRenovacionUtc);

public sealed record MercadoPagoPagoAutorizadoDetalle(
    string Id,
    string PreapprovalId,
    string ReferenciaExterna,
    string? EstadoPago,
    DateTime? FechaUtc);

public sealed record MercadoPagoPagoDetalle(
    string Id,
    string ReferenciaExterna,
    string Estado,
    DateTime? FechaAprobacionUtc);

public interface IMercadoPagoSuscripcionService
{
    Task<MercadoPagoPlanSuscripcionCreado> CrearPlanPendienteAsync(
        string plan,
        decimal monto,
        string moneda,
        string backUrl,
        CancellationToken cancellationToken);

    Task<MercadoPagoSuscripcionDetalle?> ObtenerSuscripcionAsync(
        string preapprovalId,
        CancellationToken cancellationToken);

    Task<MercadoPagoPagoAutorizadoDetalle?> ObtenerPagoAutorizadoAsync(
        string authorizedPaymentId,
        CancellationToken cancellationToken);

    Task<MercadoPagoPagoDetalle?> ObtenerPagoAsync(
        string paymentId,
        CancellationToken cancellationToken);
}
