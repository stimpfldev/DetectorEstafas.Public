using System.ComponentModel.DataAnnotations;
using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.Capturas;
using DetectorEstafas.Web.Models.Audios;
using DetectorEstafas.Web.Models.InteligenciaArtificial;
using DetectorEstafas.Web.Models.Telefonos;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DetectorEstafas.Web.ViewModels;

public class AnalisisViewModel
{
    [Required(
        ErrorMessage = "Ingresá el contenido que querés analizar.")]
    [StringLength(
        5000,
        MinimumLength = 3,
        ErrorMessage = "El contenido debe tener entre 3 y 5000 caracteres.")]
    [Display(Name = "Contenido recibido")]
    public string Contenido { get; set; } = string.Empty;

    [Display(Name = "Tipo de contenido")]
    public TipoContenido Tipo { get; set; } =
        TipoContenido.Mensaje;

    public string Origen { get; set; } = "Web";

    [Display(Name = "Agregar evaluación complementaria con IA")]
    public bool SolicitarEvaluacionIa { get; set; }

    [BindNever]
    public ResultadoAnalisis? Resultado { get; set; }

    [BindNever]
    public long? AnalisisRegistroId { get; set; }

    [BindNever]
    public ResultadoCapturaTemporal? CapturaValidada { get; set; }

    [BindNever]
    public string? CapturaError { get; set; }

    [BindNever]
    public ResultadoAudioTemporal? AudioValidado { get; set; }

    [BindNever]
    public string? AudioError { get; set; }

    [BindNever]
    public ResultadoEvaluacionIa? EvaluacionIa { get; set; }

    [BindNever]
    public ResultadoIdentificacionTelefono? IdentificacionTelefono { get; set; }
}