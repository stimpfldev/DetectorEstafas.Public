using DetectorEstafas.Web.Models.InteligenciaArtificial;

namespace DetectorEstafas.Web.Services.InteligenciaArtificial;

public interface IProveedorEvaluacionIa
{
    Task<RespuestaProveedorIa> EvaluarAsync(
        SolicitudEvaluacionIa solicitud,
        CancellationToken cancellationToken);
}
