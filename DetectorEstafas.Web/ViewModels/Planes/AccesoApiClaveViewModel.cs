namespace DetectorEstafas.Web.ViewModels.Planes;

public sealed class AccesoApiClaveViewModel
{
    public string Token { get; set; } = string.Empty;

    public string? ApiKey { get; init; }

    public bool InvalidaOExpirada { get; init; }
}
