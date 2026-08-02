using DetectorEstafas.Web.Models;

namespace DetectorEstafas.Web.Models.InteligenciaArtificial;

public sealed class SolicitudEvaluacionIa
{
    public required string Contenido { get; init; }
    public required TipoContenido TipoContenido { get; init; }
    public required NivelRiesgo NivelLocal { get; init; }
    public required int PuntajeLocal { get; init; }
    public required IReadOnlyList<string> SenalesLocales { get; init; }
}
