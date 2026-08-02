using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Services;
using DetectorEstafas.Web.Services.Capturas;
using DetectorEstafas.Web.Services.Audios;
using DetectorEstafas.Web.Services.InteligenciaArtificial;
using DetectorEstafas.Web.Services.Telefonos;
using DetectorEstafas.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafas.Web.Controllers;

public class AnalisisController : Controller
{
    private readonly IAnalizadorEstafasService
        _analizadorEstafasService;

    private readonly IRdapService
        _rdapService;

    private readonly DetectorEstafasDbContext
        _dbContext;

    private readonly ICapturaTemporalService
        _capturaTemporalService;

    private readonly IAudioTemporalService
        _audioTemporalService;

    private readonly IAnalisisIaService
        _analisisIaService;

    private readonly IIdentificacionTelefonoService
        _identificacionTelefonoService;

    private readonly ILogger<AnalisisController>
        _logger;

    public AnalisisController(
        IAnalizadorEstafasService analizadorEstafasService,
        IRdapService rdapService,
        DetectorEstafasDbContext dbContext,
        ICapturaTemporalService capturaTemporalService,
        IAudioTemporalService audioTemporalService,
        IAnalisisIaService analisisIaService,
        IIdentificacionTelefonoService identificacionTelefonoService,
        ILogger<AnalisisController> logger)
    {
        _analizadorEstafasService =
            analizadorEstafasService;

        _rdapService = rdapService;
        _dbContext = dbContext;
        _capturaTemporalService = capturaTemporalService;
        _audioTemporalService = audioTemporalService;
        _analisisIaService = analisisIaService;
        _identificacionTelefonoService = identificacionTelefonoService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index(
        string? origen)
    {
        return View(new AnalisisViewModel
        {
            Origen = NormalizarOrigen(origen)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("analisis")]
    [RequestSizeLimit(32 * 1024)]
    public async Task<IActionResult> Index(
        AnalisisViewModel model,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ResultadoAnalisis resultado =
            _analizadorEstafasService.Analizar(
                model.Contenido,
                model.Tipo);

        model.Resultado = resultado;

        if (model.Tipo == TipoContenido.Telefono)
        {
            model.IdentificacionTelefono =
                _identificacionTelefonoService.Identificar(model.Contenido);
        }

        if (model.SolicitarEvaluacionIa)
        {
            model.EvaluacionIa =
                await _analisisIaService.EvaluarAsync(
                    model.Contenido,
                    model.Tipo,
                    resultado,
                    cancellationToken);
        }

        model.Origen =
            NormalizarOrigen(model.Origen);

        await RegistrarMetricaAsync(
            model,
            resultado,
            cancellationToken);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("capturas")]
    [RequestSizeLimit(6 * 1024 * 1024)]
    public async Task<IActionResult> CargarCaptura(
        IFormFile? captura,
        string? origen,
        CancellationToken cancellationToken)
    {
        AnalisisViewModel model = new()
        {
            Origen = NormalizarOrigen(origen)
        };

        if (captura is null)
        {
            model.CapturaError =
                "Seleccioná una captura PNG o JPEG.";

            return View("Index", model);
        }

        try
        {
            model.CapturaValidada =
                await _capturaTemporalService.ProcesarAsync(
                    captura,
                    cancellationToken);

            model.Contenido =
                model.CapturaValidada.TextoExtraido;

            model.Tipo = TipoContenido.Mensaje;
            model.Resultado =
                _analizadorEstafasService.Analizar(
                    model.Contenido,
                    model.Tipo);

            if (model.SolicitarEvaluacionIa)
            {
                model.EvaluacionIa =
                    await _analisisIaService.EvaluarAsync(
                        model.Contenido,
                        model.Tipo,
                        model.Resultado,
                        cancellationToken);
            }

            await RegistrarMetricaAsync(
                model,
                model.Resultado,
                cancellationToken);
        }
        catch (CapturaInvalidaException exception)
        {
            model.CapturaError = exception.Message;
        }
        catch (OcrCapturaException exception)
        {
            _logger.LogWarning(
                exception,
                "El OCR no pudo procesar la captura.");

            model.CapturaError = exception.Message;
        }

        return View("Index", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("audios")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> CargarAudio(
        IFormFile? audio,
        string? origen,
        CancellationToken cancellationToken)
    {
        AnalisisViewModel model = new()
        {
            Origen = NormalizarOrigen(origen)
        };

        if (audio is null)
        {
            model.AudioError =
                "Seleccioná un audio MP3 o WAV.";

            return View("Index", model);
        }

        try
        {
            model.AudioValidado =
                await _audioTemporalService.ProcesarAsync(
                    audio,
                    cancellationToken);

            model.Contenido = model.AudioValidado.TextoTranscripto;
            model.Tipo = TipoContenido.Llamada;
            model.Resultado = _analizadorEstafasService.Analizar(
                model.Contenido,
                model.Tipo);

            if (model.SolicitarEvaluacionIa)
            {
                model.EvaluacionIa =
                    await _analisisIaService.EvaluarAsync(
                        model.Contenido,
                        model.Tipo,
                        model.Resultado,
                        cancellationToken);
            }

            await RegistrarMetricaAsync(
                model,
                model.Resultado,
                cancellationToken);
        }
        catch (AudioInvalidoException exception)
        {
            model.AudioError = exception.Message;
        }
        catch (TranscripcionAudioException exception)
        {
            _logger.LogWarning(
                exception,
                "La transcripción local no pudo procesar el audio.");

            model.AudioError = exception.Message;
        }

        return View("Index", model);
    }

    [HttpGet]
    [EnableRateLimiting("rdap")]
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public async Task<IActionResult> ConsultarDominio(
        string dominio,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dominio) ||
            dominio.Length > 253 ||
            !dominio.EndsWith(
                ".ar",
                StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new
            {
                ok = false,
                estado = "El dominio no es válido."
            });
        }

        ResultadoRdap resultado =
            await _rdapService.ConsultarDominioAsync(
                dominio,
                cancellationToken);

        return Json(new
        {
            ok = true,
            encontrado = resultado.Encontrado,
            estado = resultado.Estado,
            fechaRegistroUtc =
                resultado.FechaRegistroUtc,
            antiguedadDias =
                resultado.AntiguedadDias,
            senales = resultado.Senales
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("feedback")]
    [RequestSizeLimit(8 * 1024)]
    public async Task<IActionResult> RegistrarFeedback(
        long analisisRegistroId,
        bool fueUtil,
        CancellationToken cancellationToken)
    {
        bool existeAnalisis =
            await _dbContext.AnalisisRegistros
                .AnyAsync(
                    registro =>
                        registro.AnalisisRegistroId ==
                        analisisRegistroId,
                    cancellationToken);

        if (!existeAnalisis)
        {
            return NotFound();
        }

        AnalisisFeedback? feedback =
            await _dbContext.AnalisisFeedbacks
                .SingleOrDefaultAsync(
                    item =>
                        item.AnalisisRegistroId ==
                        analisisRegistroId,
                    cancellationToken);

        if (feedback is null)
        {
            feedback = new AnalisisFeedback
            {
                AnalisisRegistroId =
                    analisisRegistroId,
                FueUtil = fueUtil,
                FechaUtc = DateTime.UtcNow
            };

            _dbContext.AnalisisFeedbacks.Add(feedback);
        }
        else
        {
            feedback.FueUtil = fueUtil;
            feedback.FechaUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Json(new { ok = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("feedback")]
    [RequestSizeLimit(8 * 1024)]
    public async Task<IActionResult> RegistrarReporte(
        long analisisRegistroId,
        CategoriaReporteComunitario categoria,
        CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(
                typeof(CategoriaReporteComunitario),
                categoria))
        {
            return BadRequest();
        }

        bool existeAnalisis =
            await _dbContext.AnalisisRegistros
                .AnyAsync(
                    registro =>
                        registro.AnalisisRegistroId ==
                        analisisRegistroId,
                    cancellationToken);

        if (!existeAnalisis)
        {
            return NotFound();
        }

        ReporteComunitario? reporte =
            await _dbContext.ReportesComunitarios
                .SingleOrDefaultAsync(
                    item =>
                        item.AnalisisRegistroId ==
                        analisisRegistroId,
                    cancellationToken);

        if (reporte is null)
        {
            reporte = new ReporteComunitario
            {
                AnalisisRegistroId =
                    analisisRegistroId,
                Categoria = categoria,
                FechaUtc = DateTime.UtcNow
            };

            _dbContext.ReportesComunitarios.Add(reporte);
        }
        else
        {
            reporte.Categoria = categoria;
            reporte.FechaUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return Json(new { ok = true });
    }

    [HttpPost]
    [IgnoreAntiforgeryToken]
    [EnableRateLimiting("analisis")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    public async Task<IActionResult> CompartirDesdeDispositivo(
        string? title,
        string? text,
        string? url,
        List<IFormFile>? files,
        CancellationToken cancellationToken)
    {
        IFormFile? archivo = files?
            .FirstOrDefault(file => file.Length > 0);

        if (archivo is not null)
        {
            string tipoContenido =
                archivo.ContentType?.ToLowerInvariant() ?? string.Empty;

            try
            {
                if (tipoContenido.StartsWith("image/", StringComparison.Ordinal))
                {
                    AnalisisViewModel modeloCaptura = new()
                    {
                        Origen = "pwa-compartir"
                    };

                    modeloCaptura.CapturaValidada =
                        await _capturaTemporalService.ProcesarAsync(
                            archivo,
                            cancellationToken);

                    modeloCaptura.Contenido =
                        modeloCaptura.CapturaValidada.TextoExtraido;

                    modeloCaptura.Tipo = TipoContenido.Mensaje;
                    modeloCaptura.Resultado =
                        _analizadorEstafasService.Analizar(
                            modeloCaptura.Contenido,
                            modeloCaptura.Tipo);

                    await RegistrarMetricaAsync(
                        modeloCaptura,
                        modeloCaptura.Resultado,
                        cancellationToken);

                    return View("Index", modeloCaptura);
                }

                if (tipoContenido.StartsWith("audio/", StringComparison.Ordinal))
                {
                    AnalisisViewModel modeloAudio = new()
                    {
                        Origen = "pwa-compartir"
                    };

                    modeloAudio.AudioValidado =
                        await _audioTemporalService.ProcesarAsync(
                            archivo,
                            cancellationToken);

                    modeloAudio.Contenido =
                        modeloAudio.AudioValidado.TextoTranscripto;

                    modeloAudio.Tipo = TipoContenido.Llamada;
                    modeloAudio.Resultado =
                        _analizadorEstafasService.Analizar(
                            modeloAudio.Contenido,
                            modeloAudio.Tipo);

                    await RegistrarMetricaAsync(
                        modeloAudio,
                        modeloAudio.Resultado,
                        cancellationToken);

                    return View("Index", modeloAudio);
                }
            }
            catch (CapturaInvalidaException exception)
            {
                return View("Index", new AnalisisViewModel
                {
                    Origen = "pwa-compartir",
                    CapturaError = exception.Message
                });
            }
            catch (OcrCapturaException exception)
            {
                _logger.LogWarning(
                    exception,
                    "El OCR no pudo procesar una captura compartida.");

                return View("Index", new AnalisisViewModel
                {
                    Origen = "pwa-compartir",
                    CapturaError = exception.Message
                });
            }
            catch (AudioInvalidoException exception)
            {
                return View("Index", new AnalisisViewModel
                {
                    Origen = "pwa-compartir",
                    AudioError = exception.Message
                });
            }
            catch (TranscripcionAudioException exception)
            {
                _logger.LogWarning(
                    exception,
                    "No se pudo transcribir un audio compartido.");

                return View("Index", new AnalisisViewModel
                {
                    Origen = "pwa-compartir",
                    AudioError = exception.Message
                });
            }

            return View("Index", new AnalisisViewModel
            {
                Origen = "pwa-compartir",
                CapturaError = "El archivo compartido no es una imagen o un audio admitido."
            });
        }

        string contenido = string.Join(
            Environment.NewLine,
            new[] { title, text, url }
                .Where(value => !string.IsNullOrWhiteSpace(value)))
            .Trim();

        return Compartir(
            title,
            text,
            url);
    }

    [HttpGet]
    public IActionResult Compartir(
        string? title,
        string? text,
        string? url)
    {
        List<string> partes =
            new[] { title, text, url }
                .Where(valor =>
                    !string.IsNullOrWhiteSpace(valor))
                .Select(valor =>
                    valor!.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .ToList();

        string contenido = string.Join(
            Environment.NewLine,
            partes);

        TipoContenido tipo =
            !string.IsNullOrWhiteSpace(url)
                ? TipoContenido.Enlace
                : TipoContenido.Mensaje;

        return View("Index", new AnalisisViewModel
        {
            Contenido = contenido,
            Tipo = tipo,
            Origen = "Compartido"
        });
    }

    [HttpGet]
    public IActionResult Limite()
    {
        return View();
    }

    [HttpGet]
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public IActionResult Error()
    {
        Response.StatusCode =
            StatusCodes.Status500InternalServerError;

        return View();
    }

    private async Task RegistrarMetricaAsync(
        AnalisisViewModel model,
        ResultadoAnalisis resultado,
        CancellationToken cancellationToken)
    {
        AnalisisRegistro registro = new()
        {
            FechaUtc = DateTime.UtcNow,
            TipoContenido = model.Tipo,
            NivelRiesgo = resultado.Nivel,
            Puntaje = checked((byte)resultado.Puntaje),
            CantidadSenales = checked(
                (short)resultado.SenalesDetectadas.Count),
            Origen = model.Origen
        };

        try
        {
            _dbContext.AnalisisRegistros.Add(registro);
            await _dbContext.SaveChangesAsync(cancellationToken);
            model.AnalisisRegistroId = registro.AnalisisRegistroId;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "No se pudo registrar la métrica del análisis.");

            model.AnalisisRegistroId = null;
        }
    }

    private static string NormalizarOrigen(
        string? origen)
    {
        return origen?
            .Trim()
            .ToLowerInvariant() switch
        {
            "pwa" => "PWA",
            "compartido" => "Compartido",
            _ => "Web"
        };
    }
}