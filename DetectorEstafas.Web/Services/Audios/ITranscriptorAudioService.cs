using DetectorEstafas.Web.Models.Audios;

namespace DetectorEstafas.Web.Services.Audios;

public interface ITranscriptorAudioService
{
    Task<ResultadoTranscripcionAudio> TranscribirAsync(
        string rutaAudio,
        string extension,
        CancellationToken cancellationToken);
}
