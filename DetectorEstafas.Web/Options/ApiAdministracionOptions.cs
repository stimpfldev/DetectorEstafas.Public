namespace DetectorEstafas.Web.Options;

public sealed class ApiAdministracionOptions
{
    public const string SectionName = "ApiAdministracion";

    public bool Enabled { get; set; }

    public string Secret { get; set; } = string.Empty;

    public int SessionMinutes { get; set; } = 20;
}
