using System.Text.RegularExpressions;
using DetectorEstafas.Web.Models.Capturas;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;
using Tesseract;

namespace DetectorEstafas.Web.Services.Capturas;

public sealed partial class TesseractOcrCapturaService
    : IOcrCapturaService
{
    private readonly OcrOptions _options;
    private readonly string _dataPath;

    public TesseractOcrCapturaService(
        IOptions<OcrOptions> options,
        IWebHostEnvironment environment)
    {
        _options = options.Value;

        if (_options.MaxExtractedCharacters < 3)
        {
            throw new InvalidOperationException(
                "El límite de texto OCR debe ser de al menos 3 caracteres.");
        }

        _dataPath = Path.Combine(
            environment.ContentRootPath,
            _options.DataFolderName);
    }

    public Task<ResultadoOcr> ExtraerTextoAsync(
        string rutaImagen,
        CancellationToken cancellationToken)
    {
        return Task.Run(
            () => ExtraerTexto(rutaImagen, cancellationToken),
            cancellationToken);
    }

    private ResultadoOcr ExtraerTexto(
        string rutaImagen,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string trainedDataPath = Path.Combine(
            _dataPath,
            $"{_options.Language}.traineddata");

        if (!File.Exists(trainedDataPath))
        {
            throw new OcrCapturaException(
                "No se encontró el archivo de idioma del OCR. Ejecutá el script Preparar-OCR.ps1 y reiniciá la aplicación.");
        }

        try
        {
            using TesseractEngine engine = new(
                _dataPath,
                _options.Language,
                EngineMode.LstmOnly);

            using Pix image = Pix.LoadFromFile(rutaImagen);
            using Page page = engine.Process(
                image,
                PageSegMode.Auto);

            cancellationToken.ThrowIfCancellationRequested();

            string texto = NormalizarTexto(page.GetText());

            if (texto.Length < 3)
            {
                throw new OcrCapturaException(
                    "No se detectó texto suficiente en la captura.");
            }

            bool fueTruncado =
                texto.Length > _options.MaxExtractedCharacters;

            if (fueTruncado)
            {
                texto = texto[.._options.MaxExtractedCharacters]
                    .Trim();
            }

            return new ResultadoOcr
            {
                Texto = texto,
                ConfianzaPromedio = page.GetMeanConfidence(),
                FueTruncado = fueTruncado
            };
        }
        catch (OcrCapturaException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new OcrCapturaException(
                "No fue posible extraer el texto de la captura.",
                exception);
        }
    }

    private static string NormalizarTexto(string texto)
    {
        string normalizado = texto
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');

        normalizado = EspaciosHorizontalesRegex()
            .Replace(normalizado, " ");

        normalizado = LineasVaciasRegex()
            .Replace(normalizado, "\n\n");

        return normalizado.Trim();
    }

    [GeneratedRegex(@"[ \t]+", RegexOptions.CultureInvariant)]
    private static partial Regex EspaciosHorizontalesRegex();

    [GeneratedRegex(@"\n{3,}", RegexOptions.CultureInvariant)]
    private static partial Regex LineasVaciasRegex();
}
