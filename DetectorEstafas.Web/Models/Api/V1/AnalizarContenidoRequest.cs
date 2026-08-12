using System.ComponentModel.DataAnnotations;
using DetectorEstafas.Web.Models;

namespace DetectorEstafas.Web.Models.Api.V1;

public sealed class AnalizarContenidoRequest
{
    [Required]
    public TipoContenido? TipoContenido { get; set; }

    [Required]
    public string Contenido { get; set; } = string.Empty;
}
