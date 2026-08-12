using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Models.InteligenciaArtificial;
using DetectorEstafas.Web.Options;
using DetectorEstafas.Web.Services;
using DetectorEstafas.Web.Services.InteligenciaArtificial;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace DetectorEstafas.Tests.Services.InteligenciaArtificial;

[TestClass]
public class AnalisisIaServiceTests
{
    [TestMethod]
    public async Task EvaluarAsync_Deshabilitada_NoInvocaProveedor()
    {
        ProveedorFalso proveedor = new();
        AnalisisIaService service = CrearServicio(
            proveedor,
            enabled: false);

        ResultadoEvaluacionIa? resultado =
            await service.EvaluarAsync(
                "Texto de prueba",
                TipoContenido.Mensaje,
                CrearResultadoLocal(),
                CancellationToken.None);

        Assert.IsNull(resultado);
        Assert.AreEqual(0, proveedor.CantidadInvocaciones);
    }

    [TestMethod]
    public async Task EvaluarAsync_RespuestaValida_RetornaEvaluacionSeparada()
    {
        ProveedorFalso proveedor = new()
        {
            Respuesta = new RespuestaProveedorIa
            {
                Resumen = "Se observan señales complementarias.",
                SenalesAdicionales = ["Suplantación de identidad"],
                Recomendaciones = ["Contactar por un canal oficial"],
                Confianza = 0.82m
            }
        };

        AnalisisIaService service = CrearServicio(
            proveedor,
            enabled: true);

        ResultadoEvaluacionIa? resultado =
            await service.EvaluarAsync(
                "Texto de prueba",
                TipoContenido.Mensaje,
                CrearResultadoLocal(),
                CancellationToken.None);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.Disponible);
        Assert.IsFalse(resultado.SeUsoFallback);
        Assert.AreEqual(0.82m, resultado.Confianza);
        Assert.AreEqual(1, resultado.SenalesAdicionales.Count);
    }

    [TestMethod]
    public async Task EvaluarAsync_RespuestaInvalida_UsaFallback()
    {
        ProveedorFalso proveedor = new()
        {
            Respuesta = new RespuestaProveedorIa
            {
                Resumen = string.Empty,
                SenalesAdicionales = [],
                Recomendaciones = [],
                Confianza = 2m
            }
        };

        AnalisisIaService service = CrearServicio(
            proveedor,
            enabled: true);

        ResultadoEvaluacionIa? resultado =
            await service.EvaluarAsync(
                "Texto de prueba",
                TipoContenido.Mensaje,
                CrearResultadoLocal(),
                CancellationToken.None);

        Assert.IsNotNull(resultado);
        Assert.IsFalse(resultado.Disponible);
        Assert.IsTrue(resultado.SeUsoFallback);
    }

    [TestMethod]
    public async Task EvaluarAsync_ProveedorFalla_ConservaResultadoLocal()
    {
        ProveedorFalso proveedor = new()
        {
            Excepcion = new InvalidOperationException("Proveedor no disponible")
        };

        AnalisisIaService service = CrearServicio(
            proveedor,
            enabled: true);

        ResultadoEvaluacionIa? resultado =
            await service.EvaluarAsync(
                "Texto de prueba",
                TipoContenido.Mensaje,
                CrearResultadoLocal(),
                CancellationToken.None);

        Assert.IsNotNull(resultado);
        Assert.IsTrue(resultado.SeUsoFallback);
        StringAssert.Contains(
            resultado.Resumen,
            "resultado principal");
    }

    private static AnalisisIaService CrearServicio(
        IProveedorEvaluacionIa proveedor,
        bool enabled)
    {
        InteligenciaArtificialOptions options = new()
        {
            Enabled = enabled,
            TimeoutSeconds = 2,
            MaxInputCharacters = 100,
            MaxAdditionalSignals = 5,
            MaxRecommendations = 5
        };

        return new AnalisisIaService(
            proveedor,
            Options.Create(options),
            NullLogger<AnalisisIaService>.Instance);
    }

    private static ResultadoAnalisis CrearResultadoLocal()
    {
        return new AnalizadorEstafasService().Analizar(
            "Transferí el dinero ahora.",
            TipoContenido.Mensaje);
    }

    private sealed class ProveedorFalso : IProveedorEvaluacionIa
    {
        public int CantidadInvocaciones { get; private set; }
        public RespuestaProveedorIa? Respuesta { get; init; }
        public Exception? Excepcion { get; init; }

        public Task<RespuestaProveedorIa> EvaluarAsync(
            SolicitudEvaluacionIa solicitud,
            CancellationToken cancellationToken)
        {
            CantidadInvocaciones++;

            if (Excepcion is not null)
            {
                throw Excepcion;
            }

            return Task.FromResult(
                Respuesta ?? new RespuestaProveedorIa
                {
                    Resumen = "Respuesta válida.",
                    SenalesAdicionales = [],
                    Recomendaciones = [],
                    Confianza = 0.5m
                });
        }
    }
}
