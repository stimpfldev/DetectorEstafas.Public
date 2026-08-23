using System.Globalization;
using System.Security.Cryptography;
using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafas.Web.Controllers;

[Route("resultado-compartido")]
public sealed class ResultadosCompartidosController : Controller
{
    private readonly DetectorEstafasDbContext _dbContext;
    private readonly IDataProtector _protector;
    private readonly TimeSpan _vigencia;

    public ResultadosCompartidosController(
        DetectorEstafasDbContext dbContext,
        IDataProtectionProvider dataProtectionProvider,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _protector = dataProtectionProvider.CreateProtector(
            "DetectorEstafas.ResultadoCompartido.v1");

        int vigenciaMinutos = Math.Clamp(
            configuration.GetValue<int?>(
                "ResultadosCompartidos:VigenciaMinutos") ?? 10080,
            1,
            43200);

        _vigencia = TimeSpan.FromMinutes(vigenciaMinutos);
    }

    [HttpPost("crear")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("feedback")]
    public async Task<IActionResult> Crear(
        long analisisRegistroId,
        CancellationToken cancellationToken)
    {
        bool existe = await _dbContext.AnalisisRegistros
            .AsNoTracking()
            .AnyAsync(
                registro =>
                    registro.AnalisisRegistroId == analisisRegistroId,
                cancellationToken);

        if (!existe)
        {
            return NotFound();
        }

        long creadoUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        string payload = string.Create(
            CultureInfo.InvariantCulture,
            $"{analisisRegistroId}|{creadoUnix}");

        string token = _protector.Protect(payload);

        string? url = Url.Action(
            nameof(Ver),
            "ResultadosCompartidos",
            new { token },
            Request.Scheme);

        if (string.IsNullOrWhiteSpace(url))
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }

        return Json(new
        {
            ok = true,
            url
        });
    }

    [HttpGet("{token}")]
    [ResponseCache(
        Duration = 0,
        Location = ResponseCacheLocation.None,
        NoStore = true)]
    public async Task<IActionResult> Ver(
        string token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return View("Invalido");
        }

        string payload;

        try
        {
            payload = _protector.Unprotect(token);
        }
        catch (CryptographicException)
        {
            return View("Invalido");
        }

        string[] partes = payload.Split('|');

        if (partes.Length != 2 ||
            !long.TryParse(
                partes[0],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out long analisisRegistroId) ||
            !long.TryParse(
                partes[1],
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out long creadoUnix))
        {
            return View("Invalido");
        }

        DateTimeOffset creadoUtc;

        try
        {
            creadoUtc = DateTimeOffset.FromUnixTimeSeconds(creadoUnix);
        }
        catch (ArgumentOutOfRangeException)
        {
            return View("Invalido");
        }

        DateTimeOffset ahoraUtc = DateTimeOffset.UtcNow;

        if (creadoUtc > ahoraUtc.AddMinutes(5) ||
            ahoraUtc - creadoUtc > _vigencia)
        {
            return View("Invalido");
        }

        AnalisisRegistro? registro = await _dbContext.AnalisisRegistros
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.AnalisisRegistroId == analisisRegistroId,
                cancellationToken);

        if (registro is null)
        {
            return View("Invalido");
        }

        return View(registro);
    }
}
