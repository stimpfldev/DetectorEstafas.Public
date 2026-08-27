namespace DetectorEstafas.Web.Models.ApiComercial;

public sealed class ApiClaveEntrega
{
    public int ApiClaveEntregaId { get; set; }

    public int ApiClaveId { get; set; }

    public byte[] TokenHash { get; set; } = Array.Empty<byte>();

    public string ClaveProtegida { get; set; } = string.Empty;

    public DateTime FechaCreacionUtc { get; set; } = DateTime.UtcNow;

    public DateTime FechaExpiracionUtc { get; set; }

    public DateTime? FechaConsumoUtc { get; set; }

    public ApiClave Clave { get; set; } = null!;
}
