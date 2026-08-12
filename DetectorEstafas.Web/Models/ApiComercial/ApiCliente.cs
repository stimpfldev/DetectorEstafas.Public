namespace DetectorEstafas.Web.Models.ApiComercial;

public sealed class ApiCliente
{
    public int ApiClienteId { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Plan { get; set; } = "Prueba";

    public int CuotaDiaria { get; set; } = 100;

    public bool Habilitado { get; set; } = true;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public ICollection<ApiClave> Claves { get; set; } = new List<ApiClave>();

    public ICollection<ApiConsumoDiario> Consumos { get; set; } = new List<ApiConsumoDiario>();
}
