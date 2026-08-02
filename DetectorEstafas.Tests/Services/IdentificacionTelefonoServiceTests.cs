using DetectorEstafas.Web.Services.Telefonos;

namespace DetectorEstafas.Tests.Services;

[TestClass]
public class IdentificacionTelefonoServiceTests
{
    [TestMethod]
    public void Identificar_NumeroOficial_DevuelveFuenteVerificada()
    {
        IdentificacionTelefonoService service = new();

        var resultado = service.Identificar("911");

        Assert.IsTrue(resultado.Encontrado);
        Assert.AreEqual("Entidad oficial verificada", resultado.Clasificacion);
        Assert.AreEqual("Central de Emergencias Nacional", resultado.Entidad);
        Assert.AreEqual("Alta", resultado.NivelConfianza);
    }

    [TestMethod]
    public void Identificar_Anses_DevuelveCanalOficial()
    {
        IdentificacionTelefonoService service = new();

        var resultado = service.Identificar("130");

        Assert.IsTrue(resultado.Encontrado);
        Assert.AreEqual("ANSES", resultado.Entidad);
        Assert.AreEqual("Entidad oficial verificada", resultado.Clasificacion);
    }

    [TestMethod]
    public void Identificar_BancoNacion_ConservaPrefijo0810()
    {
        IdentificacionTelefonoService service = new();

        var resultado = service.Identificar("0810-666-4444");

        Assert.IsTrue(resultado.Encontrado);
        Assert.AreEqual("08106664444", resultado.NumeroNormalizado);
        Assert.AreEqual("Banco de la Nación Argentina", resultado.Entidad);
    }

    [TestMethod]
    public void Identificar_RedLink_DistingueOrganizacionDeEntidadVerificada()
    {
        IdentificacionTelefonoService service = new();

        var resultado = service.Identificar("0800-888-5465");

        Assert.IsTrue(resultado.Encontrado);
        Assert.AreEqual(
            "Organización encontrada en una fuente pública oficial",
            resultado.Clasificacion);
        Assert.AreEqual("Media-Alta", resultado.NivelConfianza);
    }

    [TestMethod]
    public void Identificar_NumeroConCodigoPais_NormalizaCorrectamente()
    {
        IdentificacionTelefonoService service = new();

        var resultado = service.Identificar("+54 11 4348-3500");

        Assert.IsTrue(resultado.Encontrado);
        Assert.AreEqual("Banco Central de la República Argentina", resultado.Entidad);
    }

    [TestMethod]
    public void Identificar_NumeroDesconocido_NoAtribuyePropietario()
    {
        IdentificacionTelefonoService service = new();

        var resultado = service.Identificar("11 5555-1234");

        Assert.IsFalse(resultado.Encontrado);
        Assert.IsNull(resultado.Entidad);
        Assert.AreEqual(
            "Número sin información pública verificada",
            resultado.Clasificacion);
    }
}
