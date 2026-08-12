using System.Security.Cryptography;
using System.Text;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.Api.Administracion;
using DetectorEstafas.Web.ViewModels.ApiAdministracion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Controllers;

[Route("administracion/api")]
[ResponseCache(
    Location = ResponseCacheLocation.None,
    NoStore = true)]
public sealed class AdministracionApiController :
    Controller
{
    private const string SessionKey =
        "ApiAdministracionAutorizada";

    private readonly IApiAdministracionService _service;
    private readonly ApiAdministracionOptions _options;

    public AdministracionApiController(
        IApiAdministracionService service,
        IOptions<ApiAdministracionOptions> options)
    {
        _service = service;
        _options = options.Value;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        if (!ConfiguracionValida())
        {
            return NotFound();
        }

        if (!EstaAutorizado())
        {
            return RedirectToAction(
                nameof(Ingresar));
        }

        ApiDashboardViewModel model =
            await _service.ObtenerDashboardAsync(
                cancellationToken);

        return View(model);
    }

    [HttpGet("ingresar")]
    public IActionResult Ingresar()
    {
        if (!ConfiguracionValida())
        {
            return NotFound();
        }

        if (EstaAutorizado())
        {
            return RedirectToAction(
                nameof(Index));
        }

        return View(
            new ApiAdministracionLoginViewModel());
    }

    [HttpPost("ingresar")]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("admin-login")]
    public IActionResult Ingresar(
        ApiAdministracionLoginViewModel model)
    {
        if (!ConfiguracionValida())
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (!SecretosIguales(
                model.Secret,
                _options.Secret))
        {
            ModelState.AddModelError(
                nameof(model.Secret),
                "La clave administrativa no es válida.");

            return View(model);
        }

        HttpContext.Session.SetString(
            SessionKey,
            "1");

        return RedirectToAction(
            nameof(Index));
    }

    [HttpPost("salir")]
    [ValidateAntiForgeryToken]
    public IActionResult Salir()
    {
        HttpContext.Session.Remove(SessionKey);

        return RedirectToAction(
            nameof(Ingresar));
    }

    [HttpPost(
        "clientes/{apiClienteId:int}/cambiar-estado")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
        CambiarEstadoCliente(
            int apiClienteId,
            CancellationToken cancellationToken)
    {
        if (!EstaAutorizado())
        {
            return Unauthorized();
        }

        bool changed =
            await _service.CambiarEstadoClienteAsync(
                apiClienteId,
                cancellationToken);

        TempData[
            changed
                ? "Mensaje"
                : "Error"] =
            changed
                ? "El estado del cliente fue actualizado."
                : "No se encontró el cliente.";

        return RedirectToAction(
            nameof(Index));
    }

    [HttpPost("clientes/{apiClienteId:int}/plan")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
        ActualizarPlanCliente(
            int apiClienteId,
            string plan,
            int? cuotaMensualPersonalizada,
            CancellationToken cancellationToken)
    {
        if (!EstaAutorizado())
        {
            return Unauthorized();
        }

        bool updated =
            await _service.ActualizarPlanClienteAsync(
                apiClienteId,
                plan,
                cuotaMensualPersonalizada,
                cancellationToken);

        TempData[
            updated
                ? "Mensaje"
                : "Error"] =
            updated
                ? "El plan y la cuota del cliente fueron actualizados."
                : "No fue posible actualizar el plan o la cuota.";

        return RedirectToAction(
            nameof(Index));
    }

    [HttpPost("claves/{apiClaveId:int}/revocar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RevocarClave(
        int apiClaveId,
        CancellationToken cancellationToken)
    {
        if (!EstaAutorizado())
        {
            return Unauthorized();
        }

        bool revoked =
            await _service.RevocarClaveAsync(
                apiClaveId,
                cancellationToken);

        TempData[
            revoked
                ? "Mensaje"
                : "Error"] =
            revoked
                ? "La clave fue revocada."
                : "La clave no existe o ya estaba revocada.";

        return RedirectToAction(
            nameof(Index));
    }

    private bool EstaAutorizado()
    {
        return HttpContext.Session
            .GetString(SessionKey) == "1";
    }

    private bool ConfiguracionValida()
    {
        return _options.Enabled &&
               !string.IsNullOrWhiteSpace(
                   _options.Secret);
    }

    private static bool SecretosIguales(
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
}
