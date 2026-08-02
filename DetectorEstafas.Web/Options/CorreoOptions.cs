namespace DetectorEstafas.Web.Options;

public sealed class CorreoOptions
{
    public const string SectionName = "Correo";

    public bool ModoDesarrollo { get; set; } = true;
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UsarSsl { get; set; } = true;
    public string Usuario { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RemitenteEmail { get; set; } = string.Empty;
    public string RemitenteNombre { get; set; } = "Detector de Estafas";
}
