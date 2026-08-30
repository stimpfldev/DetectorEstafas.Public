using DetectorEstafas.Web.Models.Audios;

namespace DetectorEstafas.Web.Services.Audios;

public class WhisperTranscriptorAudioService
    : ITranscriptorAudioService
{
    public Task<ResultadoTranscripcionAudio> TranscribirAsync(
        string rutaAudio,
        string extension,
        CancellationToken cancellationToken)
    {
        throw new NotSupportedException(
            "La transcripción de audio no está disponible en este entorno.");
    }
}
