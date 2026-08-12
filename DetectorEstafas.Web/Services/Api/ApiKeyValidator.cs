using System.Data;
using System.Security.Cryptography;
using System.Text;
using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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

    public async Task<ResultadoValidacionApiKey>
        ValidarYRegistrarConsumoAsync(
            string? apiKey,
            CancellationToken cancellationToken)
    {
        if (!_options.Enabled ||
            string.IsNullOrWhiteSpace(apiKey))
        {
            return Invalida();
        }

        string normalizedKey = apiKey.Trim();

        byte[] suppliedHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(normalizedKey));

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
            CryptographicOperations.FixedTimeEquals(
                item.Hash,
                suppliedHash));

        if (matched is null)
        {
            matched = await ImportarClaveConfiguradaAsync(
                normalizedKey,
                suppliedHash,
                prefix,
                cancellationToken);
        }

        if (matched is null ||
            !matched.Habilitada ||
            matched.FechaRevocacionUtc is not null ||
            !matched.Cliente.Habilitado)
        {
            return Invalida();
        }

        DateTime nowUtc = DateTime.UtcNow;

        if (PruebaExpirada(
                matched.Cliente,
                nowUtc))
        {
            return new ResultadoValidacionApiKey
            {
                Estado =
                    EstadoValidacionApiKey.PruebaExpirada,
                ApiClienteId = matched.ApiClienteId,
                NombreCliente = matched.Cliente.Nombre
            };
        }

        if (!ApiPlanes.TryObtenerPeriodo(
                matched.Cliente,
                nowUtc,
                out ApiPeriodoCuota periodo))
        {
            return Invalida();
        }

        IDbContextTransaction? transaction = null;

        if (_dbContext.Database.IsRelational())
        {
            transaction =
                await _dbContext.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable,
                    cancellationToken);
        }

        try
        {
            int alreadyUsed =
                await _dbContext.ApiConsumosDiarios
                    .Where(item =>
                        item.ApiClienteId ==
                            matched.ApiClienteId &&
                        item.FechaUtc >= periodo.DesdeUtc &&
                        item.FechaUtc <
                            periodo.HastaUtcExclusiva)
                    .SumAsync(
                        item =>
                            (int?)item.CantidadSolicitudes,
                        cancellationToken)
                ?? 0;

            if (alreadyUsed >= periodo.Limite)
            {
                return CrearResultado(
                    EstadoValidacionApiKey.CuotaAgotada,
                    matched,
                    periodo,
                    alreadyUsed);
            }

            DateOnly todayUtc =
                DateOnly.FromDateTime(nowUtc);

            ApiConsumoDiario? usage =
                await _dbContext.ApiConsumosDiarios
                    .SingleOrDefaultAsync(
                        item =>
                            item.ApiClienteId ==
                                matched.ApiClienteId &&
                            item.FechaUtc == todayUtc,
                        cancellationToken);

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

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            if (transaction is not null)
            {
                await transaction.CommitAsync(
                    cancellationToken);
            }

            return CrearResultado(
                EstadoValidacionApiKey.Valida,
                matched,
                periodo,
                alreadyUsed + 1);
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    private bool PruebaExpirada(
        ApiCliente cliente,
        DateTime nowUtc)
    {
        if (!ApiPlanes.EsPrueba(cliente.Plan) ||
            _options.TrialDays <= 0)
        {
            return false;
        }

        DateTime inicioPruebaUtc =
            cliente.FechaInicioPlanUtc
            ?? cliente.FechaCreacionUtc;

        return inicioPruebaUtc
            .AddDays(_options.TrialDays) <= nowUtc;
    }

    private async Task<ApiClave?>
        ImportarClaveConfiguradaAsync(
            string normalizedKey,
            byte[] hash,
            string prefix,
            CancellationToken cancellationToken)
    {
        ApiKeyOptions? configured = _options.Keys
            .Where(item =>
                item.Enabled &&
                !string.IsNullOrWhiteSpace(item.Key))
            .FirstOrDefault(item =>
                ClavesIguales(
                    normalizedKey,
                    item.Key.Trim()));

        if (configured is null)
        {
            return null;
        }

        string clientName =
            string.IsNullOrWhiteSpace(configured.Name)
                ? "cliente-api"
                : configured.Name.Trim();

        ApiCliente? client = await _dbContext.ApiClientes
            .Include(item => item.Claves)
            .SingleOrDefaultAsync(
                item => item.Nombre == clientName,
                cancellationToken);

        if (client is null)
        {
            DateTime nowUtc = DateTime.UtcNow;

            client = new ApiCliente
            {
                Nombre = clientName,
                Plan = ApiPlanes.Prueba,
                CuotaDiaria =
                    ApiPlanes.CuotaDiariaPrueba,
                CuotaMensual = null,
                Habilitado = true,
                FechaCreacionUtc = nowUtc,
                FechaInicioPlanUtc = nowUtc
            };

            _dbContext.ApiClientes.Add(client);
        }
        else if (!client.Habilitado)
        {
            return null;
        }

        ApiClave? existing =
            client.Claves.FirstOrDefault(item =>
                item.Hash.Length == hash.Length &&
                CryptographicOperations.FixedTimeEquals(
                    item.Hash,
                    hash));

        if (existing is not null)
        {
            if (!existing.Habilitada ||
                existing.FechaRevocacionUtc is not null)
            {
                return null;
            }

            return existing;
        }

        ApiClave nuevaClave = new()
        {
            Cliente = client,
            Prefijo = prefix,
            Hash = hash,
            Habilitada = true,
            FechaCreacionUtc = DateTime.UtcNow
        };

        _dbContext.ApiClaves.Add(nuevaClave);

        await _dbContext.SaveChangesAsync(
            cancellationToken);

        return nuevaClave;
    }

    private static ResultadoValidacionApiKey
        CrearResultado(
            EstadoValidacionApiKey estado,
            ApiClave matched,
            ApiPeriodoCuota periodo,
            int consumidas)
    {
        return new ResultadoValidacionApiKey
        {
            Estado = estado,
            ApiClienteId = matched.ApiClienteId,
            NombreCliente = matched.Cliente.Nombre,
            Periodo = periodo.Periodo,
            Limite = periodo.Limite,
            ConsumidasPeriodo = consumidas,
            ReiniciaUtc = periodo.ReiniciaUtc
        };
    }

    private static bool ClavesIguales(
        string supplied,
        string expected)
    {
        byte[] suppliedBytes =
            Encoding.UTF8.GetBytes(supplied);

        byte[] expectedBytes =
            Encoding.UTF8.GetBytes(expected);

        return suppliedBytes.Length ==
                   expectedBytes.Length &&
               CryptographicOperations.FixedTimeEquals(
                   suppliedBytes,
                   expectedBytes);
    }

    private static string ObtenerPrefijo(
        string apiKey)
    {
        return apiKey[
            ..Math.Min(PrefixLength, apiKey.Length)];
    }

    private static ResultadoValidacionApiKey
        Invalida()
    {
        return new ResultadoValidacionApiKey
        {
            Estado = EstadoValidacionApiKey.Invalida
        };
    }
}
