using System.Security.Cryptography;
using System.Text;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Comercial.MercadoPago;

public sealed class MercadoPagoWebhookSignatureValidator :
    IMercadoPagoWebhookSignatureValidator
{
    private readonly MercadoPagoOptions _options;

    public MercadoPagoWebhookSignatureValidator(
        IOptions<MercadoPagoOptions> options)
    {
        _options = options.Value;
    }

    public bool EsValida(
        string? xSignature,
        string? xRequestId,
        string? dataId)
    {
        if (!_options.Enabled ||
            string.IsNullOrWhiteSpace(_options.WebhookSecret) ||
            string.IsNullOrWhiteSpace(xSignature))
        {
            return false;
        }

        string? timestamp = null;
        string? suppliedHash = null;

        foreach (string part in xSignature.Split(','))
        {
            string[] pair = part.Trim().Split(
                '=',
                2,
                StringSplitOptions.TrimEntries);

            if (pair.Length != 2)
            {
                continue;
            }

            if (string.Equals(
                    pair[0],
                    "ts",
                    StringComparison.OrdinalIgnoreCase))
            {
                timestamp = pair[1];
            }
            else if (string.Equals(
                         pair[0],
                         "v1",
                         StringComparison.OrdinalIgnoreCase))
            {
                suppliedHash = pair[1];
            }
        }

        if (string.IsNullOrWhiteSpace(timestamp) ||
            string.IsNullOrWhiteSpace(suppliedHash))
        {
            return false;
        }

        StringBuilder manifest = new();

        if (!string.IsNullOrWhiteSpace(dataId))
        {
            manifest.Append("id:")
                .Append(dataId)
                .Append(';');
        }

        if (!string.IsNullOrWhiteSpace(xRequestId))
        {
            manifest.Append("request-id:")
                .Append(xRequestId)
                .Append(';');
        }

        manifest.Append("ts:")
            .Append(timestamp)
            .Append(';');

        byte[] expectedBytes;

        using (HMACSHA256 hmac = new(
                   Encoding.UTF8.GetBytes(
                       _options.WebhookSecret.Trim())))
        {
            expectedBytes = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(
                    manifest.ToString()));
        }

        string expectedHash =
            Convert.ToHexString(expectedBytes)
                .ToLowerInvariant();

        byte[] suppliedBytes =
            Encoding.ASCII.GetBytes(
                suppliedHash.Trim().ToLowerInvariant());

        byte[] calculatedBytes =
            Encoding.ASCII.GetBytes(expectedHash);

        return suppliedBytes.Length ==
                   calculatedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(
                   suppliedBytes,
                   calculatedBytes);
    }
}
