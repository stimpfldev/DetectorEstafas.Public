namespace DetectorEstafas.Web.ViewModels.ApiAdministracion;

public sealed class ApiDashboardViewModel
{
    public DateOnly FechaUtc { get; init; }

    public int TotalClientes { get; init; }

    public int ClientesHabilitados { get; init; }

    public int SolicitudesHoy { get; init; }

    public IReadOnlyList<ApiClienteDashboardItem> Clientes { get; init; } = [];
}
