namespace FFMpegCore;

// Compatibilidad de configuración para la variante SharkASP.
// No invoca FFmpeg ni requiere FFMpegCore.dll.
public static class GlobalFFOptions
{
    public static void Configure(
        Action<GlobalFFOptionsConfiguration> configure)
    {
        GlobalFFOptionsConfiguration options = new();
        configure(options);
    }
}

public sealed class GlobalFFOptionsConfiguration
{
    public string BinaryFolder { get; set; } = ".";
}
