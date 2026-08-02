using DetectorEstafas.Web.Models.Capturas;
using Microsoft.AspNetCore.Http;

namespace DetectorEstafas.Web.Services.Capturas;

public interface ICapturaTemporalService
{
    Task<ResultadoCapturaTemporal> ProcesarAsync(
        IFormFile archivo,
        CancellationToken cancellationToken);
}
