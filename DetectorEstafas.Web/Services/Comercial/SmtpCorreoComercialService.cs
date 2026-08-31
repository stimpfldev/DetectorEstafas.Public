using System.Net;
using System.Net.Mail;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Comercial;

public sealed class SmtpCorreoComercialService :
    ICorreoComercialService
{
    private readonly CorreoOptions _options;

    public SmtpCorreoComercialService(
        IOptions<CorreoOptions> options)
    {
        _options = options.Value;
    }

    public async Task EnviarAccesoListoAsync(
        string destinatario,
        string plan,
        string? enlaceEntregaClave,
        bool mantieneClaveExistente,
        CancellationToken cancellationToken = default)
    {
        string acceso = mantieneClaveExistente
            ? "Tu API key actual continúa vigente."
            : string.IsNullOrWhiteSpace(enlaceEntregaClave)
                ? "Tu acceso fue activado. Si necesitás regenerar la API key, contactá a soporte."
                : "Tu API key está disponible una sola vez en este enlace temporal:\n\n" +
                  enlaceEntregaClave;

        string body =
            $"Tu acceso a Detector de Estafas está listo.\n\n" +
            $"Plan: {plan}\n\n" +
            acceso +
            "\n\nEndpoint principal: POST /api/v1/analisis\n" +
            "Header: X-Api-Key\n\n" +
            "No compartas tu API key ni la incluyas en código público.";

        await EnviarAsync(
            destinatario,
            "Tu acceso API está listo - Detector de Estafas",
            body,
            cancellationToken);

        if (!ApiPlanes.EsPrueba(plan) &&
            !string.IsNullOrWhiteSpace(_options.RemitenteEmail) &&
            !string.Equals(
                destinatario,
                _options.RemitenteEmail,
                StringComparison.OrdinalIgnoreCase))
        {
            string avisoInterno =
                "Se activó automáticamente una nueva suscripción paga en AlertaEstafa.\n\n" +
                $"Cliente: {destinatario}\n" +
                $"Plan: {plan}\n\n" +
                "El acceso API ya fue provisionado automáticamente. " +
                "No requiere alta manual.";

            try
            {
                await EnviarAsync(
                    _options.RemitenteEmail,
                    "Nueva suscripción paga activada - AlertaEstafa",
                    avisoInterno,
                    cancellationToken);
            }
            catch
            {
                // La alerta interna no debe bloquear la activación ya confirmada.
            }
        }
    }

    public Task EnviarProblemaPagoAsync(
        string destinatario,
        string plan,
        CancellationToken cancellationToken = default)
    {
        string body =
            "No pudimos confirmar el último cobro de tu suscripción de Detector de Estafas.\n\n" +
            $"Plan: {plan}\n\n" +
            "Tu acceso permanecerá disponible durante el período de gracia configurado. " +
            "Regularizá el pago desde Mercado Pago para evitar la suspensión del servicio.";

        return EnviarAsync(
            destinatario,
            "Problema con el pago - Detector de Estafas",
            body,
            cancellationToken);
    }

    public Task EnviarCancelacionAsync(
        string destinatario,
        string plan,
        DateTime? accesoHastaUtc,
        CancellationToken cancellationToken = default)
    {
        string hasta = accesoHastaUtc.HasValue
            ? $"El acceso permanecerá activo hasta {accesoHastaUtc.Value:dd/MM/yyyy HH:mm} UTC."
            : "El acceso será deshabilitado al finalizar el período vigente.";

        string body =
            "Se registró la cancelación de tu suscripción de Detector de Estafas.\n\n" +
            $"Plan: {plan}\n" +
            hasta;

        return EnviarAsync(
            destinatario,
            "Suscripción cancelada - Detector de Estafas",
            body,
            cancellationToken);
    }

    private async Task EnviarAsync(
        string destinatario,
        string asunto,
        string cuerpo,
        CancellationToken cancellationToken)
    {
        if (_options.ModoDesarrollo)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_options.SmtpHost) ||
            string.IsNullOrWhiteSpace(_options.RemitenteEmail))
        {
            throw new InvalidOperationException(
                "La configuración SMTP está incompleta.");
        }

        using MailMessage mensaje = new()
        {
            From = new MailAddress(
                _options.RemitenteEmail,
                _options.RemitenteNombre),
            Subject = asunto,
            Body = cuerpo,
            IsBodyHtml = false
        };

        mensaje.To.Add(destinatario);

        using SmtpClient cliente = new(
            _options.SmtpHost,
            _options.SmtpPort)
        {
            EnableSsl = _options.UsarSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.Usuario))
        {
            cliente.Credentials = new NetworkCredential(
                _options.Usuario,
                _options.Password);
        }

        cancellationToken.ThrowIfCancellationRequested();
        await cliente.SendMailAsync(
            mensaje,
            cancellationToken);
    }
}
