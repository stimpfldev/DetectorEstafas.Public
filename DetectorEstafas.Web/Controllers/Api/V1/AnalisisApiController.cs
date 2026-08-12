using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.Api.V1;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services;
using DetectorEstafas.Web.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Controllers.Api.V1;

[ApiController]
[Route("api/v1/analisis")]
[EnableRateLimiting("api-comercial")]
public sealed class AnalisisApiController : ControllerBase
{
    private readonly IAnalizadorEstafasService _analizador;
    private readonly DetectorEstafasDbContext _dbContext;
    private readonly ApiComercialOptions _options;

    public AnalisisApiController(
        IAnalizadorEstafasService analizador,
        DetectorEstafasDbContext dbContext,
        IOptions<ApiComercialOptions> options)
    {
        _analizador = analizador;
        _dbContext = dbContext;
        _options = options.Value;
    }

    [HttpPost]
    [Consumes("application/json")]
    [Produces("application/json")]
    [ProducesResponseType<AnalizarContenidoResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<AnalizarContenidoResponse>> Analizar(
        [FromBody] AnalizarContenidoRequest request,
        CancellationToken cancellationToken)
    {
        if (request.TipoContenido is null ||
            !Enum.IsDefined(request.TipoContenido.Value))
        {
            return BadRequest(new
            {
                ok = false,
                codigo = "tipo_contenido_invalido",
                mensaje = "El tipo de contenido no es válido."
            });
        }

        string contenido = request.Contenido?.Trim() ?? string.Empty;

        if (contenido.Length == 0)
        {
            return BadRequest(new
            {
                ok = false,
                codigo = "contenido_requerido",
                mensaje = "El contenido es obligatorio."
            });
        }

        if (contenido.Length > _options.MaxContentLength)
        {
            return BadRequest(new
            {
                ok = false,
                codigo = "contenido_demasiado_largo",
                mensaje = $"El contenido no puede superar {_options.MaxContentLength} caracteres."
            });
        }

        ResultadoAnalisis resultado =
            _analizador.Analizar(contenido, request.TipoContenido.Value);

        string cliente =
            HttpContext.Items[ApiKeyMiddleware.ClientItemName]?.ToString()
            ?? "cliente-api";

        AnalisisRegistro registro = new()
        {
            FechaUtc = DateTime.UtcNow,
            TipoContenido = request.TipoContenido.Value,
            NivelRiesgo = resultado.Nivel,
            Puntaje = (byte)Math.Clamp(resultado.Puntaje, 0, 100),
            CantidadSenales = (short)Math.Min(resultado.SenalesDetectadas.Count, short.MaxValue),
            Origen = $"API:{cliente}"[..Math.Min($"API:{cliente}".Length, 20)]
        };

        _dbContext.AnalisisRegistros.Add(registro);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Ok(new AnalizarContenidoResponse
        {
            AnalisisId = registro.AnalisisRegistroId,
            Nivel = resultado.Nivel.ToString(),
            Puntaje = resultado.Puntaje,
            Resumen = resultado.Resumen,
            Senales = resultado.SenalesDetectadas,
            Recomendaciones = resultado.Recomendaciones
        });
    }
}
