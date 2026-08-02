namespace DetectorEstafas.Web.Models.Telefonos;

public sealed class ResultadoIdentificacionTelefono
{
    public bool Encontrado { get; init; }
    public string NumeroNormalizado { get; init; } = string.Empty;
    public string Clasificacion { get; init; } = "Número sin información pública verificada";
    public string? Entidad { get; init; }
    public string? Descripcion { get; init; }
    public string NivelConfianza { get; init; } = "Sin información";
    public string? FuenteNombre { get; init; }
    public string? FuenteUrl { get; init; }
    public DateTime? FechaConsultaUtc { get; init; }
}
