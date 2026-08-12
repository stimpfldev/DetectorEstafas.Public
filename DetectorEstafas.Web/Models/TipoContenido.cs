using System.ComponentModel.DataAnnotations;

namespace DetectorEstafas.Web.Models;

public enum TipoContenido
{
    [Display(Name = "Mensaje")]
    Mensaje = 1,

    [Display(Name = "Enlace")]
    Enlace = 2,

    [Display(Name = "Número telefónico")]
    Telefono = 3,

    [Display(Name = "Descripción de llamada")]
    Llamada = 4
}