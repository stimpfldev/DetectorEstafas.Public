using DetectorEstafas.Web.Models.Audios;
using DetectorEstafas.Web.Options;
using FFMpegCore;
using Microsoft.Extensions.Options;
using NAudio.Wave;
namespace DetectorEstafas.Web.Services.Audios;

public class AudioTemporalService : IAudioTemporalService
{
    private static readonly HashSet<string> ExtensionesPermitidas =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".wav",
            ".ogg",
            ".opus",
            ".m4a",
            ".aac"
        };

    private readonly AudioOptions _options;
    private readonly string _directorioTemporal;
    private readonly ITranscriptorAudioService _transcriptor;
    private readonly IAudioNormalizadorService _normalizador;

    public AudioTemporalService(
        IOptions<AudioOptions> options,
        IWebHostEnvironment environment,
        ITranscriptorAudioService transcriptor,
        IAudioNormalizadorService normalizador)
    {
        _options = options.Value;
        _transcriptor = transcriptor;
        _normalizador = normalizador;

        string carpetaConfigurada =
            _options.TemporaryFolderName
                .Replace('/', Path.DirectorySeparatorChar);

        _directorioTemporal = Path.Combine(
            Path.GetTempPath(),
            carpetaConfigurada);

        Directory.CreateDirectory(_directorioTemporal);
        LimpiarTemporalesAbandonados();
    }

    public async Task<ResultadoAudioTemporal> ProcesarAsync(
        IFormFile archivo,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            throw new AudioInvalidoException(
                "El análisis de audio no está disponible en este entorno.");
        }

        ValidarMetadatos(archivo);

        string extension =
            Path.GetExtension(archivo.FileName)
                .ToLowerInvariant();

        string rutaTemporal = Path.Combine(
            _directorioTemporal,
            $"{Guid.NewGuid():N}{extension}");

        string rutaNormalizada = Path.Combine(
            _directorioTemporal,
            $"{Guid.NewGuid():N}.wav");

        try
        {
            await using (FileStream destino = new(
                rutaTemporal,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous |
                FileOptions.SequentialScan))
            {
                await archivo.CopyToAsync(
                    destino,
                    cancellationToken);
            }

            await ValidarAudioRealAsync(rutaTemporal);

            await _normalizador.NormalizarAWavAsync(
                rutaTemporal,
                rutaNormalizada,
                cancellationToken);

            ResultadoTranscripcionAudio transcripcion =
                await _transcriptor.TranscribirAsync(
                    rutaNormalizada,
                    ".wav",
                    cancellationToken);

            return new ResultadoAudioTemporal
            {
                NombreMostrado = "audio-procesado.wav",
                TipoContenido = "audio/wav",
                TamanoBytes = archivo.Length,
                TextoTranscripto = transcripcion.Texto,
                TextoFueTruncado =
                    transcripcion.TextoFueTruncado
            };
        }
        finally
        {
            EliminarSiExiste(rutaTemporal);
            EliminarSiExiste(rutaNormalizada);
        }
    }

    private void ValidarMetadatos(IFormFile archivo)
    {
        if (archivo.Length <= 0)
        {
            throw new AudioInvalidoException(
                "El archivo de audio está vacío.");
        }

        if (archivo.Length > _options.MaxFileSizeBytes)
        {
            throw new AudioInvalidoException(
                "El audio supera el límite permitido de 10 MB.");
        }

        string extension =
            Path.GetExtension(archivo.FileName);

        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new AudioInvalidoException(
                "Solo se permiten audios MP3, WAV, OGG, OPUS, M4A o AAC.");
        }
    }

    private static async Task ValidarAudioRealAsync(
        string ruta)
    {
        try
        {
            var informacion =
                await FFProbe.AnalyseAsync(ruta);

            if (!informacion.AudioStreams.Any())
            {
                throw new AudioInvalidoException(
                    "El archivo no contiene una pista de audio válida.");
            }
        }
        catch (AudioInvalidoException)
        {
            throw;
        }
        catch
        {
            throw new AudioInvalidoException(
                "El contenido del archivo no corresponde a un audio válido.");
        }
    }

    private void LimpiarTemporalesAbandonados()
    {
        DateTime limite =
            DateTime.UtcNow.AddMinutes(
                -Math.Max(
                    1,
                    _options.RetentionMinutes));

        foreach (string ruta in
                 Directory.EnumerateFiles(
                     _directorioTemporal))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(ruta) < limite)
                {
                    File.Delete(ruta);
                }
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }

    private static void EliminarSiExiste(
        string ruta)
    {
        try
        {
            if (File.Exists(ruta))
            {
                File.Delete(ruta);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
