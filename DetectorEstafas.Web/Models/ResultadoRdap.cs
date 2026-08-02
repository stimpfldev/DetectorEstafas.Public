namespace DetectorEstafas.Web.Models;

public class ResultadoRdap
{
    public bool FueConsultado { get; set; }

    public bool Encontrado { get; set; }

    public DateTime? FechaRegistroUtc { get; set; }

    public int? AntiguedadDias { get; set; }

    public int PuntajeAdicional { get; set; }

    public string Estado { get; set; } = string.Empty;

    public List<string> Senales { get; set; } = new();
}