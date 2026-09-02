using Microsoft.AspNetCore.Mvc;

namespace DetectorEstafas.Web.Controllers;

public sealed class IdiomaController : Controller
{
    public const string LanguageCookieName =
        "AlertaEstafa.Language";

    [HttpGet]
    public IActionResult Cambiar(
        string? culture,
        string? returnUrl = null)
    {
        string language = string.Equals(
            culture,
            "en",
            StringComparison.OrdinalIgnoreCase)
                ? "en"
                : "es";

        Response.Cookies.Append(
            LanguageCookieName,
            language,
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Secure = Request.IsHttps,
                Path = "/"
            });

        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }

        return RedirectToAction("Index", "Analisis");
    }
}
