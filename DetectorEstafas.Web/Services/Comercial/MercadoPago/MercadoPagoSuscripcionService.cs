using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Comercial.MercadoPago;

public sealed class MercadoPagoSuscripcionService :
    IMercadoPagoSuscripcionService
{
    private readonly HttpClient _httpClient;
    private readonly MercadoPagoOptions _options;
    private readonly ILogger<MercadoPagoSuscripcionService> _logger;

    public MercadoPagoSuscripcionService(
        HttpClient httpClient,
        IOptions<MercadoPagoOptions> options,
        ILogger<MercadoPagoSuscripcionService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<MercadoPagoPlanSuscripcionCreado>
        CrearPlanPendienteAsync(
            string plan,
            decimal monto,
            string moneda,
            string backUrl,
            CancellationToken cancellationToken)
    {
        ValidarConfiguracion();

        object payload = new
        {
            reason = $"Detector de Estafas - Plan {plan}",
            auto_recurring = new
            {
                frequency = 1,
                frequency_type = "months",
                transaction_amount = monto,
                currency_id = moneda
            },
            back_url = backUrl
        };

        using HttpRequestMessage request =
            CrearRequest(HttpMethod.Post, "preapproval_plan");

        request.Content = JsonContent.Create(payload);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string detalle = await ObtenerDetalleErrorAsync(
                response,
                cancellationToken);

            _logger.LogWarning(
                "Mercado Pago rechazó la creación del plan de suscripción. HTTP {StatusCode}. {Detalle}",
                (int)response.StatusCode,
                detalle);

            throw new HttpRequestException(
                $"Mercado Pago devolvió HTTP {(int)response.StatusCode} al crear el plan de suscripción.");
        }

        using JsonDocument document =
            await JsonDocument.ParseAsync(
                await response.Content.ReadAsStreamAsync(
                    cancellationToken),
                cancellationToken: cancellationToken);

        JsonElement root = document.RootElement;

        string id = ObtenerString(root, "id")
            ?? throw new InvalidOperationException(
                "Mercado Pago no devolvió el ID del plan de suscripción.");

        string initPoint = ObtenerString(root, "init_point")
            ?? throw new InvalidOperationException(
                "Mercado Pago no devolvió el enlace de pago.");

        return new MercadoPagoPlanSuscripcionCreado(
            id,
            initPoint,
            ObtenerString(root, "status") ?? "active");
    }

    public async Task<MercadoPagoSuscripcionDetalle?>
        ObtenerSuscripcionAsync(
            string preapprovalId,
            CancellationToken cancellationToken)
    {
        JsonDocument? document = await ObtenerJsonAsync(
            $"preapproval/{Uri.EscapeDataString(preapprovalId)}",
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        using (document)
        {
            JsonElement root = document.RootElement;

            string? id = ObtenerString(root, "id");

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return new MercadoPagoSuscripcionDetalle(
                id,
                ObtenerString(root, "status") ?? string.Empty,
                ObtenerString(root, "external_reference")
                    ?? string.Empty,
                ObtenerString(root, "preapproval_plan_id"),
                ObtenerFecha(root, "next_payment_date"));
        }
    }

    public async Task<MercadoPagoPagoAutorizadoDetalle?>
        ObtenerPagoAutorizadoAsync(
            string authorizedPaymentId,
            CancellationToken cancellationToken)
    {
        JsonDocument? document = await ObtenerJsonAsync(
            $"authorized_payments/{Uri.EscapeDataString(authorizedPaymentId)}",
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        using (document)
        {
            JsonElement root = document.RootElement;

            string? id = ObtenerString(root, "id");
            string? preapprovalId =
                ObtenerString(root, "preapproval_id");

            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(preapprovalId))
            {
                return null;
            }

            string? paymentStatus = null;

            if (root.TryGetProperty(
                    "payment",
                    out JsonElement payment) &&
                payment.ValueKind == JsonValueKind.Object)
            {
                paymentStatus =
                    ObtenerString(payment, "status");
            }

            return new MercadoPagoPagoAutorizadoDetalle(
                id,
                preapprovalId,
                ObtenerString(root, "external_reference")
                    ?? string.Empty,
                paymentStatus,
                ObtenerFecha(root, "date_created"));
        }
    }

    public async Task<MercadoPagoPagoDetalle?> ObtenerPagoAsync(
        string paymentId,
        CancellationToken cancellationToken)
    {
        JsonDocument? document = await ObtenerJsonAsync(
            $"v1/payments/{Uri.EscapeDataString(paymentId)}",
            cancellationToken);

        if (document is null)
        {
            return null;
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            string? id = ObtenerString(root, "id");

            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            return new MercadoPagoPagoDetalle(
                id,
                ObtenerString(root, "external_reference")
                    ?? string.Empty,
                ObtenerString(root, "status")
                    ?? string.Empty,
                ObtenerFecha(root, "date_approved"));
        }
    }

    private async Task<JsonDocument?> ObtenerJsonAsync(
        string path,
        CancellationToken cancellationToken)
    {
        ValidarConfiguracion();

        using HttpRequestMessage request =
            CrearRequest(HttpMethod.Get, path);

        using HttpResponseMessage response =
            await _httpClient.SendAsync(
                request,
                cancellationToken);

        if (response.StatusCode ==
            System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            string detalle = await ObtenerDetalleErrorAsync(
                response,
                cancellationToken);

            _logger.LogWarning(
                "Mercado Pago rechazó la consulta del recurso. HTTP {StatusCode}. {Detalle}",
                (int)response.StatusCode,
                detalle);

            throw new HttpRequestException(
                $"Mercado Pago devolvió HTTP {(int)response.StatusCode} al consultar el recurso.");
        }

        return await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(
                cancellationToken),
            cancellationToken: cancellationToken);
    }

    private HttpRequestMessage CrearRequest(
        HttpMethod method,
        string path)
    {
        HttpRequestMessage request = new(method, path);
        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                _options.AccessToken.Trim());
        request.Headers.Accept.ParseAdd("application/json");
        return request;
    }

    private void ValidarConfiguracion()
    {
        if (!_options.Enabled ||
            string.IsNullOrWhiteSpace(_options.AccessToken))
        {
            throw new InvalidOperationException(
                "Mercado Pago no está configurado.");
        }
    }

    private static async Task<string> ObtenerDetalleErrorAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        string content = await response.Content.ReadAsStringAsync(
            cancellationToken);

        if (string.IsNullOrWhiteSpace(content))
        {
            return "Sin detalle adicional.";
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(content);
            JsonElement root = document.RootElement;
            List<string> partes = new();

            AgregarDetalle(partes, root, "error");
            AgregarDetalle(partes, root, "message");

            if (root.TryGetProperty(
                    "cause",
                    out JsonElement cause) &&
                cause.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement item in cause.EnumerateArray().Take(3))
                {
                    string? code = ObtenerString(item, "code");
                    string? description = ObtenerString(
                        item,
                        "description");

                    if (!string.IsNullOrWhiteSpace(code) ||
                        !string.IsNullOrWhiteSpace(description))
                    {
                        partes.Add(
                            $"cause={code ?? "-"}: {description ?? "-"}");
                    }
                }
            }

            return partes.Count > 0
                ? string.Join(" | ", partes)
                : "Respuesta de error sin detalle reconocido.";
        }
        catch (JsonException)
        {
            const int maxLength = 500;
            return content.Length <= maxLength
                ? content
                : content[..maxLength];
        }
    }

    private static void AgregarDetalle(
        List<string> partes,
        JsonElement root,
        string propertyName)
    {
        string? value = ObtenerString(root, propertyName);

        if (!string.IsNullOrWhiteSpace(value))
        {
            partes.Add($"{propertyName}={value}");
        }
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

    private static DateTime? ObtenerFecha(
        JsonElement element,
        string propertyName)
    {
        string? value = ObtenerString(
            element,
            propertyName);

        if (DateTimeOffset.TryParse(
                value,
                out DateTimeOffset parsed))
        {
            return parsed.UtcDateTime;
        }

        return null;
    }
}
