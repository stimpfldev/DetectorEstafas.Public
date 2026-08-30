using DetectorEstafas.Web.Services.Audios;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DetectorEstafas.Tests.Services.Audios;

[TestClass]
public class AudioTemporalServiceTests
{
    [TestMethod]
    public async Task ProcesarAsync_Wav_RechazaEnHostingCompartido()
    {
        await AssertAudioNoDisponibleAsync(
            "audio.wav",
            "audio/wav");
    }

    [TestMethod]
    public async Task ProcesarAsync_Mp3_RechazaEnHostingCompartido()
    {
        await AssertAudioNoDisponibleAsync(
            "audio.mp3",
            "audio/mpeg");
    }

    [TestMethod]
    public async Task ProcesarAsync_Ogg_RechazaEnHostingCompartido()
    {
        await AssertAudioNoDisponibleAsync(
            "audio.ogg",
            "audio/ogg");
    }

    [TestMethod]
    public async Task ProcesarAsync_CualquierArchivo_RechazaSinProcesar()
    {
        await AssertAudioNoDisponibleAsync(
            "audio.txt",
            "text/plain");
    }

    private static async Task AssertAudioNoDisponibleAsync(
        string nombre,
        string contentType)
    {
        FormFile archivo = new(
            new MemoryStream([0x01, 0x02, 0x03]),
            0,
            3,
            "audio",
            nombre)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };

        AudioTemporalService service = new();

        AudioInvalidoException exception =
            await Assert.ThrowsExactlyAsync<AudioInvalidoException>(
                () => service.ProcesarAsync(
                    archivo,
                    CancellationToken.None));

        StringAssert.Contains(
            exception.Message,
            "no está disponible");
    }
}
