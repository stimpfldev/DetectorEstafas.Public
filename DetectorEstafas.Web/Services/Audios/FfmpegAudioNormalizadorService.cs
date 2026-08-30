namespace DetectorEstafas.Web.Services.Audios;

public class FfmpegAudioNormalizadorService
    : IAudioNormalizadorService
{
    public Task NormalizarAWavAsync(
        string rutaOrigen,
        string rutaDestino,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "La normalización de audio no está disponible en este entorno.");
    }
}
