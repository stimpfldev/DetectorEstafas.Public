namespace DetectorEstafas.Web.Services.Comercial;

public sealed record ActivacionPruebaComercialResultado(
    bool Exito,
    bool YaExistia,
    string? TokenEntrega,
    string? Mensaje);

public sealed record InicioSuscripcionComercialResultado(
    bool Exito,
    string? UrlPago,
    Guid? ReferenciaPublica,
    string? Mensaje);

public interface IComercializacionApiService
{
    Task<ActivacionPruebaComercialResultado> ActivarPruebaAsync(
        string nombre,
        string email,
        string baseUrl,
        CancellationToken cancellationToken);

    Task<InicioSuscripcionComercialResultado> IniciarSuscripcionAsync(
        string nombre,
        string email,
        string plan,
        string baseUrl,
        CancellationToken cancellationToken);

    Task ProcesarWebhookAsync(
        string eventoProveedorId,
        string tipo,
        string accion,
        string recursoId,
        string baseUrl,
        CancellationToken cancellationToken);

    Task<int> AplicarSuspensionesVencidasAsync(
        CancellationToken cancellationToken);
}
