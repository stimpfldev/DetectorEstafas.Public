using System.Net;
using System.Net.Mail;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Correo;

public sealed class SmtpCorreoRegistroService : ICorreoRegistroService
{
    private readonly CorreoOptions _options;

    public SmtpCorreoRegistroService(
        IOptions<CorreoOptions> options)
    {
        _options = options.Value;
    }

    public async Task EnviarConfirmacionAsync(
        string destinatario,
        string enlaceConfirmacion,
        CancellationToken cancellationToken = default)
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
            Subject = "Confirmá tu correo - AlertaEstafa",
            Body =
                "Confirmá tu correo ingresando al siguiente enlace:\n\n" +
                enlaceConfirmacion +
                "\n\nSi no creaste esta cuenta, ignorá este mensaje.",
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
        await cliente.SendMailAsync(mensaje, cancellationToken);
    }
}
