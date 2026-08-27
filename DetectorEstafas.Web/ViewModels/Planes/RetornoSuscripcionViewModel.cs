namespace DetectorEstafas.Web.ViewModels.Planes;

public sealed class RetornoSuscripcionViewModel
{
    public string Plan { get; init; } = string.Empty;

    public string Estado { get; init; } = string.Empty;

    public bool AccesoActivo { get; init; }
}
