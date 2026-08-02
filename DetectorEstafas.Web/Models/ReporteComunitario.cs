namespace DetectorEstafas.Web.Models;

public class ReporteComunitario
{
    public long AnalisisRegistroId { get; set; }

    public CategoriaReporteComunitario Categoria { get; set; }

    public DateTime FechaUtc { get; set; }
}