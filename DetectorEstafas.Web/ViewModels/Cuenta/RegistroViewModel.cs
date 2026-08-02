using System.ComponentModel.DataAnnotations;

namespace DetectorEstafas.Web.ViewModels.Cuenta;

public sealed class RegistroViewModel
{
    [Required(ErrorMessage = "Ingresá tu correo electrónico.")]
    [EmailAddress(ErrorMessage = "Ingresá un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá una contraseña.")]
    [StringLength(
        100,
        MinimumLength = 10,
        ErrorMessage = "La contraseña debe tener al menos 10 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmá la contraseña.")]
    [DataType(DataType.Password)]
    [Compare(
        nameof(Password),
        ErrorMessage = "Las contraseñas no coinciden.")]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmPassword { get; set; } = string.Empty;

    public bool AceptoCondiciones { get; set; }

    public bool AceptoPrivacidad { get; set; }
}
