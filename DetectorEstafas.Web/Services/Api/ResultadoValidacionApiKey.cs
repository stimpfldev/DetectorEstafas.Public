using DetectorEstafas.Web.Models.ApiComercial;

namespace DetectorEstafas.Web.Services.Api;

public enum EstadoValidacionApiKey
{
    Valida = 1,
    Invalida = 2,
    CuotaAgotada = 3,
    PruebaExpirada = 4
}

public sealed class ResultadoValidacionApiKey
{
    public EstadoValidacionApiKey Estado { get; init; }

    public int ApiClienteId { get; init; }

    public string NombreCliente { get; init; } = string.Empty;

    public PeriodoCuotaApi Periodo { get; init; }

    public int Limite { get; init; }

    public int ConsumidasPeriodo { get; init; }

    public DateTime ReiniciaUtc { get; init; }

    public int Restantes =>
        Math.Max(0, Limite - ConsumidasPeriodo);
}
