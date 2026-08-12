using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.InteligenciaArtificial;

namespace DetectorEstafas.Web.Services.InteligenciaArtificial;

public interface IAnalisisIaService
{
    Task<ResultadoEvaluacionIa?> EvaluarAsync(
        string contenido,
        TipoContenido tipoContenido,
        ResultadoAnalisis resultadoLocal,
        CancellationToken cancellationToken);
}
