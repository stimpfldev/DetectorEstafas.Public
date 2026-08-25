using System.Text.Json;
using DetectorEstafas.Web.Services.Comercial;
using DetectorEstafas.Web.Services.Comercial.MercadoPago;
using Microsoft.AspNetCore.Mvc;

namespace DetectorEstafas.Web.Controllers;

[ApiController]
[Route("webhooks/mercadopago")]
[IgnoreAntiforgeryToken]
public sealed class MercadoPagoWebhookController : ControllerBase
{
    private readonly IMercadoPagoWebhookSignatureValidator _signatureValidator;
    private readonly IComercializacionApiService _comercializacion;

    public MercadoPagoWebhookController(
        IMercadoPagoWebhookSignatureValidator signatureValidator,
        IComercializacionApiService comercializacion)
    {
        _signatureValidator = signatureValidator;
        _comercializacion = comercializacion;
    }

    [HttpPost]
    public async Task<IActionResult> Recibir(
        CancellationToken cancellationToken)
    {
        string? xSignature =
            Request.Headers["x-signature"].FirstOrDefault();
        string? xRequestId =
            Request.Headers["x-request-id"].FirstOrDefault();
        string? dataIdQuery =
            Request.Query["data.id"].FirstOrDefault();

        if (!_signatureValidator.EsValida(
                xSignature,
                xRequestId,
                dataIdQuery))
        {
            return Unauthorized();
        }

        using JsonDocument document =
            await JsonDocument.ParseAsync(
                Request.Body,
                cancellationToken: cancellationToken);

        JsonElement root = document.RootElement;

        string? eventoId = ObtenerString(root, "id");
        string? tipo = ObtenerString(root, "type");
        string accion =
            ObtenerString(root, "action") ?? string.Empty;
        string? recursoId = dataIdQuery;

        if (string.IsNullOrWhiteSpace(recursoId) &&
            root.TryGetProperty(
                "data",
                out JsonElement data) &&
            data.ValueKind == JsonValueKind.Object)
        {
            recursoId = ObtenerString(data, "id");
        }

        if (string.IsNullOrWhiteSpace(eventoId) ||
            string.IsNullOrWhiteSpace(tipo) ||
            string.IsNullOrWhiteSpace(recursoId))
        {
            return BadRequest();
        }

        await _comercializacion.ProcesarWebhookAsync(
            eventoId,
            tipo,
            accion,
            recursoId,
            $"{Request.Scheme}://{Request.Host}{Request.PathBase}",
            cancellationToken);

        return Ok();
    }

    private static string? ObtenerString(
        JsonElement element,
        string propertyName)
    {
        if (!element.TryGetProperty(
                propertyName,
                out JsonElement value) ||
            value.ValueKind is JsonValueKind.Null or
                JsonValueKind.Undefined)
        {
            return null;
        }

        return value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : value.ToString();
    }
}
