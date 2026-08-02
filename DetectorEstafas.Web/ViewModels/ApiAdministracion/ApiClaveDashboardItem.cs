namespace DetectorEstafas.Web.ViewModels.ApiAdministracion;

public sealed class ApiClaveDashboardItem
{
    public int ApiClaveId { get; init; }

    public string Prefijo { get; init; } = string.Empty;

    public bool Habilitada { get; init; }

    public DateTime FechaCreacionUtc { get; init; }

    public DateTime? FechaRevocacionUtc { get; init; }
}
