using System.Security.Cryptography;
using System.Text;
using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Options;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Services.Comercial;

public sealed class ProvisionamientoApiComercialService :
    IProvisionamientoApiComercialService
{
    private const int PrefixLength = 8;
    private const string ProtectorPurpose =
        "DetectorEstafas.ApiClaveEntrega.v1";

    private readonly DetectorEstafasDbContext _dbContext;
    private readonly IDataProtector _protector;
    private readonly ApiComercialOptions _options;

    public ProvisionamientoApiComercialService(
        DetectorEstafasDbContext dbContext,
        IDataProtectionProvider dataProtectionProvider,
        IOptions<ApiComercialOptions> options)
    {
        _dbContext = dbContext;
        _protector = dataProtectionProvider.CreateProtector(
            ProtectorPurpose);
        _options = options.Value;
    }

    public async Task<ProvisionamientoApiResultado>
        CrearPruebaAsync(
            string nombre,
            string email,
            CancellationToken cancellationToken)
    {
        string emailNormalizado = NormalizarEmail(email);

        ApiCliente? existente = await _dbContext.ApiClientes
            .Include(item => item.Claves)
            .SingleOrDefaultAsync(
                item => item.Email == emailNormalizado,
                cancellationToken);

        if (existente is not null)
        {
            return new ProvisionamientoApiResultado(
                EstadoProvisionamientoApi.YaExistia,
                existente.ApiClienteId,
                null,
                false);
        }

        DateTime nowUtc = DateTime.UtcNow;
        string nombreUnico = await CrearNombreUnicoAsync(
            nombre,
            emailNormalizado,
            cancellationToken);

        ApiCliente cliente = new()
        {
            Nombre = nombreUnico,
            Email = emailNormalizado,
            Plan = ApiPlanes.Prueba,
            CuotaDiaria = ApiPlanes.CuotaDiariaPrueba,
            CuotaMensual = null,
            Habilitado = true,
            FechaCreacionUtc = nowUtc,
            FechaInicioPlanUtc = nowUtc
        };

        _dbContext.ApiClientes.Add(cliente);

        (ApiClave _, string tokenEntrega) =
            CrearClaveYEntrega(cliente, nowUtc);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProvisionamientoApiResultado(
            EstadoProvisionamientoApi.Creado,
            cliente.ApiClienteId,
            tokenEntrega,
            true);
    }

    public async Task<ProvisionamientoApiResultado>
        ActivarPlanPagadoAsync(
            string nombre,
            string email,
            string plan,
            CancellationToken cancellationToken)
    {
        string? planNormalizado = ApiPlanes.Normalizar(plan);

        if (planNormalizado is null ||
            string.Equals(
                planNormalizado,
                ApiPlanes.Prueba,
                StringComparison.Ordinal) ||
            string.Equals(
                planNormalizado,
                ApiPlanes.AMedida,
                StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "El plan pago no es válido.",
                nameof(plan));
        }

        int? cuotaMensual =
            ApiPlanes.ObtenerCuotaMensualFija(planNormalizado);

        if (!cuotaMensual.HasValue)
        {
            throw new InvalidOperationException(
                "No se encontró la cuota mensual del plan.");
        }

        string emailNormalizado = NormalizarEmail(email);
        DateTime nowUtc = DateTime.UtcNow;

        ApiCliente? cliente = await _dbContext.ApiClientes
            .Include(item => item.Claves)
            .SingleOrDefaultAsync(
                item => item.Email == emailNormalizado,
                cancellationToken);

        bool creado = cliente is null;

        if (cliente is null)
        {
            string nombreUnico = await CrearNombreUnicoAsync(
                nombre,
                emailNormalizado,
                cancellationToken);

            cliente = new ApiCliente
            {
                Nombre = nombreUnico,
                Email = emailNormalizado,
                Plan = planNormalizado,
                CuotaDiaria = 0,
                CuotaMensual = cuotaMensual.Value,
                Habilitado = true,
                FechaCreacionUtc = nowUtc,
                FechaInicioPlanUtc = nowUtc
            };

            _dbContext.ApiClientes.Add(cliente);
        }
        else
        {
            string? planAnterior =
                ApiPlanes.Normalizar(cliente.Plan);

            bool cambiaPlan = !string.Equals(
                planAnterior,
                planNormalizado,
                StringComparison.Ordinal);

            cliente.Plan = planNormalizado;
            cliente.CuotaDiaria = 0;
            cliente.CuotaMensual = cuotaMensual.Value;
            cliente.Habilitado = true;

            if (cambiaPlan ||
                !cliente.FechaInicioPlanUtc.HasValue)
            {
                cliente.FechaInicioPlanUtc = nowUtc;
            }
        }

        bool tieneClaveActiva = cliente.Claves.Any(item =>
            item.Habilitada &&
            item.FechaRevocacionUtc is null);

        string? tokenEntrega = null;

        if (!tieneClaveActiva)
        {
            (_, tokenEntrega) =
                CrearClaveYEntrega(cliente, nowUtc);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new ProvisionamientoApiResultado(
            creado
                ? EstadoProvisionamientoApi.Creado
                : EstadoProvisionamientoApi.Actualizado,
            cliente.ApiClienteId,
            tokenEntrega,
            !tieneClaveActiva);
    }

    public async Task<string?> ConsumirEntregaClaveAsync(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        byte[] tokenHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(token.Trim()));

        ApiClaveEntrega? entrega =
            await _dbContext.ApiClaveEntregas
                .SingleOrDefaultAsync(
                    item => item.TokenHash.SequenceEqual(tokenHash),
                    cancellationToken);

        DateTime nowUtc = DateTime.UtcNow;

        if (entrega is null ||
            entrega.FechaConsumoUtc.HasValue ||
            entrega.FechaExpiracionUtc <= nowUtc)
        {
            return null;
        }

        string apiKey;

        try
        {
            apiKey = _protector.Unprotect(
                entrega.ClaveProtegida);
        }
        catch (CryptographicException)
        {
            return null;
        }

        entrega.FechaConsumoUtc = nowUtc;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return apiKey;
    }

    private (ApiClave Clave, string TokenEntrega)
        CrearClaveYEntrega(
            ApiCliente cliente,
            DateTime nowUtc)
    {
        string apiKey =
            "de_" + WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(32));

        byte[] keyHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(apiKey));

        ApiClave clave = new()
        {
            Cliente = cliente,
            Prefijo = apiKey[
                ..Math.Min(PrefixLength, apiKey.Length)],
            Hash = keyHash,
            Habilitada = true,
            FechaCreacionUtc = nowUtc
        };

        string tokenEntrega =
            WebEncoders.Base64UrlEncode(
                RandomNumberGenerator.GetBytes(32));

        byte[] tokenHash = SHA256.HashData(
            Encoding.UTF8.GetBytes(tokenEntrega));

        int horasEntrega = Math.Clamp(
            _options.KeyDeliveryHours,
            1,
            168);

        ApiClaveEntrega entrega = new()
        {
            Clave = clave,
            TokenHash = tokenHash,
            ClaveProtegida = _protector.Protect(apiKey),
            FechaCreacionUtc = nowUtc,
            FechaExpiracionUtc =
                nowUtc.AddHours(horasEntrega)
        };

        _dbContext.ApiClaves.Add(clave);
        _dbContext.ApiClaveEntregas.Add(entrega);

        return (clave, tokenEntrega);
    }

    private async Task<string> CrearNombreUnicoAsync(
        string nombre,
        string email,
        CancellationToken cancellationToken)
    {
        string baseName = string.IsNullOrWhiteSpace(nombre)
            ? email
            : nombre.Trim();

        if (baseName.Length > 100)
        {
            baseName = baseName[..100];
        }

        bool existe = await _dbContext.ApiClientes
            .AnyAsync(
                item => item.Nombre == baseName,
                cancellationToken);

        if (!existe)
        {
            return baseName;
        }

        string suffix =
            $"-{Guid.NewGuid():N}"[..9];

        int maxBaseLength = 100 - suffix.Length;

        if (baseName.Length > maxBaseLength)
        {
            baseName = baseName[..maxBaseLength];
        }

        return baseName + suffix;
    }

    private static string NormalizarEmail(string email)
    {
        return email.Trim().ToLowerInvariant();
    }
}
