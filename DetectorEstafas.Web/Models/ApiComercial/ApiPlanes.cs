namespace DetectorEstafas.Web.Models.ApiComercial;

public enum PeriodoCuotaApi
{
    Diario = 1,
    Mensual = 2
}

public readonly record struct ApiPeriodoCuota(
    PeriodoCuotaApi Periodo,
    DateOnly DesdeUtc,
    DateOnly HastaUtcExclusiva,
    int Limite)
{
    public DateTime ReiniciaUtc =>
        HastaUtcExclusiva.ToDateTime(
            TimeOnly.MinValue,
            DateTimeKind.Utc);
}

public static class ApiPlanes
{
    public const string Prueba = "Prueba";
    public const string Starter = "Starter";
    public const string Growth = "Growth";
    public const string AMedida = "A medida";

    public const int CuotaDiariaPrueba = 20;
    public const int CuotaMensualStarter = 5_000;
    public const int CuotaMensualGrowth = 25_000;
    public const int CuotaMensualMaximaPersonalizada = 10_000_000;

    public const decimal PrecioReferenciaUsdStarter = 19m;
    public const decimal PrecioReferenciaUsdGrowth = 35m;

    public static IReadOnlyList<string> Todos { get; } =
        [Prueba, Starter, Growth, AMedida];

    public static string? Normalizar(string? plan)
    {
        if (string.IsNullOrWhiteSpace(plan))
        {
            return null;
        }

        string value = plan.Trim();

        if (string.Equals(
                value,
                Prueba,
                StringComparison.OrdinalIgnoreCase))
        {
            return Prueba;
        }

        if (string.Equals(
                value,
                Starter,
                StringComparison.OrdinalIgnoreCase))
        {
            return Starter;
        }

        if (string.Equals(
                value,
                Growth,
                StringComparison.OrdinalIgnoreCase))
        {
            return Growth;
        }

        if (string.Equals(
                value,
                AMedida,
                StringComparison.OrdinalIgnoreCase))
        {
            return AMedida;
        }

        // Compatibilidad temporal con datos creados por 2.0.x.
        if (string.Equals(
                value,
                "Comercial",
                StringComparison.OrdinalIgnoreCase))
        {
            return Starter;
        }

        return null;
    }

    public static bool EsPrueba(string? plan)
    {
        return string.Equals(
            Normalizar(plan),
            Prueba,
            StringComparison.Ordinal);
    }

    public static int? ObtenerCuotaMensualFija(string? plan)
    {
        return Normalizar(plan) switch
        {
            Starter => CuotaMensualStarter,
            Growth => CuotaMensualGrowth,
            _ => null
        };
    }

    public static bool TryObtenerPeriodo(
        ApiCliente cliente,
        DateTime nowUtc,
        out ApiPeriodoCuota periodo)
    {
        periodo = default;

        string? plan = Normalizar(cliente.Plan);

        if (plan is null)
        {
            return false;
        }

        DateOnly todayUtc = DateOnly.FromDateTime(nowUtc);

        if (string.Equals(
                plan,
                Prueba,
                StringComparison.Ordinal))
        {
            if (cliente.CuotaDiaria < 1)
            {
                return false;
            }

            periodo = new ApiPeriodoCuota(
                PeriodoCuotaApi.Diario,
                todayUtc,
                todayUtc.AddDays(1),
                cliente.CuotaDiaria);

            return true;
        }

        int cuotaMensual = cliente.CuotaMensual.GetValueOrDefault();

        if (cuotaMensual < 1)
        {
            return false;
        }

        DateOnly inicioMesUtc =
            new(todayUtc.Year, todayUtc.Month, 1);

        DateOnly inicioPlanUtc = DateOnly.FromDateTime(
            cliente.FechaInicioPlanUtc
            ?? cliente.FechaCreacionUtc);

        DateOnly inicioPeriodoUtc =
            inicioPlanUtc > inicioMesUtc
                ? inicioPlanUtc
                : inicioMesUtc;

        if (inicioPeriodoUtc > todayUtc)
        {
            return false;
        }

        periodo = new ApiPeriodoCuota(
            PeriodoCuotaApi.Mensual,
            inicioPeriodoUtc,
            inicioMesUtc.AddMonths(1),
            cuotaMensual);

        return true;
    }
}
