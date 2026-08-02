namespace DetectorEstafas.Web.Models.Capturas;

public sealed class ResultadoOcr
{
    public required string Texto { get; init; }

    public float ConfianzaPromedio { get; init; }

    public bool FueTruncado { get; init; }
}
