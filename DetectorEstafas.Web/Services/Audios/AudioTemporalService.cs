using DetectorEstafas.Web.Models.Audios;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Audios;

public class AudioTemporalService : IAudioTemporalService
{
    private static readonly HashSet<string> ExtensionesPermitidas =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".mp3",
            ".wav"
        };

    private static readonly HashSet<string> TiposPermitidos =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "audio/mpeg",
            "audio/mp3",
            "audio/wav",
            "audio/x-wav",
            "audio/wave"
        };

    private readonly AudioOptions _options;
    private readonly string _directorioTemporal;
    private readonly ITranscriptorAudioService _transcriptor;

    public AudioTemporalService(
        IOptions<AudioOptions> options,
        IWebHostEnvironment environment,
        ITranscriptorAudioService transcriptor)
    {
        _options = options.Value;
        _transcriptor = transcriptor;

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
        ValidarMetadatos(archivo);

        string extension =
            Path.GetExtension(archivo.FileName)
                .ToLowerInvariant();

        string rutaTemporal = Path.Combine(
            _directorioTemporal,
            $"{Guid.NewGuid():N}{extension}");

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

            await ValidarFirmaAsync(
                rutaTemporal,
                extension,
                cancellationToken);

            ResultadoTranscripcionAudio transcripcion =
                await _transcriptor.TranscribirAsync(
                    rutaTemporal,
                    extension,
                    cancellationToken);

            return new ResultadoAudioTemporal
            {
                NombreMostrado =
                    extension == ".mp3"
                        ? "audio-procesado.mp3"
                        : "audio-procesado.wav",
                TipoContenido =
                    extension == ".mp3"
                        ? "audio/mpeg"
                        : "audio/wav",
                TamanoBytes = archivo.Length,
                TextoTranscripto = transcripcion.Texto,
                TextoFueTruncado =
                    transcripcion.TextoFueTruncado
            };
        }
        finally
        {
            EliminarSiExiste(rutaTemporal);
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

        string extension = Path.GetExtension(archivo.FileName);

        if (!ExtensionesPermitidas.Contains(extension))
        {
            throw new AudioInvalidoException(
                "Solo se permiten audios MP3 o WAV.");
        }

        if (!TiposPermitidos.Contains(archivo.ContentType))
        {
            throw new AudioInvalidoException(
                "El tipo declarado del archivo no corresponde a un audio permitido.");
        }
    }

    private static async Task ValidarFirmaAsync(
        string ruta,
        string extension,
        CancellationToken cancellationToken)
    {
        byte[] encabezado = new byte[12];

        await using FileStream stream = new(
            ruta,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            FileOptions.Asynchronous |
            FileOptions.SequentialScan);

        int leidos = await stream.ReadAsync(
            encabezado,
            cancellationToken);

        bool firmaValida = extension switch
        {
            ".wav" => EsWav(encabezado, leidos),
            ".mp3" => EsMp3(encabezado, leidos),
            _ => false
        };

        if (!firmaValida)
        {
            throw new AudioInvalidoException(
                "El contenido del archivo no corresponde a un audio MP3 o WAV válido.");
        }
    }

    private static bool EsWav(byte[] bytes, int cantidad)
    {
        return cantidad >= 12 &&
               bytes[0] == (byte)'R' &&
               bytes[1] == (byte)'I' &&
               bytes[2] == (byte)'F' &&
               bytes[3] == (byte)'F' &&
               bytes[8] == (byte)'W' &&
               bytes[9] == (byte)'A' &&
               bytes[10] == (byte)'V' &&
               bytes[11] == (byte)'E';
    }

    private static bool EsMp3(byte[] bytes, int cantidad)
    {
        bool tieneId3 =
            cantidad >= 3 &&
            bytes[0] == (byte)'I' &&
            bytes[1] == (byte)'D' &&
            bytes[2] == (byte)'3';

        bool tieneFrameMpeg =
            cantidad >= 2 &&
            bytes[0] == 0xFF &&
            (bytes[1] & 0xE0) == 0xE0;

        return tieneId3 || tieneFrameMpeg;
    }

    private void LimpiarTemporalesAbandonados()
    {
        DateTime limite =
            DateTime.UtcNow.AddMinutes(
                -Math.Max(1, _options.RetentionMinutes));

        foreach (string ruta in Directory.EnumerateFiles(
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

    private static void EliminarSiExiste(string ruta)
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
