using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.InteligenciaArtificial;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.InteligenciaArtificial;

public sealed class AnalisisIaService : IAnalisisIaService
{
    private readonly IProveedorEvaluacionIa _proveedor;
    private readonly InteligenciaArtificialOptions _options;
    private readonly ILogger<AnalisisIaService> _logger;

    public AnalisisIaService(
        IProveedorEvaluacionIa proveedor,
        IOptions<InteligenciaArtificialOptions> options,
        ILogger<AnalisisIaService> logger)
    {
        _proveedor = proveedor;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ResultadoEvaluacionIa?> EvaluarAsync(
        string contenido,
        TipoContenido tipoContenido,
        ResultadoAnalisis resultadoLocal,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return null;
        }

        string contenidoLimitado = contenido.Length <= _options.MaxInputCharacters
            ? contenido
            : contenido[.._options.MaxInputCharacters];

        SolicitudEvaluacionIa solicitud = new()
        {
            Contenido = contenidoLimitado,
            TipoContenido = tipoContenido,
            NivelLocal = resultadoLocal.Nivel,
            PuntajeLocal = resultadoLocal.Puntaje,
            SenalesLocales = resultadoLocal.SenalesDetectadas
        };

        using CancellationTokenSource timeout =
            CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        timeout.CancelAfter(TimeSpan.FromSeconds(
            Math.Clamp(_options.TimeoutSeconds, 1, 30)));

        try
        {
            RespuestaProveedorIa respuesta =
                await _proveedor.EvaluarAsync(
                    solicitud,
                    timeout.Token);

            if (string.IsNullOrWhiteSpace(respuesta.Resumen) ||
                respuesta.Confianza < 0 ||
                respuesta.Confianza > 1)
            {
                return CrearFallback(
                    "La respuesta del proveedor no superó la validación.");
            }

            return new ResultadoEvaluacionIa
            {
                Disponible = true,
                SeUsoFallback = false,
                Estado = "Evaluación complementaria disponible.",
                Resumen = respuesta.Resumen.Trim(),
                SenalesAdicionales = respuesta.SenalesAdicionales
                    .Where(valor => !string.IsNullOrWhiteSpace(valor))
                    .Select(valor => valor.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(Math.Clamp(_options.MaxAdditionalSignals, 1, 10))
                    .ToArray(),
                Recomendaciones = respuesta.Recomendaciones
                    .Where(valor => !string.IsNullOrWhiteSpace(valor))
                    .Select(valor => valor.Trim())
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(Math.Clamp(_options.MaxRecommendations, 1, 10))
                    .ToArray(),
                Confianza = respuesta.Confianza
            };
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(
                "La evaluación complementaria con IA superó el tiempo límite.");

            return CrearFallback(
                "El proveedor de IA no respondió dentro del tiempo permitido.");
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "La evaluación complementaria con IA no estuvo disponible.");

            return CrearFallback(
                "La evaluación con IA no estuvo disponible. Se conserva el resultado del motor local.");
        }
    }

    private static ResultadoEvaluacionIa CrearFallback(string estado)
    {
        return new ResultadoEvaluacionIa
        {
            Disponible = false,
            SeUsoFallback = true,
            Estado = estado,
            Resumen =
                "El análisis determinista continúa siendo el resultado principal.",
            SenalesAdicionales = [],
            Recomendaciones = [],
            Confianza = null
        };
    }
}
