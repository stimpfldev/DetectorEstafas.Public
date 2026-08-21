using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using DetectorEstafas.Web.Models;
using Microsoft.Extensions.Caching.Memory;

namespace DetectorEstafas.Web.Services;

public class RdapService : IRdapService
{
    private static readonly Regex DominioArRegex = new(
        @"^[a-z0-9](?:[a-z0-9.-]*[a-z0-9])?\.ar$",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant |
        RegexOptions.Compiled);

    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<RdapService> _logger;

    public RdapService(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        ILogger<RdapService> logger)
    {
        _httpClient = httpClient;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<ResultadoRdap> ConsultarDominioAsync(
        string dominio,
        CancellationToken cancellationToken)
    {
        string dominioNormalizado = dominio
            .Trim()
            .TrimEnd('.')
            .ToLowerInvariant();

        if (!DominioArRegex.IsMatch(dominioNormalizado))
        {
            return new ResultadoRdap
            {
                FueConsultado = false,
                Estado =
                    "La consulta registral se aplica solamente a dominios .ar."
            };
        }

        string cacheKey = $"rdap:{dominioNormalizado}";

        if (_memoryCache.TryGetValue(
                cacheKey,
                out ResultadoRdap? resultadoCache) &&
            resultadoCache is not null)
        {
            return resultadoCache;
        }

        ResultadoRdap resultado = new()
        {
            FueConsultado = true
        };

        using CancellationTokenSource timeout =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        timeout.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            string url =
                $"https://rdap.nic.ar/domain/{Uri.EscapeDataString(dominioNormalizado)}";

            using HttpRequestMessage request = new(
                HttpMethod.Get,
                url);

            request.Headers.Accept.Clear();
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/rdap+json"));
            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json", 0.9));

            request.Headers.UserAgent.Clear();
            request.Headers.UserAgent.ParseAdd(
                "Mozilla/5.0 DetectorEstafas/2.1");

            using HttpResponseMessage response =
                await _httpClient.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    timeout.Token);

            _logger.LogDebug(
                "Consulta RDAP {Dominio}: HTTP {StatusCode} desde {RequestUri}.",
                dominioNormalizado,
                (int)response.StatusCode,
                response.RequestMessage?.RequestUri);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                resultado.Estado =
                    "El dominio no fue encontrado en el registro consultado.";
                resultado.PuntajeAdicional = 15;
                resultado.Senales.Add(
                    "El dominio no fue encontrado en el registro RDAP.");

                GuardarEnCache(
                    cacheKey,
                    resultado,
                    TimeSpan.FromHours(1));

                return resultado;
            }

            if (!response.IsSuccessStatusCode)
            {
                resultado.Estado =
                    "El registro del dominio no respondió correctamente.";

                GuardarEnCache(
                    cacheKey,
                    resultado,
                    TimeSpan.FromMinutes(5));

                return resultado;
            }

            await using Stream contenido =
                await response.Content.ReadAsStreamAsync(timeout.Token);

            using JsonDocument documento =
                await JsonDocument.ParseAsync(
                    contenido,
                    cancellationToken: timeout.Token);

            resultado.Encontrado = true;
            resultado.Estado =
                "Dominio encontrado en el registro oficial.";

            DateTime? fechaRegistro =
                ObtenerFechaRegistro(documento.RootElement);

            resultado.FechaRegistroUtc = fechaRegistro;

            if (!fechaRegistro.HasValue)
            {
                resultado.Estado =
                    "Dominio registrado, sin fecha disponible.";

                GuardarEnCache(
                    cacheKey,
                    resultado,
                    TimeSpan.FromHours(12));

                return resultado;
            }

            int antiguedadDias = Math.Max(
                0,
                (DateTime.UtcNow - fechaRegistro.Value).Days);

            resultado.AntiguedadDias = antiguedadDias;

            if (antiguedadDias <= 30)
            {
                resultado.PuntajeAdicional = 25;
                resultado.Senales.Add(
                    "El dominio fue registrado hace menos de 30 días.");
            }
            else if (antiguedadDias <= 180)
            {
                resultado.PuntajeAdicional = 15;
                resultado.Senales.Add(
                    "El dominio fue registrado hace menos de 6 meses.");
            }
            else if (antiguedadDias <= 365)
            {
                resultado.PuntajeAdicional = 8;
                resultado.Senales.Add(
                    "El dominio fue registrado hace menos de un año.");
            }

            GuardarEnCache(
                cacheKey,
                resultado,
                TimeSpan.FromHours(24));

            return resultado;
        }
        catch (OperationCanceledException)
            when (!cancellationToken.IsCancellationRequested)
        {
            resultado.Estado =
                "El registro oficial no respondió a tiempo.";

            GuardarEnCache(
                cacheKey,
                resultado,
                TimeSpan.FromMinutes(2));

            return resultado;
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "No se pudo consultar RDAP para {Dominio}.",
                dominioNormalizado);

            resultado.Estado =
                "El registro oficial no está disponible.";

            GuardarEnCache(
                cacheKey,
                resultado,
                TimeSpan.FromMinutes(5));

            return resultado;
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Respuesta RDAP inválida para {Dominio}.",
                dominioNormalizado);

            resultado.Estado =
                "La respuesta registral no pudo interpretarse.";

            GuardarEnCache(
                cacheKey,
                resultado,
                TimeSpan.FromMinutes(10));

            return resultado;
        }
    }

    private void GuardarEnCache(
        string cacheKey,
        ResultadoRdap resultado,
        TimeSpan duracion)
    {
        _memoryCache.Set(
            cacheKey,
            resultado,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = duracion,
                Size = 1
            });
    }

    private static DateTime? ObtenerFechaRegistro(JsonElement raiz)
    {
        if (!raiz.TryGetProperty(
                "events",
                out JsonElement eventos) ||
            eventos.ValueKind != JsonValueKind.Array)
        {
            return null;
        }

        foreach (JsonElement evento in eventos.EnumerateArray())
        {
            if (!evento.TryGetProperty(
                    "eventAction",
                    out JsonElement accion) ||
                !string.Equals(
                    accion.GetString(),
                    "registration",
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!evento.TryGetProperty(
                    "eventDate",
                    out JsonElement fecha))
            {
                continue;
            }

            if (DateTime.TryParse(
                    fecha.GetString(),
                    out DateTime fechaRegistro))
            {
                return fechaRegistro.ToUniversalTime();
            }
        }

        return null;
    }
}
