using DetectorEstafas.Web.Services.Comercial;
using DetectorEstafas.Web.ViewModels.Planes;
using Microsoft.AspNetCore.Mvc;

namespace DetectorEstafas.Web.Controllers;

[Route("acceso-api")]
[ResponseCache(
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public sealed class AccesoApiController : Controller
{
    private readonly IProvisionamientoApiComercialService _provisionamiento;

    public AccesoApiController(
        IProvisionamientoApiComercialService provisionamiento)
    {
        _provisionamiento = provisionamiento;
    }

    [HttpGet("clave")]
    public IActionResult Clave(string? token)
    {
        ViewData["DisableGoogleAnalytics"] = true;

        if (string.IsNullOrWhiteSpace(token))
        {
            return View(
                new AccesoApiClaveViewModel
                {
                    InvalidaOExpirada = true
                });
        }

        return View(
            new AccesoApiClaveViewModel
            {
                Token = token
            });
    }

    [HttpPost("clave")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clave(
        AccesoApiClaveViewModel model,
        CancellationToken cancellationToken)
    {
        ViewData["DisableGoogleAnalytics"] = true;

        string? apiKey =
            await _provisionamiento.ConsumirEntregaClaveAsync(
                model.Token,
                cancellationToken);

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return View(
                new AccesoApiClaveViewModel
                {
                    InvalidaOExpirada = true
                });
        }

        return View(
            new AccesoApiClaveViewModel
            {
                ApiKey = apiKey
            });
    }
}
