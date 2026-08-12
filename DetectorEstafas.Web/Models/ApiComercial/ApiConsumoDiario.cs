namespace DetectorEstafas.Web.Models.ApiComercial;

public sealed class ApiConsumoDiario
{
    public long ApiConsumoDiarioId { get; set; }

    public int ApiClienteId { get; set; }

    public DateOnly FechaUtc { get; set; }

    public int CantidadSolicitudes { get; set; }

    public DateTime UltimaSolicitudUtc { get; set; }

    public ApiCliente Cliente { get; set; } = null!;
}
