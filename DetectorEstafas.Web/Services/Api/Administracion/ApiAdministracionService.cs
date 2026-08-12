using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.ViewModels.ApiAdministracion;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafas.Web.Services.Api.Administracion;

public sealed class ApiAdministracionService : IApiAdministracionService
{
    private readonly DetectorEstafasDbContext _dbContext;

    public ApiAdministracionService(
        DetectorEstafasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiDashboardViewModel> ObtenerDashboardAsync(
        CancellationToken cancellationToken)
    {
        DateOnly todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);

        List<ApiClienteDashboardItem> clients = await _dbContext.ApiClientes
            .AsNoTracking()
            .OrderBy(item => item.Nombre)
            .Select(item => new ApiClienteDashboardItem
            {
                ApiClienteId = item.ApiClienteId,
                Nombre = item.Nombre,
                Plan = item.Plan,
                CuotaDiaria = item.CuotaDiaria,
                ConsumidasHoy = item.Consumos
                    .Where(consumo => consumo.FechaUtc == todayUtc)
                    .Select(consumo => consumo.CantidadSolicitudes)
                    .FirstOrDefault(),
                Habilitado = item.Habilitado,
                FechaCreacionUtc = item.FechaCreacionUtc,
                Claves = item.Claves
                    .OrderByDescending(clave => clave.FechaCreacionUtc)
                    .Select(clave => new ApiClaveDashboardItem
                    {
                        ApiClaveId = clave.ApiClaveId,
                        Prefijo = clave.Prefijo,
                        Habilitada = clave.Habilitada && clave.FechaRevocacionUtc == null,
                        FechaCreacionUtc = clave.FechaCreacionUtc,
                        FechaRevocacionUtc = clave.FechaRevocacionUtc
                    })
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return new ApiDashboardViewModel
        {
            FechaUtc = todayUtc,
            TotalClientes = clients.Count,
            ClientesHabilitados = clients.Count(item => item.Habilitado),
            SolicitudesHoy = clients.Sum(item => item.ConsumidasHoy),
            Clientes = clients
        };
    }

    public async Task<bool> CambiarEstadoClienteAsync(
        int apiClienteId,
        CancellationToken cancellationToken)
    {
        var client = await _dbContext.ApiClientes
            .SingleOrDefaultAsync(
                item => item.ApiClienteId == apiClienteId,
                cancellationToken);

        if (client is null)
        {
            return false;
        }

        client.Habilitado = !client.Habilitado;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }


    public async Task<bool> ActualizarPlanClienteAsync(
        int apiClienteId,
        string plan,
        int cuotaDiaria,
        CancellationToken cancellationToken)
    {
        string normalizedPlan = plan.Trim();

        if ((!string.Equals(normalizedPlan, "Prueba", StringComparison.OrdinalIgnoreCase) &&
             !string.Equals(normalizedPlan, "Comercial", StringComparison.OrdinalIgnoreCase)) ||
            cuotaDiaria < 1 ||
            cuotaDiaria > 1_000_000)
        {
            return false;
        }

        var client = await _dbContext.ApiClientes
            .SingleOrDefaultAsync(
                item => item.ApiClienteId == apiClienteId,
                cancellationToken);

        if (client is null)
        {
            return false;
        }

        client.Plan = string.Equals(
            normalizedPlan,
            "Comercial",
            StringComparison.OrdinalIgnoreCase)
                ? "Comercial"
                : "Prueba";

        client.CuotaDiaria = cuotaDiaria;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RevocarClaveAsync(
        int apiClaveId,
        CancellationToken cancellationToken)
    {
        var key = await _dbContext.ApiClaves
            .SingleOrDefaultAsync(
                item => item.ApiClaveId == apiClaveId,
                cancellationToken);

        if (key is null || !key.Habilitada || key.FechaRevocacionUtc is not null)
        {
            return false;
        }

        key.Habilitada = false;
        key.FechaRevocacionUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
