using System.Text;
using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.Correo;
using DetectorEstafas.Web.ViewModels.Cuenta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Web.Controllers;

[Route("Cuenta")]
public sealed class CuentaController : Controller
{
    private readonly UserManager<UsuarioAplicacion> _userManager;
    private readonly ICorreoRegistroService _correoRegistroService;
    private readonly CorreoOptions _correoOptions;

    public CuentaController(
        UserManager<UsuarioAplicacion> userManager,
        ICorreoRegistroService correoRegistroService,
        IOptions<CorreoOptions> correoOptions)
    {
        _userManager = userManager;
        _correoRegistroService = correoRegistroService;
        _correoOptions = correoOptions.Value;
    }

    [AllowAnonymous]
    [HttpGet("Registrarse")]
    public IActionResult Registrarse()
    {
        return View(new RegistroViewModel());
    }

    [AllowAnonymous]
    [EnableRateLimiting("registro")]
    [ValidateAntiForgeryToken]
    [HttpPost("Registrarse")]
    public async Task<IActionResult> Registrarse(
        RegistroViewModel model,
        CancellationToken cancellationToken)
    {
        if (!model.AceptoCondiciones)
        {
            ModelState.AddModelError(
                nameof(model.AceptoCondiciones),
                "Debés aceptar las condiciones de uso.");
        }

        if (!model.AceptoPrivacidad)
        {
            ModelState.AddModelError(
                nameof(model.AceptoPrivacidad),
                "Debés aceptar la política de privacidad.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        string email = model.Email.Trim();
        DateTime fechaUtc = DateTime.UtcNow;

        UsuarioAplicacion usuario = new()
        {
            UserName = email,
            Email = email,
            EmailConfirmed = false,
            FechaRegistroUtc = fechaUtc,
            AceptoCondiciones = model.AceptoCondiciones,
            AceptoPrivacidad = model.AceptoPrivacidad,
            FechaAceptacionCondicionesUtc = fechaUtc
        };

        IdentityResult resultado =
            await _userManager.CreateAsync(
                usuario,
                model.Password);

        if (!resultado.Succeeded)
        {
            AgregarErrores(resultado);
            return View(model);
        }

        string token =
            await _userManager.GenerateEmailConfirmationTokenAsync(
                usuario);

        string tokenCodificado =
            WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(token));

        string? enlaceConfirmacion = Url.Action(
            nameof(ConfirmarCorreo),
            "Cuenta",
            new
            {
                usuarioId = usuario.Id,
                token = tokenCodificado
            },
            Request.Scheme);

        if (string.IsNullOrWhiteSpace(enlaceConfirmacion))
        {
            await _userManager.DeleteAsync(usuario);
            ModelState.AddModelError(
                string.Empty,
                "No fue posible generar el enlace de confirmación.");
            return View(model);
        }

        try
        {
            await _correoRegistroService.EnviarConfirmacionAsync(
                email,
                enlaceConfirmacion,
                cancellationToken);
        }
        catch
        {
            await _userManager.DeleteAsync(usuario);
            ModelState.AddModelError(
                string.Empty,
                "No fue posible enviar el correo de confirmación. La cuenta no fue creada.");
            return View(model);
        }

        TempData["RegistroExitoso"] =
            "La cuenta fue creada. Confirmá tu correo para completar el registro.";

        if (_correoOptions.ModoDesarrollo)
        {
            TempData["EnlaceConfirmacionDesarrollo"] =
                enlaceConfirmacion;
        }

        return RedirectToAction(nameof(Registrarse));
    }

    [AllowAnonymous]
    [HttpGet("ConfirmarCorreo")]
    public async Task<IActionResult> ConfirmarCorreo(
        string? usuarioId,
        string? token)
    {
        if (string.IsNullOrWhiteSpace(usuarioId) ||
            string.IsNullOrWhiteSpace(token))
        {
            return View("ConfirmacionCorreo", false);
        }

        UsuarioAplicacion? usuario =
            await _userManager.FindByIdAsync(usuarioId);

        if (usuario is null)
        {
            return View("ConfirmacionCorreo", false);
        }

        if (usuario.EmailConfirmed)
        {
            return View("ConfirmacionCorreo", true);
        }

        string tokenDecodificado;

        try
        {
            tokenDecodificado = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token));
        }
        catch (FormatException)
        {
            return View("ConfirmacionCorreo", false);
        }

        IdentityResult resultado =
            await _userManager.ConfirmEmailAsync(
                usuario,
                tokenDecodificado);

        return View(
            "ConfirmacionCorreo",
            resultado.Succeeded);
    }

    private void AgregarErrores(
        IdentityResult resultado)
    {
        foreach (IdentityError error in resultado.Errors)
        {
            string mensaje = error.Code switch
            {
                "DuplicateUserName" or "DuplicateEmail" =>
                    "Ya existe una cuenta registrada con ese correo electrónico.",
                "PasswordTooShort" =>
                    "La contraseña no alcanza la longitud mínima.",
                "PasswordRequiresDigit" =>
                    "La contraseña debe incluir al menos un número.",
                "PasswordRequiresLower" =>
                    "La contraseña debe incluir al menos una letra minúscula.",
                "PasswordRequiresUpper" =>
                    "La contraseña debe incluir al menos una letra mayúscula.",
                "PasswordRequiresNonAlphanumeric" =>
                    "La contraseña debe incluir al menos un símbolo.",
                _ => "No fue posible crear la cuenta con los datos ingresados."
            };

            ModelState.AddModelError(string.Empty, mensaje);
        }
    }
}
