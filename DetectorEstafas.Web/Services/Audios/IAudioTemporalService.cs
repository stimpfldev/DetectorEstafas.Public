using DetectorEstafas.Web.Models.Audios;

namespace DetectorEstafas.Web.Services.Audios;

public interface IAudioTemporalService
{
    Task<ResultadoAudioTemporal> ProcesarAsync(
        IFormFile archivo,
        CancellationToken cancellationToken);
}
