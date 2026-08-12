using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.ViewModels.ApiAdministracion;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafas.Web.Services.Api.Administracion;

public sealed class ApiAdministracionService :
    IApiAdministracionService
{
    private readonly DetectorEstafasDbContext _dbContext;

    public ApiAdministracionService(
        DetectorEstafasDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ApiDashboardViewModel>
        ObtenerDashboardAsync(
            CancellationToken cancellationToken)
    {
        DateTime nowUtc = DateTime.UtcNow;

        DateOnly todayUtc =
            DateOnly.FromDateTime(nowUtc);

        DateOnly inicioMesUtc =
            new(todayUtc.Year, todayUtc.Month, 1);

        DateOnly proximoMesUtc =
            inicioMesUtc.AddMonths(1);

        List<ApiCliente> clients =
            await _dbContext.ApiClientes
                .AsNoTracking()
                .Include(item => item.Claves)
                .OrderBy(item => item.Nombre)
                .ToListAsync(cancellationToken);

        List<ApiConsumoDiario> consumosMes =
            clients.Count == 0
                ? []
                : await _dbContext.ApiConsumosDiarios
                    .AsNoTracking()
                    .Where(item =>
                        item.FechaUtc >= inicioMesUtc &&
                        item.FechaUtc < proximoMesUtc)
                    .ToListAsync(cancellationToken);

        ILookup<int, ApiConsumoDiario> consumosPorCliente =
            consumosMes.ToLookup(
                item => item.ApiClienteId);

        List<ApiClienteDashboardItem> items =
            clients.Select(client =>
            {
                bool configuracionValida =
                    ApiPlanes.TryObtenerPeriodo(
                        client,
                        nowUtc,
                        out ApiPeriodoCuota periodo);

                int consumidasPeriodo =
                    configuracionValida
                        ? consumosPorCliente[
                                client.ApiClienteId]
                            .Where(consumo =>
                                consumo.FechaUtc >=
                                    periodo.DesdeUtc &&
                                consumo.FechaUtc <
                                    periodo.HastaUtcExclusiva)
                            .Sum(consumo =>
                                consumo.CantidadSolicitudes)
                        : 0;

                string plan =
                    ApiPlanes.Normalizar(client.Plan)
                    ?? client.Plan;

                return new ApiClienteDashboardItem
                {
                    ApiClienteId =
                        client.ApiClienteId,
                    Nombre = client.Nombre,
                    Plan = plan,
                    Periodo =
                        configuracionValida
                            ? periodo.Periodo
                            : ApiPlanes.EsPrueba(plan)
                                ? PeriodoCuotaApi.Diario
                                : PeriodoCuotaApi.Mensual,
                    LimitePeriodo =
                        configuracionValida
                            ? periodo.Limite
                            : 0,
                    ConsumidasPeriodo =
                        consumidasPeriodo,
                    ReiniciaUtc =
                        configuracionValida
                            ? periodo.ReiniciaUtc
                            : default,
                    CuotaMensualPersonalizada =
                        string.Equals(
                            plan,
                            ApiPlanes.AMedida,
                            StringComparison.Ordinal)
                                ? client.CuotaMensual
                                : null,
                    ConfiguracionCuotaValida =
                        configuracionValida,
                    Habilitado =
                        client.Habilitado,
                    FechaCreacionUtc =
                        client.FechaCreacionUtc,
                    Claves = client.Claves
                        .OrderByDescending(clave =>
                            clave.FechaCreacionUtc)
                        .Select(clave =>
                            new ApiClaveDashboardItem
                            {
                                ApiClaveId =
                                    clave.ApiClaveId,
                                Prefijo =
                                    clave.Prefijo,
                                Habilitada =
                                    clave.Habilitada &&
                                    clave.FechaRevocacionUtc
                                        == null,
                                FechaCreacionUtc =
                                    clave.FechaCreacionUtc,
                                FechaRevocacionUtc =
                                    clave.FechaRevocacionUtc
                            })
                        .ToList()
                };
            })
            .ToList();

        return new ApiDashboardViewModel
        {
            FechaUtc = todayUtc,
            TotalClientes = items.Count,
            ClientesHabilitados =
                items.Count(item =>
                    item.Habilitado),
            SolicitudesHoy =
                consumosMes
                    .Where(item =>
                        item.FechaUtc == todayUtc)
                    .Sum(item =>
                        item.CantidadSolicitudes),
            Clientes = items
        };
    }

    public async Task<bool>
        CambiarEstadoClienteAsync(
            int apiClienteId,
            CancellationToken cancellationToken)
    {
        ApiCliente? client =
            await _dbContext.ApiClientes
                .SingleOrDefaultAsync(
                    item =>
                        item.ApiClienteId ==
                            apiClienteId,
                    cancellationToken);

        if (client is null)
        {
            return false;
        }

        client.Habilitado =
            !client.Habilitado;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool>
        ActualizarPlanClienteAsync(
            int apiClienteId,
            string plan,
            int? cuotaMensualPersonalizada,
            CancellationToken cancellationToken)
    {
        string? normalizedPlan =
            ApiPlanes.Normalizar(plan);

        if (normalizedPlan is null)
        {
            return false;
        }

        int? cuotaMensual =
            ApiPlanes.ObtenerCuotaMensualFija(
                normalizedPlan);

        if (string.Equals(
                normalizedPlan,
                ApiPlanes.AMedida,
                StringComparison.Ordinal))
        {
            if (!cuotaMensualPersonalizada.HasValue ||
                cuotaMensualPersonalizada.Value < 1 ||
                cuotaMensualPersonalizada.Value >
                    ApiPlanes
                        .CuotaMensualMaximaPersonalizada)
            {
                return false;
            }

            cuotaMensual =
                cuotaMensualPersonalizada.Value;
        }

        ApiCliente? client =
            await _dbContext.ApiClientes
                .SingleOrDefaultAsync(
                    item =>
                        item.ApiClienteId ==
                            apiClienteId,
                    cancellationToken);

        if (client is null)
        {
            return false;
        }

        string? planAnterior =
            ApiPlanes.Normalizar(client.Plan);

        bool anteriorEraPrueba =
            string.Equals(
                planAnterior,
                ApiPlanes.Prueba,
                StringComparison.Ordinal);

        bool nuevoEsPrueba =
            string.Equals(
                normalizedPlan,
                ApiPlanes.Prueba,
                StringComparison.Ordinal);

        bool cambiaPeriodo =
            anteriorEraPrueba != nuevoEsPrueba;

        DateTime nowUtc = DateTime.UtcNow;

        client.Plan = normalizedPlan;

        if (nuevoEsPrueba)
        {
            client.CuotaDiaria =
                ApiPlanes.CuotaDiariaPrueba;

            client.CuotaMensual = null;
        }
        else
        {
            if (!cuotaMensual.HasValue)
            {
                return false;
            }

            client.CuotaDiaria = 0;
            client.CuotaMensual =
                cuotaMensual.Value;
        }

        if (cambiaPeriodo)
        {
            client.FechaInicioPlanUtc = nowUtc;

            DateOnly todayUtc =
                DateOnly.FromDateTime(nowUtc);

            ApiConsumoDiario? consumoHoy =
                await _dbContext.ApiConsumosDiarios
                    .SingleOrDefaultAsync(
                        item =>
                            item.ApiClienteId ==
                                apiClienteId &&
                            item.FechaUtc ==
                                todayUtc,
                        cancellationToken);

            if (consumoHoy is not null)
            {
                _dbContext.ApiConsumosDiarios
                    .Remove(consumoHoy);
            }
        }
        else if (!client.FechaInicioPlanUtc.HasValue)
        {
            client.FechaInicioPlanUtc =
                client.FechaCreacionUtc;
        }

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }

    public async Task<bool> RevocarClaveAsync(
        int apiClaveId,
        CancellationToken cancellationToken)
    {
        ApiClave? key =
            await _dbContext.ApiClaves
                .SingleOrDefaultAsync(
                    item =>
                        item.ApiClaveId ==
                            apiClaveId,
                    cancellationToken);

        if (key is null ||
            !key.Habilitada ||
            key.FechaRevocacionUtc is not null)
        {
            return false;
        }

        key.Habilitada = false;
        key.FechaRevocacionUtc =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return true;
    }
}
