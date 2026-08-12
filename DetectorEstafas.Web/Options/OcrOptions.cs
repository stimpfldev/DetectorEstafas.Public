namespace DetectorEstafas.Web.Options;

public sealed class OcrOptions
{
    public const string SectionName = "Ocr";

    public string DataFolderName { get; set; } = "OcrData";

    public string Language { get; set; } = "spa";

    public int MaxExtractedCharacters { get; set; } = 5000;
}
