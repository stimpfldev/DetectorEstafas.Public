using DetectorEstafas.Web.Models.Audios;

namespace DetectorEstafas.Web.Services.Audios;

public class AudioTemporalService : IAudioTemporalService
{
    public Task<ResultadoAudioTemporal> ProcesarAsync(
        IFormFile archivo,
        CancellationToken cancellationToken)
    {
        throw new AudioInvalidoException(
            "El análisis de audio no está disponible en este entorno.");
    }
}
