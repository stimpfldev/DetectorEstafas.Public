namespace DetectorEstafas.Web.Models.ApiComercial;

public sealed class ApiClave
{
    public int ApiClaveId { get; set; }

    public int ApiClienteId { get; set; }

    public string Prefijo { get; set; } = string.Empty;

    public byte[] Hash { get; set; } = Array.Empty<byte>();

    public bool Habilitada { get; set; } = true;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public DateTime? FechaRevocacionUtc { get; set; }

    public ApiCliente Cliente { get; set; } = null!;

    public ICollection<ApiClaveEntrega> Entregas { get; set; } =
        new List<ApiClaveEntrega>();
}
