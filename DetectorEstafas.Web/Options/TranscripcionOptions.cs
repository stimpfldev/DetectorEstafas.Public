namespace DetectorEstafas.Web.Options;

public class TranscripcionOptions
{
    public const string SectionName = "Transcripcion";

    public string ModelFolderName { get; set; } = "WhisperModels";

    public string ModelFileName { get; set; } = "ggml-base.bin";

    public string Language { get; set; } = "es";

    public int MaxExtractedCharacters { get; set; } = 5000;
}
