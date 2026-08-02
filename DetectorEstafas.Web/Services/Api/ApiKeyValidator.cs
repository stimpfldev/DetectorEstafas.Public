using System.Security.Cryptography;
using System.Text;
using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Api;

public sealed class ApiKeyValidator : IApiKeyValidator
{
    private const int PrefixLength = 8;

    private readonly DetectorEstafasDbContext _dbContext;
    private readonly ApiComercialOptions _options;

    public ApiKeyValidator(
        DetectorEstafasDbContext dbContext,
        IOptions<ApiComercialOptions> options)
    {
        _dbContext = dbContext;
        _options = options.Value;
    }

    public async Task<ResultadoValidacionApiKey> ValidarYRegistrarConsumoAsync(
        string? apiKey,
        CancellationToken cancellationToken)
    {
        if (!_options.Enabled || string.IsNullOrWhiteSpace(apiKey))
        {
            return Invalida();
        }

        string normalizedKey = apiKey.Trim();
        byte[] suppliedHash = SHA256.HashData(Encoding.UTF8.GetBytes(normalizedKey));
        string prefix = ObtenerPrefijo(normalizedKey);

        List<ApiClave> candidates = await _dbContext.ApiClaves
            .Include(item => item.Cliente)
            .Where(item =>
                item.Prefijo == prefix &&
                item.Habilitada &&
                item.FechaRevocacionUtc == null &&
                item.Cliente.Habilitado)
            .ToListAsync(cancellationToken);

        ApiClave? matched = candidates.FirstOrDefault(item =>
            item.Hash.Length == suppliedHash.Length &&
            CryptographicOperations.FixedTimeEquals(item.Hash, suppliedHash));

        if (matched is null)
        {
            matched = await ImportarClaveConfiguradaAsync(
                normalizedKey,
                suppliedHash,
                prefix,
                cancellationToken);
        }

        if (matched is null)
        {
            return Invalida();
        }

        DateTime nowUtc = DateTime.UtcNow;
        DateOnly todayUtc = DateOnly.FromDateTime(nowUtc);

        ApiConsumoDiario? usage = await _dbContext.ApiConsumosDiarios
            .SingleOrDefaultAsync(
                item =>
                    item.ApiClienteId == matched.ApiClienteId &&
                    item.FechaUtc == todayUtc,
                cancellationToken);

        int alreadyUsed = usage?.CantidadSolicitudes ?? 0;

        if (alreadyUsed >= matched.Cliente.CuotaDiaria)
        {
            return new ResultadoValidacionApiKey
            {
                Estado = EstadoValidacionApiKey.CuotaAgotada,
                ApiClienteId = matched.ApiClienteId,
                NombreCliente = matched.Cliente.Nombre,
                CuotaDiaria = matched.Cliente.CuotaDiaria,
                ConsumidasHoy = alreadyUsed
            };
        }

        if (usage is null)
        {
            usage = new ApiConsumoDiario
            {
                ApiClienteId = matched.ApiClienteId,
                FechaUtc = todayUtc,
                CantidadSolicitudes = 1,
                UltimaSolicitudUtc = nowUtc
            };

            _dbContext.ApiConsumosDiarios.Add(usage);
        }
        else
        {
            usage.CantidadSolicitudes++;
            usage.UltimaSolicitudUtc = nowUtc;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ResultadoValidacionApiKey
        {
            Estado = EstadoValidacionApiKey.Valida,
            ApiClienteId = matched.ApiClienteId,
            NombreCliente = matched.Cliente.Nombre,
            CuotaDiaria = matched.Cliente.CuotaDiaria,
            ConsumidasHoy = usage.CantidadSolicitudes
        };
    }

    private async Task<ApiClave?> ImportarClaveConfiguradaAsync(
        string normalizedKey,
        byte[] hash,
        string prefix,
        CancellationToken cancellationToken)
    {
        ApiKeyOptions? configured = _options.Keys
            .Where(item => item.Enabled && !string.IsNullOrWhiteSpace(item.Key))
            .FirstOrDefault(item => ClavesIguales(normalizedKey, item.Key.Trim()));

        if (configured is null)
        {
            return null;
        }

        string clientName = string.IsNullOrWhiteSpace(configured.Name)
            ? "cliente-api"
            : configured.Name.Trim();

        ApiCliente? client = await _dbContext.ApiClientes
            .Include(item => item.Claves)
            .SingleOrDefaultAsync(
                item => item.Nombre == clientName,
                cancellationToken);

        if (client is null)
        {
            client = new ApiCliente
            {
                Nombre = clientName,
                Plan = "Prueba",
                CuotaDiaria = Math.Max(1, _options.DefaultDailyQuota),
                Habilitado = true,
                FechaCreacionUtc = DateTime.UtcNow
            };

            _dbContext.ApiClientes.Add(client);
        }

        ApiClave? existing = client.Claves.FirstOrDefault(item =>
            item.Hash.Length == hash.Length &&
            CryptographicOperations.FixedTimeEquals(item.Hash, hash));

        if (existing is null)
        {
            existing = new ApiClave
            {
                Cliente = client,
                Prefijo = prefix,
                Hash = hash,
                Habilitada = true,
                FechaCreacionUtc = DateTime.UtcNow
            };

            _dbContext.ApiClaves.Add(existing);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return existing;
    }

    private static bool ClavesIguales(string supplied, string expected)
    {
        byte[] suppliedBytes = Encoding.UTF8.GetBytes(supplied);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);

        return suppliedBytes.Length == expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(suppliedBytes, expectedBytes);
    }

    private static string ObtenerPrefijo(string apiKey)
    {
        return apiKey[..Math.Min(PrefixLength, apiKey.Length)];
    }

    private static ResultadoValidacionApiKey Invalida()
    {
        return new ResultadoValidacionApiKey
        {
            Estado = EstadoValidacionApiKey.Invalida
        };
    }
}
