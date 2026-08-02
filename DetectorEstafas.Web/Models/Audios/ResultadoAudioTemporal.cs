namespace DetectorEstafas.Web.Models.Audios;

public class ResultadoAudioTemporal
{
    public string NombreMostrado { get; init; } = string.Empty;

    public string TipoContenido { get; init; } = string.Empty;

    public long TamanoBytes { get; init; }

    public string TextoTranscripto { get; init; } = string.Empty;

    public bool TextoFueTruncado { get; init; }
}
