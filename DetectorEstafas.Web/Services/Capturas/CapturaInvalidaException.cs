namespace DetectorEstafas.Web.Services.Capturas;

public sealed class CapturaInvalidaException : Exception
{
    public CapturaInvalidaException(string message)
        : base(message)
    {
    }
}
