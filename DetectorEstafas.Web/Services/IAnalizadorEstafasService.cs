using DetectorEstafas.Web.Models;

namespace DetectorEstafas.Web.Services;

public interface IAnalizadorEstafasService
{
    ResultadoAnalisis Analizar(
        string contenido,
        TipoContenido tipoContenido);
}