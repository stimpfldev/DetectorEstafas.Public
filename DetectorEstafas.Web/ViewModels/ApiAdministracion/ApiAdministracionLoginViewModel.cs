using System.ComponentModel.DataAnnotations;

namespace DetectorEstafas.Web.ViewModels.ApiAdministracion;

public sealed class ApiAdministracionLoginViewModel
{
    [Required(ErrorMessage = "Ingresá la clave administrativa.")]
    [DataType(DataType.Password)]
    public string Secret { get; set; } = string.Empty;
}
