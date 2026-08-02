using System.ComponentModel.DataAnnotations;

namespace DetectorEstafas.Web.Models;

public enum CategoriaReporteComunitario
{
    [Display(Name = "Solicitud de dinero o transferencia")]
    SolicitudDinero = 1,

    [Display(Name = "Solicitud de código, clave o datos")]
    SolicitudCredenciales = 2,

    [Display(Name = "Suplantación de banco, empresa u organismo")]
    SuplantacionIdentidad = 3,

    [Display(Name = "Premio, inversión o beneficio falso")]
    PromesaEnganosa = 4,

    [Display(Name = "Amenaza o falsa emergencia")]
    AmenazaEmergencia = 5,

    [Display(Name = "Otro comportamiento sospechoso")]
    Otro = 6
}