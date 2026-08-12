namespace DetectorEstafas.Web.Options;

public class AudioOptions
{
    public const string SectionName = "Audios";

    public long MaxFileSizeBytes { get; set; } =
        10 * 1024 * 1024;

    public int RetentionMinutes { get; set; } = 60;

    public string TemporaryFolderName { get; set; } =
        "DetectorEstafas/Audios";
}
