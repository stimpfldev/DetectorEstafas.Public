namespace DetectorEstafas.Web.Options;

public sealed class InteligenciaArtificialOptions
{
    public const string SectionName = "InteligenciaArtificial";

    public bool Enabled { get; set; }
    public string Provider { get; set; } = "OpenAI";
    public string Endpoint { get; set; } = "https://api.openai.com/v1/chat/completions";
    public string Model { get; set; } = "gpt-5-mini";
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 12;
    public int MaxInputCharacters { get; set; } = 4000;
    public int MaxOutputTokens { get; set; } = 600;
    public int MaxAdditionalSignals { get; set; } = 5;
    public int MaxRecommendations { get; set; } = 5;
}
