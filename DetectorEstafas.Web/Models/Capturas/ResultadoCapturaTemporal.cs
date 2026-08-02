namespace DetectorEstafas.Web.Models.Capturas;

public sealed class ResultadoCapturaTemporal
{
    public required string NombreMostrado { get; init; }

    public required string TipoContenido { get; init; }

    public required string VistaPreviaDataUrl { get; init; }

    public required string TextoExtraido { get; init; }

    public long TamanoBytes { get; init; }

    public float ConfianzaOcr { get; init; }

    public bool TextoFueTruncado { get; init; }
}
