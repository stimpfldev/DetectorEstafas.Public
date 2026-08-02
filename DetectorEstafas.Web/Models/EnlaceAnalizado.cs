namespace DetectorEstafas.Web.Models;

public class EnlaceAnalizado
{
    public string ValorIngresado { get; set; } = string.Empty;

    public string Dominio { get; set; } = string.Empty;

    public bool EsValido { get; set; }

    public bool UsaHttps { get; set; }

    public int Puntaje { get; set; }

    public NivelRiesgo Nivel { get; set; }

    public List<string> Senales { get; set; } = new();

    public ResultadoRdap? Rdap { get; set; }
}