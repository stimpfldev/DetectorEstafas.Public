using Microsoft.AspNetCore.Mvc;

namespace DetectorEstafas.Web.Controllers;

public sealed class ContactoController : Controller
{
    private readonly IConfiguration _configuration;

    public ContactoController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Index()
    {
        string email =
            _configuration["Contacto:Email"]?.Trim()
            ?? string.Empty;

        bool mostrarEmail =
            _configuration.GetValue<bool>(
                "Contacto:MostrarEmail")
            && !string.IsNullOrWhiteSpace(email);

        ViewData["Title"] = "Contacto";
        ViewData["EmailContacto"] = email;
        ViewData["MostrarEmailContacto"] = mostrarEmail;

        return View();
    }
}
