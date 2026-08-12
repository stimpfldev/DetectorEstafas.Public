namespace DetectorEstafas.Web.Options;

public sealed class CapturaOptions
{
    public const string SectionName = "Capturas";

    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;

    public int RetentionMinutes { get; set; } = 60;

    public string TemporaryFolderName { get; set; } = "DetectorEstafas/Capturas";
}
