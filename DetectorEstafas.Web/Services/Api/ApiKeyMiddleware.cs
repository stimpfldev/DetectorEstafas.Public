using System.Text.Json;

namespace DetectorEstafas.Web.Services.Api;

public sealed class ApiKeyMiddleware
{
    public const string HeaderName = "X-Api-Key";
    public const string ClientItemName = "ApiClientName";
    public const string ClientIdItemName = "ApiClientId";

    private readonly RequestDelegate _next;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IApiKeyValidator validator)
    {
        if (!context.Request.Path.StartsWithSegments("/api/v1"))
        {
            await _next(context);
            return;
        }

        string? apiKey = context.Request.Headers[HeaderName].FirstOrDefault();

        ResultadoValidacionApiKey result =
            await validator.ValidarYRegistrarConsumoAsync(
                apiKey,
                context.RequestAborted);

        if (result.Estado == EstadoValidacionApiKey.Invalida)
        {
            await EscribirErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "api_key_invalida",
                "La API key es inexistente, inválida o está deshabilitada.");

            return;
        }

        context.Response.Headers["X-RateLimit-Limit"] =
            result.CuotaDiaria.ToString();

        context.Response.Headers["X-RateLimit-Remaining"] =
            result.Restantes.ToString();

        if (result.Estado == EstadoValidacionApiKey.CuotaAgotada)
        {
            await EscribirErrorAsync(
                context,
                StatusCodes.Status429TooManyRequests,
                "cuota_diaria_agotada",
                "Se alcanzó la cuota diaria asignada a esta API key.");

            return;
        }

        context.Items[ClientItemName] = result.NombreCliente;
        context.Items[ClientIdItemName] = result.ApiClienteId;

        await _next(context);
    }

    private static async Task EscribirErrorAsync(
        HttpContext context,
        int statusCode,
        string code,
        string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(new
            {
                ok = false,
                codigo = code,
                mensaje = message
            }));
    }
}
