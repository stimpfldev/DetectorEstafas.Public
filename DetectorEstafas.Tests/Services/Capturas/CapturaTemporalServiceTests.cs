using DetectorEstafas.Web.Models.Capturas;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.Capturas;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Tests.Services.Capturas;

[TestClass]
public class CapturaTemporalServiceTests
{
    private string _temporaryFolderName = null!;
    private string _temporaryDirectory = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _temporaryFolderName =
            $"DetectorEstafasTests/Capturas/{Guid.NewGuid():N}";

        _temporaryDirectory = Path.Combine(
            Path.GetTempPath(),
            _temporaryFolderName.Replace(
                '/',
                Path.DirectorySeparatorChar));
    }

    [TestCleanup]
    public void Finalizar()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(
                _temporaryDirectory,
                recursive: true);
        }
    }

    [TestMethod]
    public async Task ProcesarAsync_PngValido_DevuelveVistaPreviaYEliminaTemporal()
    {
        byte[] contenido =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

        CapturaTemporalService service = CrearServicio();
        FormFile archivo = CrearArchivo(
            contenido,
            "captura.png",
            "image/png");

        var resultado = await service.ProcesarAsync(
            archivo,
            CancellationToken.None);

        Assert.AreEqual(
            "image/png",
            resultado.TipoContenido);

        Assert.AreEqual(
            "Texto extraído de prueba",
            resultado.TextoExtraido);

        StringAssert.StartsWith(
            resultado.VistaPreviaDataUrl,
            "data:image/png;base64,");

        Assert.IsTrue(
            !Directory.Exists(_temporaryDirectory) ||
            !Directory.EnumerateFiles(_temporaryDirectory).Any());
    }

    [TestMethod]
    public async Task ProcesarAsync_ExtensionNoCoincide_RechazaYEliminaTemporal()
    {
        byte[] contenido =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

        CapturaTemporalService service = CrearServicio();
        FormFile archivo = CrearArchivo(
            contenido,
            "captura.jpg",
            "image/jpeg");

        CapturaInvalidaException exception =
            await Assert.ThrowsAsync<CapturaInvalidaException>(
                () => service.ProcesarAsync(
                    archivo,
                    CancellationToken.None));

        StringAssert.Contains(
            exception.Message,
            "extensión");

        Assert.IsTrue(
            !Directory.Exists(_temporaryDirectory) ||
            !Directory.EnumerateFiles(_temporaryDirectory).Any());
    }

    [TestMethod]
    public async Task ProcesarAsync_ArchivoExcedeLimite_RechazaSinGuardar()
    {
        CapturaTemporalService service = CrearServicio(
            maxFileSizeBytes: 8);

        FormFile archivo = CrearArchivo(
            new byte[9],
            "captura.png",
            "image/png");

        CapturaInvalidaException exception =
            await Assert.ThrowsAsync<CapturaInvalidaException>(
                () => service.ProcesarAsync(
                    archivo,
                    CancellationToken.None));

        StringAssert.Contains(
            exception.Message,
            "límite");

        Assert.IsFalse(
            Directory.Exists(_temporaryDirectory));
    }

    [TestMethod]
    public async Task ProcesarAsync_NombrePeligroso_NoSeUsaComoRutaTemporal()
    {
        byte[] contenido =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

        CapturaTemporalService service = CrearServicio();
        FormFile archivo = CrearArchivo(
            contenido,
            "../../captura.png",
            "image/png");

        var resultado = await service.ProcesarAsync(
            archivo,
            CancellationToken.None);

        Assert.AreEqual(
            "captura-validada.png",
            resultado.NombreMostrado);

        Assert.IsTrue(
            !Directory.Exists(_temporaryDirectory) ||
            !Directory.EnumerateFiles(_temporaryDirectory).Any());
    }

    private CapturaTemporalService CrearServicio(
        long maxFileSizeBytes = 5 * 1024 * 1024)
    {
        CapturaOptions options = new()
        {
            MaxFileSizeBytes = maxFileSizeBytes,
            RetentionMinutes = 60,
            TemporaryFolderName = _temporaryFolderName
        };

        return new CapturaTemporalService(
            Options.Create(options),
            new OcrCapturaServiceFalso());
    }

    private sealed class OcrCapturaServiceFalso
        : IOcrCapturaService
    {
        public Task<ResultadoOcr> ExtraerTextoAsync(
            string rutaImagen,
            CancellationToken cancellationToken)
        {
            Assert.IsTrue(File.Exists(rutaImagen));

            return Task.FromResult(new ResultadoOcr
            {
                Texto = "Texto extraído de prueba",
                ConfianzaPromedio = 0.95f,
                FueTruncado = false
            });
        }
    }

    private static FormFile CrearArchivo(
        byte[] contenido,
        string fileName,
        string contentType)
    {
        MemoryStream stream = new(contenido);

        return new FormFile(
            stream,
            0,
            contenido.Length,
            "captura",
            fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
