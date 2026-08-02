using System.Text;
using DetectorEstafas.Web.Models.Audios;
using DetectorEstafas.Web.Options;
using Microsoft.Extensions.Options;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Whisper.net;

namespace DetectorEstafas.Web.Services.Audios;

public class WhisperTranscriptorAudioService
    : ITranscriptorAudioService
{
    private readonly TranscripcionOptions _options;
    private readonly string _rutaModelo;
    private readonly SemaphoreSlim _semaforo = new(1, 1);

    public WhisperTranscriptorAudioService(
        IOptions<TranscripcionOptions> options,
        IWebHostEnvironment environment)
    {
        _options = options.Value;
        _rutaModelo = Path.Combine(
            environment.ContentRootPath,
            _options.ModelFolderName,
            _options.ModelFileName);
    }

    public async Task<ResultadoTranscripcionAudio> TranscribirAsync(
        string rutaAudio,
        string extension,
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_rutaModelo))
        {
            throw new TranscripcionAudioException(
                "No se encontró el modelo local de transcripción. Ejecutá Scripts\\Preparar-Whisper.ps1.");
        }

        await _semaforo.WaitAsync(cancellationToken);

        try
        {
            using MemoryStream wavStream =
                ConvertirAWav16Khz(rutaAudio, extension);

            using WhisperFactory factory =
                WhisperFactory.FromPath(_rutaModelo);

            using var processor = factory
                .CreateBuilder()
                .WithLanguage(_options.Language)
                .Build();

            StringBuilder texto = new();

            await foreach (var segment in
                processor.ProcessAsync(wavStream)
                    .WithCancellation(cancellationToken))
            {
                string fragmento = segment.Text.Trim();

                if (fragmento.Length == 0)
                {
                    continue;
                }

                if (texto.Length > 0)
                {
                    texto.Append(' ');
                }

                texto.Append(fragmento);
            }

            string resultado = texto.ToString().Trim();

            if (resultado.Length < 3)
            {
                throw new TranscripcionAudioException(
                    "No se detectó voz suficiente en el audio.");
            }

            bool truncado =
                resultado.Length > _options.MaxExtractedCharacters;

            if (truncado)
            {
                resultado = resultado[.._options.MaxExtractedCharacters];
            }

            return new ResultadoTranscripcionAudio
            {
                Texto = resultado,
                TextoFueTruncado = truncado
            };
        }
        catch (TranscripcionAudioException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            throw new TranscripcionAudioException(
                "No fue posible transcribir el audio.",
                exception);
        }
        finally
        {
            _semaforo.Release();
        }
    }

    private static MemoryStream ConvertirAWav16Khz(
        string rutaAudio,
        string extension)
    {
        WaveStream reader = extension.Equals(
            ".mp3",
            StringComparison.OrdinalIgnoreCase)
            ? new Mp3FileReader(rutaAudio)
            : new WaveFileReader(rutaAudio);

        using (reader)
        {
            ISampleProvider samples = reader.ToSampleProvider();

            if (samples.WaveFormat.Channels == 2)
            {
                samples = new StereoToMonoSampleProvider(samples);
            }
            else if (samples.WaveFormat.Channels > 2)
            {
                samples = new MultiplexingSampleProvider(
                    [samples],
                    1);
            }

            ISampleProvider resampled =
                samples.WaveFormat.SampleRate == 16000
                    ? samples
                    : new WdlResamplingSampleProvider(
                        samples,
                        16000);

            MemoryStream wavStream = new();

            WaveFileWriter.WriteWavFileToStream(
                wavStream,
                resampled.ToWaveProvider16());

            wavStream.Position = 0;
            return wavStream;
        }
    }
}
