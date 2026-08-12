namespace DetectorEstafas.Web.Services.Audios;

public class TranscripcionAudioException : Exception
{
    public TranscripcionAudioException(string message)
        : base(message)
    {
    }

    public TranscripcionAudioException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
