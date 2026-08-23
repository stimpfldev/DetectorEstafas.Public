using FFMpegCore;

namespace DetectorEstafas.Web.Services.Audios;

public class FfmpegAudioNormalizadorService
    : IAudioNormalizadorService
{
    public async Task NormalizarAWavAsync(
        string rutaOrigen,
        string rutaDestino,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaOrigen);
        ArgumentException.ThrowIfNullOrWhiteSpace(rutaDestino);

        if (!File.Exists(rutaOrigen))
        {
            throw new FileNotFoundException(
                "No se encontró el audio a normalizar.",
                rutaOrigen);
        }

        await FFMpegArguments
            .FromFileInput(rutaOrigen)
            .OutputToFile(
                rutaDestino,
                overwrite: true,
                options => options.WithCustomArgument(
                    "-vn -ac 1 -ar 16000 -c:a pcm_s16le"))
            .ProcessAsynchronously();
    }
}