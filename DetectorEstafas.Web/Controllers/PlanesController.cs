using DetectorEstafas.Web.Data;
using DetectorEstafas.Web.Models.ApiComercial;
using DetectorEstafas.Web.Services.Comercial;
using DetectorEstafas.Web.ViewModels.Planes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DetectorEstafas.Web.Controllers;

[Route("planes")]
public sealed class PlanesController : Controller
{
    private readonly IComercializacionApiService _comercializacion;
    private readonly DetectorEstafasDbContext _dbContext;

    public PlanesController(
        IComercializacionApiService comercializacion,
        DetectorEstafasDbContext dbContext)
    {
        _comercializacion = comercializacion;
        _dbContext = dbContext;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet("prueba")]
    public IActionResult Prueba()
    {
        ViewData["DisableGoogleAnalytics"] = true;
        return View(new SolicitudPruebaViewModel());
    }

    [HttpPost("prueba")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("comercial-alta")]
    public async Task<IActionResult> Prueba(
        SolicitudPruebaViewModel model,
        CancellationToken cancellationToken)
    {
        ViewData["DisableGoogleAnalytics"] = true;
        ValidarConsentimientos(
            model.AceptaPrivacidad,
            model.AceptaCondiciones);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        ActivacionPruebaComercialResultado resultado =
            await _comercializacion.ActivarPruebaAsync(
                model.Nombre,
                model.Email,
                ObtenerBaseUrl(),
                cancellationToken);

        if (!resultado.Exito ||
            string.IsNullOrWhiteSpace(resultado.TokenEntrega))
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.Mensaje ??
                "No fue posible activar la prueba.");

            return View(model);
        }

        return RedirectToAction(
            "Clave",
            "AccesoApi",
            new { token = resultado.TokenEntrega });
    }

    [HttpGet("suscripcion")]
    public IActionResult Suscripcion(string? plan)
    {
        ViewData["DisableGoogleAnalytics"] = true;

        string? planNormalizado =
            NormalizarPlanPago(plan);

        if (planNormalizado is null)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(
            new SolicitudSuscripcionViewModel
            {
                Plan = planNormalizado
            });
    }

    [HttpPost("suscripcion")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("comercial-alta")]
    public async Task<IActionResult> Suscripcion(
        SolicitudSuscripcionViewModel model,
        CancellationToken cancellationToken)
    {
        ViewData["DisableGoogleAnalytics"] = true;
        ValidarConsentimientos(
            model.AceptaPrivacidad,
            model.AceptaCondiciones);

        string? planNormalizado =
            NormalizarPlanPago(model.Plan);

        if (planNormalizado is null)
        {
            ModelState.AddModelError(
                nameof(model.Plan),
                "El plan seleccionado no es válido.");
        }
        else
        {
            model.Plan = planNormalizado;
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        InicioSuscripcionComercialResultado resultado =
            await _comercializacion.IniciarSuscripcionAsync(
                model.Nombre,
                model.Email,
                model.Plan,
                ObtenerBaseUrl(),
                cancellationToken);

        if (!resultado.Exito ||
            !EsUrlMercadoPagoSegura(resultado.UrlPago))
        {
            ModelState.AddModelError(
                string.Empty,
                resultado.Mensaje ??
                "No fue posible iniciar el pago.");

            return View(model);
        }

        return Redirect(resultado.UrlPago!);
    }

    [HttpGet("retorno")]
    public async Task<IActionResult> Retorno(
        string? referencia,
        CancellationToken cancellationToken)
    {
        ViewData["DisableGoogleAnalytics"] = true;

        if (!Guid.TryParseExact(
                referencia,
                "N",
                out Guid referenciaPublica))
        {
            return View(
                new RetornoSuscripcionViewModel
                {
                    Estado = "Pendiente"
                });
        }

        SuscripcionComercial? suscripcion =
            await _dbContext.SuscripcionesComerciales
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    item =>
                        item.ReferenciaPublica ==
                            referenciaPublica,
                    cancellationToken);

        if (suscripcion is null)
        {
            return View(
                new RetornoSuscripcionViewModel
                {
                    Estado = "Pendiente"
                });
        }

        return View(
            new RetornoSuscripcionViewModel
            {
                Plan = suscripcion.Plan,
                Estado = suscripcion.Estado,
                AccesoActivo = suscripcion.Estado ==
                    EstadosSuscripcionComercial.Activa
            });
    }

    private void ValidarConsentimientos(
        bool aceptaPrivacidad,
        bool aceptaCondiciones)
    {
        if (!aceptaPrivacidad)
        {
            ModelState.AddModelError(
                "AceptaPrivacidad",
                "Debés aceptar la política de privacidad.");
        }

        if (!aceptaCondiciones)
        {
            ModelState.AddModelError(
                "AceptaCondiciones",
                "Debés aceptar las condiciones de uso.");
        }
    }

    private string ObtenerBaseUrl()
    {
        return $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
    }

    private static string? NormalizarPlanPago(string? plan)
    {
        string? normalizado = ApiPlanes.Normalizar(plan);

        return normalizado == ApiPlanes.Starter ||
               normalizado == ApiPlanes.Growth
            ? normalizado
            : null;
    }

    private static bool EsUrlMercadoPagoSegura(
        string? url)
    {
        if (!Uri.TryCreate(
                url,
                UriKind.Absolute,
                out Uri? uri) ||
            !string.Equals(
                uri.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        string host = uri.Host;

        return host.Equals(
                   "mercadopago.com",
                   StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith(
                   ".mercadopago.com",
                   StringComparison.OrdinalIgnoreCase) ||
               host.Equals(
                   "mercadopago.com.ar",
                   StringComparison.OrdinalIgnoreCase) ||
               host.EndsWith(
                   ".mercadopago.com.ar",
                   StringComparison.OrdinalIgnoreCase);
    }
}
