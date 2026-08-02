using DetectorEstafas.Web.Controllers;
using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.Audios;
using DetectorEstafas.Web.Models.Capturas;
using DetectorEstafas.Web.Models.InteligenciaArtificial;
using DetectorEstafas.Web.Services;
using DetectorEstafas.Web.Services.Audios;
using DetectorEstafas.Web.Services.Capturas;
using DetectorEstafas.Web.Services.InteligenciaArtificial;
using DetectorEstafas.Web.Services.Telefonos;
using DetectorEstafas.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace DetectorEstafas.Tests.Controllers;

[TestClass]
public class AnalisisControllerTests
{
    private DetectorEstafasDbContext _dbContext = null!;
    private AnalisisController _controller = null!;

    [TestInitialize]
    public void Inicializar()
    {
        DbContextOptions<DetectorEstafasDbContext> options =
            new DbContextOptionsBuilder<DetectorEstafasDbContext>()
                .UseInMemoryDatabase(
                    $"DetectorEstafasTests-{Guid.NewGuid()}")
                .Options;

        _dbContext =
            new DetectorEstafasDbContext(options);

        _controller = new AnalisisController(
            new AnalizadorEstafasService(),
            new RdapServiceFalso(),
            _dbContext,
            new CapturaTemporalServiceFalso(),
            new AudioTemporalServiceFalso(),
            new AnalisisIaServiceFalso(),
            new IdentificacionTelefonoService(),
            NullLogger<AnalisisController>.Instance);
    }
    [TestCleanup]
    public void Finalizar()
    {
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task Index_ModeloValido_RegistraAnalisis()
    {
        AnalisisViewModel model = new()
        {
            Contenido =
                "Último aviso. Tu cuenta será suspendida. " +
                "Enviá tu código de seguridad.",
            Tipo = TipoContenido.Mensaje,
            Origen = "pwa"
        };

        IActionResult actionResult =
            await _controller.Index(
                model,
                CancellationToken.None);

        ViewResult? viewResult =
            actionResult as ViewResult;

        Assert.IsNotNull(viewResult);
        Assert.IsNotNull(model.Resultado);
        Assert.IsTrue(model.AnalisisRegistroId.HasValue);

        AnalisisRegistro registro =
            await _dbContext.AnalisisRegistros
                .SingleAsync();

        Assert.AreEqual(
            model.AnalisisRegistroId.Value,
            registro.AnalisisRegistroId);

        Assert.AreEqual(
            TipoContenido.Mensaje,
            registro.TipoContenido);

        Assert.AreEqual(
            NivelRiesgo.Alto,
            registro.NivelRiesgo);

        Assert.AreEqual(
            "PWA",
            registro.Origen);

        Assert.IsTrue(
            registro.CantidadSenales > 0);
    }

    [TestMethod]
    public async Task Index_ModeloInvalido_NoRegistraAnalisis()
    {
        AnalisisViewModel model = new()
        {
            Contenido = string.Empty,
            Tipo = TipoContenido.Mensaje,
            Origen = "Web"
        };

        _controller.ModelState.AddModelError(
            nameof(AnalisisViewModel.Contenido),
            "Contenido obligatorio.");

        IActionResult actionResult =
            await _controller.Index(
                model,
                CancellationToken.None);

        ViewResult? viewResult =
            actionResult as ViewResult;

        Assert.IsNotNull(viewResult);

        int cantidadRegistros =
            await _dbContext.AnalisisRegistros.CountAsync();

        Assert.AreEqual(
            0,
            cantidadRegistros);

        Assert.IsNull(model.Resultado);
        Assert.IsFalse(model.AnalisisRegistroId.HasValue);
    }

    [TestMethod]
    public async Task CargarCaptura_TextoExtraido_AnalizaYRegistraMetrica()
    {
        byte[] contenido = [0x89, 0x50, 0x4E, 0x47];
        FormFile archivo = new(
            new MemoryStream(contenido),
            0,
            contenido.Length,
            "captura",
            "captura.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        IActionResult resultado =
            await _controller.CargarCaptura(
                archivo,
                "Web",
                CancellationToken.None);

        ViewResult vista =
            Assert.IsInstanceOfType<ViewResult>(resultado);

        AnalisisViewModel model =
            Assert.IsInstanceOfType<AnalisisViewModel>(vista.Model);

        Assert.IsNotNull(model.CapturaValidada);
        Assert.IsNotNull(model.Resultado);
        Assert.IsTrue(model.AnalisisRegistroId.HasValue);
        Assert.AreEqual(
            "Último aviso. Enviá tu código de seguridad.",
            model.Contenido);
        Assert.AreEqual(
            1,
            await _dbContext.AnalisisRegistros.CountAsync());
    }

    [TestMethod]
    public async Task CargarAudio_ArchivoValido_TranscribeYAnalizaContenido()
    {
        byte[] contenido =
        [
            (byte)'R', (byte)'I', (byte)'F', (byte)'F',
            0, 0, 0, 0,
            (byte)'W', (byte)'A', (byte)'V', (byte)'E'
        ];

        FormFile archivo = new(
            new MemoryStream(contenido),
            0,
            contenido.Length,
            "audio",
            "audio.wav")
        {
            Headers = new HeaderDictionary(),
            ContentType = "audio/wav"
        };

        IActionResult resultado =
            await _controller.CargarAudio(
                archivo,
                "Web",
                CancellationToken.None);

        ViewResult vista =
            Assert.IsInstanceOfType<ViewResult>(resultado);

        AnalisisViewModel model =
          Assert.IsInstanceOfType<AnalisisViewModel>(vista.Model);

        Assert.IsNotNull(model.AudioValidado);
        Assert.IsNotNull(model);
        Assert.IsNotNull(model.Resultado);
        Assert.IsFalse(string.IsNullOrWhiteSpace(model.Contenido));
     

        Assert.AreEqual(
            1,
            await _dbContext.AnalisisRegistros.CountAsync());
    }

    [TestMethod]
    public async Task RegistrarFeedback_SegundaRespuesta_ActualizaLaFila()
    {
        long analisisRegistroId =
            await CrearAnalisisRegistroAsync();

        IActionResult primeraRespuesta =
            await _controller.RegistrarFeedback(
                analisisRegistroId,
                true,
                CancellationToken.None);

        IActionResult segundaRespuesta =
            await _controller.RegistrarFeedback(
                analisisRegistroId,
                false,
                CancellationToken.None);

        Assert.IsInstanceOfType<JsonResult>(
            primeraRespuesta);

        Assert.IsInstanceOfType<JsonResult>(
            segundaRespuesta);

        List<AnalisisFeedback> feedbacks =
            await _dbContext.AnalisisFeedbacks
                .ToListAsync();

        Assert.AreEqual(
            1,
            feedbacks.Count);

        Assert.IsFalse(
            feedbacks[0].FueUtil);

        Assert.AreEqual(
            analisisRegistroId,
            feedbacks[0].AnalisisRegistroId);
    }

    [TestMethod]
    public async Task RegistrarReporte_SegundaCategoria_ActualizaLaFila()
    {
        long analisisRegistroId =
            await CrearAnalisisRegistroAsync();

        IActionResult primerReporte =
            await _controller.RegistrarReporte(
                analisisRegistroId,
                CategoriaReporteComunitario.SolicitudDinero,
                CancellationToken.None);

        IActionResult segundoReporte =
            await _controller.RegistrarReporte(
                analisisRegistroId,
                CategoriaReporteComunitario.SuplantacionIdentidad,
                CancellationToken.None);

        Assert.IsInstanceOfType<JsonResult>(
            primerReporte);

        Assert.IsInstanceOfType<JsonResult>(
            segundoReporte);

        List<ReporteComunitario> reportes =
            await _dbContext.ReportesComunitarios
                .ToListAsync();

        Assert.AreEqual(
            1,
            reportes.Count);

        Assert.AreEqual(
            CategoriaReporteComunitario.SuplantacionIdentidad,
            reportes[0].Categoria);

        Assert.AreEqual(
            analisisRegistroId,
            reportes[0].AnalisisRegistroId);
    }

    [TestMethod]
    public async Task RegistrarFeedback_AnalisisInexistente_RetornaNotFound()
    {
        IActionResult resultado =
            await _controller.RegistrarFeedback(
                999999,
                true,
                CancellationToken.None);

        Assert.IsInstanceOfType<NotFoundResult>(
            resultado);

        Assert.AreEqual(
            0,
            await _dbContext.AnalisisFeedbacks.CountAsync());
    }

    [TestMethod]
    public async Task RegistrarReporte_CategoriaInvalida_RetornaBadRequest()
    {
        long analisisRegistroId =
            await CrearAnalisisRegistroAsync();

        CategoriaReporteComunitario categoriaInvalida =
            (CategoriaReporteComunitario)999;

        IActionResult resultado =
            await _controller.RegistrarReporte(
                analisisRegistroId,
                categoriaInvalida,
                CancellationToken.None);

        Assert.IsInstanceOfType<BadRequestResult>(
            resultado);

        Assert.AreEqual(
            0,
            await _dbContext.ReportesComunitarios.CountAsync());
    }

    private async Task<long> CrearAnalisisRegistroAsync()
    {
        AnalisisRegistro registro = new()
        {
            FechaUtc = DateTime.UtcNow,
            TipoContenido = TipoContenido.Mensaje,
            NivelRiesgo = NivelRiesgo.Medio,
            Puntaje = 30,
            CantidadSenales = 2,
            Origen = "Web"
        };

        _dbContext.AnalisisRegistros.Add(registro);

        await _dbContext.SaveChangesAsync();

        return registro.AnalisisRegistroId;
    }

    private sealed class CapturaTemporalServiceFalso
        : ICapturaTemporalService
    {
        public Task<ResultadoCapturaTemporal> ProcesarAsync(
            IFormFile archivo,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new ResultadoCapturaTemporal
                {
                    NombreMostrado = "captura-validada.png",
                    TipoContenido = "image/png",
                    VistaPreviaDataUrl = "data:image/png;base64,iVBORw0KGgo=",
                    TextoExtraido =
                        "Último aviso. Enviá tu código de seguridad.",
                    ConfianzaOcr = 0.92f,
                    TextoFueTruncado = false,
                    TamanoBytes = archivo.Length
                });
        }
    }

    private sealed class AudioTemporalServiceFalso
        : IAudioTemporalService
    {
        public Task<ResultadoAudioTemporal> ProcesarAsync(
            IFormFile archivo,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                new ResultadoAudioTemporal
                {
                    NombreMostrado = "audio-validado.wav",
                    TipoContenido = "audio/wav",
                    TamanoBytes = archivo.Length,
                    TextoTranscripto =
                        "Transferí dinero para evitar el bloqueo.",
                    TextoFueTruncado = false
                });
        }
    }

    private sealed class RdapServiceFalso : IRdapService
    {
        public Task<ResultadoRdap> ConsultarDominioAsync(
            string dominio,
            CancellationToken cancellationToken)
        {
            ResultadoRdap resultado = new()
            {
                FueConsultado = true,
                Encontrado = true,
                Estado = "Resultado de prueba."
            };

            return Task.FromResult(resultado);
        }
    }
    private sealed class AnalisisIaServiceFalso : IAnalisisIaService
    {
        public Task<ResultadoEvaluacionIa?> EvaluarAsync(
            string contenido,
            TipoContenido tipoContenido,
            ResultadoAnalisis resultadoLocal,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<ResultadoEvaluacionIa?>(null);
        }
    }

}