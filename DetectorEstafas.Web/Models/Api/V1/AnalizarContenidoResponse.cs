namespace DetectorEstafas.Web.Models.Api.V1;

public sealed class AnalizarContenidoResponse
{
    public bool Ok { get; init; } = true;

    public string VersionApi { get; init; } = "v1";

    public long AnalisisId { get; init; }

    public string Nivel { get; init; } = string.Empty;

    public int Puntaje { get; init; }

    public string Resumen { get; init; } = string.Empty;

    public IReadOnlyList<string> Senales { get; init; } = Array.Empty<string>();

    public IReadOnlyList<string> Recomendaciones { get; init; } = Array.Empty<string>();
}
