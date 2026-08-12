using DetectorEstafas.Web.Models.Capturas;
using DetectorEstafas.Web.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Capturas;

public sealed class CapturaTemporalService : ICapturaTemporalService
{
    private static readonly byte[] FirmaPng =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    private readonly CapturaOptions _options;
    private readonly string _temporaryDirectory;
    private readonly IOcrCapturaService _ocrCapturaService;

    public CapturaTemporalService(
        IOptions<CapturaOptions> options,
        IOcrCapturaService ocrCapturaService)
    {
        _options = options.Value;
        _ocrCapturaService = ocrCapturaService;

        if (_options.MaxFileSizeBytes <= 0)
        {
            throw new InvalidOperationException(
                "El tamaño máximo de capturas debe ser mayor que cero.");
        }

        string relativeFolder =
            _options.TemporaryFolderName
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar)
                .Trim(Path.DirectorySeparatorChar);

        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            relativeFolder);
    }

    public async Task<ResultadoCapturaTemporal> ProcesarAsync(
        IFormFile archivo,
        CancellationToken cancellationToken)
    {
        ValidarDatosBasicos(archivo);
        Directory.CreateDirectory(_temporaryDirectory);
        EliminarArchivosVencidos();

        string extensionDeclarada =
            Path.GetExtension(archivo.FileName)
                .ToLowerInvariant();

        string internalFileName =
            $"{Guid.NewGuid():N}{extensionDeclarada}";

        string temporaryPath = Path.Combine(
            _temporaryDirectory,
            internalFileName);

        try
        {
            await using (FileStream destination = new(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await archivo.CopyToAsync(
                    destination,
                    cancellationToken);
            }

            byte[] bytes = await File.ReadAllBytesAsync(
                temporaryPath,
                cancellationToken);

            TipoCaptura tipoReal = DetectarTipoReal(bytes);
            ValidarCoincidencia(
                extensionDeclarada,
                archivo.ContentType,
                tipoReal);

            ResultadoOcr resultadoOcr =
                await _ocrCapturaService.ExtraerTextoAsync(
                    temporaryPath,
                    cancellationToken);

            return new ResultadoCapturaTemporal
            {
                NombreMostrado = "captura-validada" + tipoReal.Extension,
                TipoContenido = tipoReal.MediaType,
                TamanoBytes = bytes.LongLength,
                VistaPreviaDataUrl =
                    $"data:{tipoReal.MediaType};base64,{Convert.ToBase64String(bytes)}",
                TextoExtraido = resultadoOcr.Texto,
                ConfianzaOcr = resultadoOcr.ConfianzaPromedio,
                TextoFueTruncado = resultadoOcr.FueTruncado
            };
        }
        finally
        {
            EliminarSiExiste(temporaryPath);
        }
    }

    private void ValidarDatosBasicos(IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
        {
            throw new CapturaInvalidaException(
                "Seleccioná una captura PNG o JPEG.");
        }

        if (archivo.Length > _options.MaxFileSizeBytes)
        {
            throw new CapturaInvalidaException(
                "La captura supera el límite de 5 MB.");
        }

        string extension =
            Path.GetExtension(archivo.FileName)
                .ToLowerInvariant();

        if (extension is not ".png" and not ".jpg" and not ".jpeg")
        {
            throw new CapturaInvalidaException(
                "Solo se permiten capturas PNG o JPEG.");
        }
    }

    private static TipoCaptura DetectarTipoReal(byte[] bytes)
    {
        if (bytes.Length >= FirmaPng.Length &&
            bytes.AsSpan(0, FirmaPng.Length)
                .SequenceEqual(FirmaPng))
        {
            return new TipoCaptura(
                ".png",
                "image/png");
        }

        if (bytes.Length >= 4 &&
            bytes[0] == 0xFF &&
            bytes[1] == 0xD8 &&
            bytes[2] == 0xFF &&
            bytes[^2] == 0xFF &&
            bytes[^1] == 0xD9)
        {
            return new TipoCaptura(
                ".jpg",
                "image/jpeg");
        }

        throw new CapturaInvalidaException(
            "El contenido del archivo no corresponde a una imagen PNG o JPEG válida.");
    }

    private static void ValidarCoincidencia(
        string extensionDeclarada,
        string? contentType,
        TipoCaptura tipoReal)
    {
        bool extensionValida =
            tipoReal.Extension == ".png"
                ? extensionDeclarada == ".png"
                : extensionDeclarada is ".jpg" or ".jpeg";

        if (!extensionValida)
        {
            throw new CapturaInvalidaException(
                "La extensión no coincide con el contenido real del archivo.");
        }

        if (!string.Equals(
                contentType,
                tipoReal.MediaType,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new CapturaInvalidaException(
                "El tipo declarado no coincide con el contenido real del archivo.");
        }
    }

    private void EliminarArchivosVencidos()
    {
        if (!Directory.Exists(_temporaryDirectory))
        {
            return;
        }

        DateTime expirationUtc = DateTime.UtcNow.AddMinutes(
            -Math.Max(1, _options.RetentionMinutes));

        foreach (string path in Directory.EnumerateFiles(
                     _temporaryDirectory,
                     "*",
                     SearchOption.TopDirectoryOnly))
        {
            try
            {
                if (File.GetLastWriteTimeUtc(path) < expirationUtc)
                {
                    File.Delete(path);
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

    private static void EliminarSiExiste(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    private sealed record TipoCaptura(
        string Extension,
        string MediaType);
}
