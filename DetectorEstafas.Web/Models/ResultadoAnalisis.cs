namespace DetectorEstafas.Web.Models;

public class ResultadoAnalisis
{
    public int Puntaje { get; set; }

    public NivelRiesgo Nivel { get; set; }

    public string Resumen { get; set; } = string.Empty;

    public List<string> SenalesDetectadas { get; set; } = new();

    public List<string> Recomendaciones { get; set; } = new();

    public List<EnlaceAnalizado> EnlacesAnalizados { get; set; } = new();
}