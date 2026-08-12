using DetectorEstafas.Web.ViewModels.ApiAdministracion;

namespace DetectorEstafas.Web.Services.Api.Administracion;

public interface IApiAdministracionService
{
    Task<ApiDashboardViewModel> ObtenerDashboardAsync(
        CancellationToken cancellationToken);

    Task<bool> CambiarEstadoClienteAsync(
        int apiClienteId,
        CancellationToken cancellationToken);

    Task<bool> ActualizarPlanClienteAsync(
        int apiClienteId,
        string plan,
        int cuotaDiaria,
        CancellationToken cancellationToken);

    Task<bool> RevocarClaveAsync(
        int apiClaveId,
        CancellationToken cancellationToken);
}
