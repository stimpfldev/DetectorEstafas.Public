namespace DetectorEstafas.Web.Models.InteligenciaArtificial;

public sealed class ResultadoEvaluacionIa
{
    public bool Disponible { get; init; }
    public bool SeUsoFallback { get; init; }
    public string Estado { get; init; } = string.Empty;
    public string Resumen { get; init; } = string.Empty;
    public IReadOnlyList<string> SenalesAdicionales { get; init; } = [];
    public IReadOnlyList<string> Recomendaciones { get; init; } = [];
    public decimal? Confianza { get; init; }
}
