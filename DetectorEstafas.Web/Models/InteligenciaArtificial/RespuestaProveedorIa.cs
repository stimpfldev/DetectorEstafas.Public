namespace DetectorEstafas.Web.Models.InteligenciaArtificial;

public sealed class RespuestaProveedorIa
{
    public required string Resumen { get; init; }
    public required IReadOnlyList<string> SenalesAdicionales { get; init; }
    public required IReadOnlyList<string> Recomendaciones { get; init; }
    public required decimal Confianza { get; init; }
}
