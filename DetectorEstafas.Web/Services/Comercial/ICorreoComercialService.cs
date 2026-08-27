namespace DetectorEstafas.Web.Services.Comercial;

public interface ICorreoComercialService
{
    Task EnviarAccesoListoAsync(
        string destinatario,
        string plan,
        string? enlaceEntregaClave,
        bool mantieneClaveExistente,
        CancellationToken cancellationToken = default);

    Task EnviarProblemaPagoAsync(
        string destinatario,
        string plan,
        CancellationToken cancellationToken = default);

    Task EnviarCancelacionAsync(
        string destinatario,
        string plan,
        DateTime? accesoHastaUtc,
        CancellationToken cancellationToken = default);
}
