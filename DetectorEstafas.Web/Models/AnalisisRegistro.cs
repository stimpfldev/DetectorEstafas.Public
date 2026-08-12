namespace DetectorEstafas.Web.Models;

public class AnalisisRegistro
{
    public long AnalisisRegistroId { get; set; }

    public DateTime FechaUtc { get; set; }

    public TipoContenido TipoContenido { get; set; }

    public NivelRiesgo NivelRiesgo { get; set; }

    public byte Puntaje { get; set; }

    public short CantidadSenales { get; set; }

    public string Origen { get; set; } = "Web";
}