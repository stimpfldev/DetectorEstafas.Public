using Microsoft.AspNetCore.Mvc;

namespace DetectorEstafas.Web.Controllers;

public class LegalController : Controller
{
    [HttpGet]
    public IActionResult Privacidad()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Condiciones()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Terceros()
    {
        return View();
    }
}