namespace DetectorEstafas.Web.Services.Comercial;

public enum EstadoProvisionamientoApi
{
    Creado = 1,
    Actualizado = 2,
    YaExistia = 3
}

public sealed record ProvisionamientoApiResultado(
    EstadoProvisionamientoApi Estado,
    int ApiClienteId,
    string? TokenEntrega,
    bool ClaveNueva);

public interface IProvisionamientoApiComercialService
{
    Task<ProvisionamientoApiResultado> CrearPruebaAsync(
        string nombre,
        string email,
        CancellationToken cancellationToken);

    Task<ProvisionamientoApiResultado> ActivarPlanPagadoAsync(
        string nombre,
        string email,
        string plan,
        CancellationToken cancellationToken);

    Task<string?> ConsumirEntregaClaveAsync(
        string token,
        CancellationToken cancellationToken);
}
