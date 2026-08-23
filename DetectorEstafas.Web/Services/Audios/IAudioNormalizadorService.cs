namespace DetectorEstafas.Web.Services.Audios;

public interface IAudioNormalizadorService
{
    Task NormalizarAWavAsync(
        string rutaOrigen,
        string rutaDestino,
        CancellationToken cancellationToken);
}