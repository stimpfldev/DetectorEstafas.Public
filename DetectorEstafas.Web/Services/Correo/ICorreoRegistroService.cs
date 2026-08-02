namespace DetectorEstafas.Web.Services.Correo;

public interface ICorreoRegistroService
{
    Task EnviarConfirmacionAsync(
        string destinatario,
        string enlaceConfirmacion,
        CancellationToken cancellationToken = default);
}
