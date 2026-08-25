using System.ComponentModel.DataAnnotations;

namespace DetectorEstafas.Web.ViewModels.Planes;

public sealed class SolicitudSuscripcionViewModel
{
    [Required]
    public string Plan { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu nombre.")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá tu email.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(254)]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    public bool AceptaPrivacidad { get; set; }

    public bool AceptaCondiciones { get; set; }
}
