namespace DetectorEstafas.Web.Options;

public sealed class ApiComercialOptions
{
    public const string SectionName = "ApiComercial";

    public bool Enabled { get; set; }

    public int MaxContentLength { get; set; } = 5000;

    public int TrialDays { get; set; } = 14;

    public List<ApiKeyOptions> Keys { get; set; } =
        new();
}

public sealed class ApiKeyOptions
{
    public string Name { get; set; } =
        string.Empty;

    public string Key { get; set; } =
        string.Empty;

    public bool Enabled { get; set; } = true;
}
