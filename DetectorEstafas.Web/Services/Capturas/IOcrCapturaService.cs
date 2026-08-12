using DetectorEstafas.Web.Models.Capturas;

namespace DetectorEstafas.Web.Services.Capturas;

public interface IOcrCapturaService
{
    Task<ResultadoOcr> ExtraerTextoAsync(
        string rutaImagen,
        CancellationToken cancellationToken);
}
