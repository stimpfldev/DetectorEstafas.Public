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

    public int CuotaDiaria { get; init; }

    public int ConsumidasHoy { get; init; }

    public int Restantes => Math.Max(0, CuotaDiaria - ConsumidasHoy);
}
