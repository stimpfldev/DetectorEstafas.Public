using DetectorEstafas.Web.Models.Audios;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.Audios;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;

namespace DetectorEstafas.Tests.Services.Audios;

[TestClass]
public class AudioTemporalServiceTests
{
    [TestMethod]
    public async Task ProcesarAsync_WavValido_AceptaYEliminaTemporal()
    {
        byte[] contenido;

        using (var memoria = new MemoryStream())
        {
            using (var writer = new WaveFileWriter(
                       memoria,
                       new WaveFormat(16000, 16, 1)))
            {
                byte[] silencio = new byte[16000 * 2];
                writer.Write(silencio, 0, silencio.Length);
            }

            contenido = memoria.ToArray();
        }
        FormFile archivo = CrearArchivo(
            contenido,
            "audio.wav",
            "audio/wav");

        string carpeta =
            $"DetectorEstafasTests/Audios/{Guid.NewGuid():N}";

        AudioTemporalService service = CrearServicio(carpeta);

        var resultado = await service.ProcesarAsync(
            archivo,
            CancellationToken.None);

        Assert.AreEqual("audio/wav", resultado.TipoContenido);
        Assert.AreEqual(contenido.Length, resultado.TamanoBytes);

        string ruta = Path.Combine(
            Path.GetTempPath(),
            carpeta.Replace('/', Path.DirectorySeparatorChar));

        Assert.AreEqual(0, Directory.EnumerateFiles(ruta).Count());

        Directory.Delete(ruta, true);
    }

    [TestMethod]
    public async Task ProcesarAsync_TextoRenombradoComoMp3_Rechaza()
    {
        FormFile archivo = CrearArchivo(
            "esto no es audio"u8.ToArray(),
            "audio.mp3",
            "audio/mpeg");

        AudioTemporalService service = CrearServicio(
            $"DetectorEstafasTests/Audios/{Guid.NewGuid():N}");

        await Assert.ThrowsExactlyAsync<AudioInvalidoException>(
            () => service.ProcesarAsync(
                archivo,
                CancellationToken.None));
    }

    [TestMethod]
    public async Task ProcesarAsync_ExtensionNoPermitida_Rechaza()
    {
        FormFile archivo = CrearArchivo(
     [0x01, 0x02, 0x03],
     "audio.txt",
     "text/plain");

        AudioTemporalService service = CrearServicio(
            $"DetectorEstafasTests/Audios/{Guid.NewGuid():N}");

        await Assert.ThrowsExactlyAsync<AudioInvalidoException>(
            () => service.ProcesarAsync(
                archivo,
                CancellationToken.None));
    }

    private static AudioTemporalService CrearServicio(string carpeta)
    {
        AudioOptions options = new()
        {
            MaxFileSizeBytes = 10 * 1024 * 1024,
            RetentionMinutes = 60,
            TemporaryFolderName = carpeta
        };

        return new AudioTemporalService(
       Options.Create(options),
       new WebHostEnvironmentFalso(),
       new TranscriptorFalso(),
       new NormalizadorFalso());
    }

    private static FormFile CrearArchivo(
        byte[] contenido,
        string nombre,
        string contentType)
    {
        return new FormFile(
            new MemoryStream(contenido),
            0,
            contenido.Length,
            "audio",
            nombre)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
    private sealed class NormalizadorFalso
    : IAudioNormalizadorService
    {
        public Task NormalizarAWavAsync(
            string rutaOrigen,
            string rutaDestino,
            CancellationToken cancellationToken)
        {
            File.Copy(
                rutaOrigen,
                rutaDestino,
                overwrite: true);

            return Task.CompletedTask;
        }
    }
    private sealed class TranscriptorFalso : ITranscriptorAudioService
    {
        public Task<ResultadoTranscripcionAudio> TranscribirAsync(
            string rutaAudio,
            string extension,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new ResultadoTranscripcionAudio
            {
                Texto = "Transferí el dinero ahora."
            });
        }
    }

    private sealed class WebHostEnvironmentFalso : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Development";
        public string ContentRootPath { get; set; } = string.Empty;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
