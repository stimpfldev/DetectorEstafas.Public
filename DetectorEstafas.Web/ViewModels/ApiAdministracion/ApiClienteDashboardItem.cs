using DetectorEstafas.Web.Models.ApiComercial;

namespace DetectorEstafas.Web.ViewModels.ApiAdministracion;

public sealed class ApiClienteDashboardItem
{
    public int ApiClienteId { get; init; }

    public string Nombre { get; init; } =
        string.Empty;

    public string Plan { get; init; } =
        string.Empty;

    public PeriodoCuotaApi Periodo { get; init; }

    public int LimitePeriodo { get; init; }

    public int ConsumidasPeriodo { get; init; }

    public int RestantesPeriodo =>
        Math.Max(
            0,
            LimitePeriodo - ConsumidasPeriodo);

    public string PeriodoTexto =>
        Periodo == PeriodoCuotaApi.Mensual
            ? "mes"
            : "día";

    public DateTime ReiniciaUtc { get; init; }

    public int? CuotaMensualPersonalizada
        { get; init; }

    public bool ConfiguracionCuotaValida
        { get; init; }

    public bool Habilitado { get; init; }

    public DateTime FechaCreacionUtc { get; init; }

    public IReadOnlyList<ApiClaveDashboardItem>
        Claves { get; init; } = [];
}
