using DetectorEstafas.Web.Models;

namespace DetectorEstafas.Web.Services;

public interface IRdapService
{
    Task<ResultadoRdap> ConsultarDominioAsync(
        string dominio,
        CancellationToken cancellationToken);
}