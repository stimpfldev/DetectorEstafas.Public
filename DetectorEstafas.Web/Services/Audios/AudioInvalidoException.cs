namespace DetectorEstafas.Web.Services.Audios;

public class AudioInvalidoException : Exception
{
    public AudioInvalidoException(string message)
        : base(message)
    {
    }
}
