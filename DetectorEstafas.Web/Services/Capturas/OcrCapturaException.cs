namespace DetectorEstafas.Web.Services.Capturas;

public sealed class OcrCapturaException : Exception
{
    public OcrCapturaException(string message)
        : base(message)
    {
    }

    public OcrCapturaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
