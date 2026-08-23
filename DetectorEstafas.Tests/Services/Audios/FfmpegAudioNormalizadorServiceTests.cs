using DetectorEstafas.Web.Services.Audios;
using FFMpegCore;
using NAudio.Wave;

namespace DetectorEstafas.Tests.Services.Audios;

[TestClass]
public class FfmpegAudioNormalizadorServiceTests
{
    [TestMethod]
    public async Task NormalizarAWavAsync_GeneraWav16KhzMono()
    {
        string? ffmpegBinaryFolder =
    Environment.GetEnvironmentVariable(
        "FFMPEG_BINARY_FOLDER");

        if (string.IsNullOrWhiteSpace(ffmpegBinaryFolder))
        {
            Assert.Inconclusive(
                "No se configuró FFMPEG_BINARY_FOLDER.");
        }

        GlobalFFOptions.Configure(options =>
        {
            options.BinaryFolder = ffmpegBinaryFolder;
        });

        string origen =
            Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.wav");

        string destino =
            Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.wav");

        try
        {
            using (var writer = new WaveFileWriter(
                       origen,
                       new WaveFormat(44100, 1)))
            {
                byte[] silencio = new byte[44100 * 2];
                writer.Write(silencio, 0, silencio.Length);
            }

            var servicio =
                new FfmpegAudioNormalizadorService();

            await servicio.NormalizarAWavAsync(
                origen,
                destino,
                CancellationToken.None);

            Assert.IsTrue(File.Exists(destino));

            using var reader = new WaveFileReader(destino);

            Assert.AreEqual(16000, reader.WaveFormat.SampleRate);
            Assert.AreEqual(1, reader.WaveFormat.Channels);
            Assert.AreEqual(16, reader.WaveFormat.BitsPerSample);
        }
        finally
        {
            if (File.Exists(origen))
                File.Delete(origen);

            if (File.Exists(destino))
                File.Delete(destino);
        }
    }
}