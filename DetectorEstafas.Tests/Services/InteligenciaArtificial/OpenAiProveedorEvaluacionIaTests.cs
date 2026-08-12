using System.Net;
using System.Text;
using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.InteligenciaArtificial;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services.InteligenciaArtificial;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Tests.Services.InteligenciaArtificial;

[TestClass]
public sealed class OpenAiProveedorEvaluacionIaTests
{
    [TestMethod]
    public async Task EvaluarAsync_RespuestaValida_DevuelveEstructura()
    {
        const string response = """
        {
          "choices": [
            {
              "message": {
                "content": "{\"resumen\":\"Hay señales adicionales.\",\"senalesAdicionales\":[\"Presión emocional\"],\"recomendaciones\":[\"No responder\"],\"confianza\":0.82}"
              }
            }
          ]
        }
        """;

        HttpClient httpClient = new(
            new FakeHandler(response));

        OpenAiProveedorEvaluacionIa provider = new(
            httpClient,
            Options.Create(new InteligenciaArtificialOptions
            {
                Enabled = true,
                ApiKey = "clave-prueba",
                Endpoint = "https://example.test/v1/chat/completions"
            }));

        RespuestaProveedorIa result =
            await provider.EvaluarAsync(
                CrearSolicitud(),
                CancellationToken.None);

        Assert.AreEqual(
            "Hay señales adicionales.",
            result.Resumen);

        Assert.AreEqual(1, result.SenalesAdicionales.Count);
        Assert.AreEqual(0.82m, result.Confianza);
    }

    [TestMethod]
    public async Task EvaluarAsync_SinClave_LanzaErrorControlado()
    {
        OpenAiProveedorEvaluacionIa provider = new(
            new HttpClient(new FakeHandler("{}")),
            Options.Create(new InteligenciaArtificialOptions
            {
                Enabled = true,
                ApiKey = string.Empty
            }));

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(
            () => provider.EvaluarAsync(
                CrearSolicitud(),
                CancellationToken.None));
    }

    private static SolicitudEvaluacionIa CrearSolicitud()
    {
        return new SolicitudEvaluacionIa
        {
            Contenido = "Enviá tu clave ahora",
            TipoContenido = TipoContenido.Mensaje,
            NivelLocal = NivelRiesgo.Alto,
            PuntajeLocal = 90,
            SenalesLocales = ["Solicita claves"]
        };
    }

    private sealed class FakeHandler : HttpMessageHandler
    {
        private readonly string _response;

        public FakeHandler(string response)
        {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    _response,
                    Encoding.UTF8,
                    "application/json")
            });
        }
    }
}
