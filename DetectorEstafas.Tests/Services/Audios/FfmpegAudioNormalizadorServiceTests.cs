using DetectorEstafas.Web.Services.Audios;

namespace DetectorEstafas.Tests.Services.Audios;

[TestClass]
public class FfmpegAudioNormalizadorServiceTests
{
    [TestMethod]
    public async Task NormalizarAWavAsync_EnSharkHosting_RechazaPorNoDisponible()
    {
        var servicio = new FfmpegAudioNormalizadorService();

        NotSupportedException exception =
            await Assert.ThrowsExactlyAsync<NotSupportedException>(
                () => servicio.NormalizarAWavAsync(
                    "origen.wav",
                    "destino.wav",
                    CancellationToken.None));

        StringAssert.Contains(
            exception.Message,
            "no está disponible");
    }
}
