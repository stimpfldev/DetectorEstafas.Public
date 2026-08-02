namespace DetectorEstafas.Web.ViewModels.ApiAdministracion;

public sealed class ApiClienteDashboardItem
{
    public int ApiClienteId { get; init; }

    public string Nombre { get; init; } = string.Empty;

    public string Plan { get; init; } = string.Empty;

    public int CuotaDiaria { get; init; }

    public int ConsumidasHoy { get; init; }

    public int RestantesHoy => Math.Max(0, CuotaDiaria - ConsumidasHoy);

    public bool Habilitado { get; init; }

    public DateTime FechaCreacionUtc { get; init; }

    public IReadOnlyList<ApiClaveDashboardItem> Claves { get; init; } = [];
}
