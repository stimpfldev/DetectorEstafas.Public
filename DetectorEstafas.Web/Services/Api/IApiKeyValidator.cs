namespace DetectorEstafas.Web.Services.Api;

public interface IApiKeyValidator
{
    Task<ResultadoValidacionApiKey> ValidarYRegistrarConsumoAsync(
        string? apiKey,
        CancellationToken cancellationToken);
}
