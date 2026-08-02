using Microsoft.AspNetCore.Identity;

namespace DetectorEstafas.Web.Models;

public sealed class UsuarioAplicacion : IdentityUser
{
    public DateTime FechaRegistroUtc { get; set; } = DateTime.UtcNow;

    public DateTime? FechaAceptacionCondicionesUtc { get; set; }

    public bool AceptoCondiciones { get; set; }

    public bool AceptoPrivacidad { get; set; }
}
