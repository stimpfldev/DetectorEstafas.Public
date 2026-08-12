using DetectorEstafas.Web.Models.Telefonos;

namespace DetectorEstafas.Web.Services.Telefonos;

public interface IIdentificacionTelefonoService
{
    ResultadoIdentificacionTelefono Identificar(string numeroIngresado);
}
