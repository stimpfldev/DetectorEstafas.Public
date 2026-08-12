using DetectorEstafas.Web.Models.InteligenciaArtificial;

namespace DetectorEstafas.Web.Services.InteligenciaArtificial;

public sealed class ProveedorIaDeshabilitado : IProveedorEvaluacionIa
{
    public Task<RespuestaProveedorIa> EvaluarAsync(
        SolicitudEvaluacionIa solicitud,
        CancellationToken cancellationToken)
    {
        throw new InvalidOperationException(
            "No hay un proveedor de IA configurado.");
    }
}
