using DetectorEstafas.Web.Models;
using DetectorEstafas.Web.Services;

namespace DetectorEstafas.Tests.Services;

[TestClass]
public class AnalizadorEstafasServiceTests
{
    private AnalizadorEstafasService _service = null!;

    [TestInitialize]
    public void Inicializar()
    {
        _service = new AnalizadorEstafasService();
    }

    [TestMethod]
    public void Analizar_ContenidoCotidiano_RetornaRiesgoBajo()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Hola, nos reunimos mañana a las diez.",
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Bajo, resultado.Nivel);
        Assert.AreEqual(0, resultado.Puntaje);
        Assert.IsTrue(
            resultado.Resumen.Contains(
                "no garantiza",
                StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Analizar_UrgenciaAislada_RetornaRiesgoMedio()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Urgente: comunicate con nosotros para revisar tu solicitud.",
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Medio, resultado.Nivel);
        Assert.AreEqual(20, resultado.Puntaje);
    }

    [TestMethod]
    public void Analizar_MensajeConCredencial_RetornaRiesgoMedio()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Para continuar ingresá tu contraseña.",
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Medio, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 35);
    }

    [TestMethod]
    public void Analizar_MensajeConUrgenciaYCodigo_RetornaRiesgoAlto()
    {
        const string contenido =
            "Último aviso. Tu cuenta será suspendida. " +
            "Ingresá ahora y enviá tu código de seguridad.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 50);
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "urgencia",
                    StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "credenciales",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_MensajeDeBancoSolicitandoPin_RetornaRiesgoAlto()
    {
        const string contenido =
            "Hola, somos de tu banco. Para validar tu cuenta, ingresá tu PIN.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 50);
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "entidad financiera",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_MensajeBancoSolicitaEnviesClave_Retorna80RiesgoAlto()
    {
        const string contenido =
            "Somos del banco. Necesitamos que nos envíes tu clave personal para verificar tu cuenta.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.AreEqual(80, resultado.Puntaje);
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "entidad financiera",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_MensajeMultilineaBancoSolicitaClave_RetornaRiesgoAlto()
    {
        const string contenido =
            "Somos del banco.\nNecesitamos que nos envíes\ntu clave personal para verificar tu cuenta.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.AreEqual(80, resultado.Puntaje);
    }

    [TestMethod]
    public void Analizar_BancoAdvierteNoCompartirClave_NoEscalaAAlto()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Banco: nunca compartas tu clave con nadie.",
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Medio, resultado.Nivel);
        Assert.AreEqual(35, resultado.Puntaje);
        Assert.IsFalse(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "entidad financiera solicita",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_TransferenciaUrgente_RetornaRiesgoAlto()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Urgente: realizá una transferencia ahora mismo para evitar problemas.",
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 50);
    }

    [TestMethod]
    public void Analizar_BloqueoYEnlaceInseguro_RetornaRiesgoAlto()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Tu cuenta será bloqueada. Verificá ahora en http://192.168.1.50/acceso",
            TipoContenido.Mensaje);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 50);
    }

    [TestMethod]
    public void Analizar_AccesoRemoto_RetornaAlMenosRiesgoMedio()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Instalá AnyDesk para que podamos ayudarte con tu cuenta.",
            TipoContenido.Mensaje);

        Assert.AreNotEqual(NivelRiesgo.Bajo, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 45);
    }

    [TestMethod]
    public void Analizar_EnlaceHttpsNormal_RetornaRiesgoBajo()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "https://www.microsoft.com",
            TipoContenido.Enlace);

        Assert.AreEqual(NivelRiesgo.Bajo, resultado.Nivel);
        Assert.AreEqual(0, resultado.Puntaje);
        Assert.AreEqual(1, resultado.EnlacesAnalizados.Count);

        EnlaceAnalizado enlace = resultado.EnlacesAnalizados[0];
        Assert.IsTrue(enlace.EsValido);
        Assert.IsTrue(enlace.UsaHttps);
        Assert.AreEqual("www.microsoft.com", enlace.Dominio);
    }

    [TestMethod]
    public void Analizar_EnlaceConIpHttps_RetornaRiesgoMedio()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "https://192.168.1.50/verificar",
            TipoContenido.Enlace);

        Assert.AreEqual(NivelRiesgo.Medio, resultado.Nivel);
        Assert.AreEqual(35, resultado.Puntaje);
    }

    [TestMethod]
    public void Analizar_EnlaceHttpConIp_DetectaSenalesTecnicas()
    {
        const string contenido =
            "http://192.168.1.50:8080/verificar";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Enlace);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.AreEqual(1, resultado.EnlacesAnalizados.Count);

        EnlaceAnalizado enlace = resultado.EnlacesAnalizados[0];

        Assert.IsFalse(enlace.UsaHttps);
        Assert.IsTrue(
            enlace.Senales.Any(
                senal => senal.Contains(
                    "HTTPS",
                    StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(
            enlace.Senales.Any(
                senal => senal.Contains(
                    "dirección IP",
                    StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(
            enlace.Senales.Any(
                senal => senal.Contains(
                    "puerto",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_EnlaceConUsuarioEIp_RetornaRiesgoAlto()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "https://banco.com@192.168.1.50/ingreso",
            TipoContenido.Enlace);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 70);
        Assert.IsTrue(
            resultado.EnlacesAnalizados[0].Senales.Any(
                senal => senal.Contains(
                    "información antes del dominio",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_MensajeConEnlace_ExtraeYAnalizaElEnlace()
    {
        const string contenido =
            "Verificá tu cuenta en " +
            "http://192.168.1.50/verificar ahora.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Mensaje);

        Assert.AreEqual(1, resultado.EnlacesAnalizados.Count);

        EnlaceAnalizado enlace = resultado.EnlacesAnalizados[0];

        Assert.AreEqual("192.168.1.50", enlace.Dominio);
        Assert.IsTrue(
            enlace.Senales.Any(
                senal => senal.Contains(
                    "dirección IP",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_TelefonoCodigoCorto_NoMarcaFormatoInusual()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "911",
            TipoContenido.Telefono);

        Assert.AreEqual(NivelRiesgo.Bajo, resultado.Nivel);
        Assert.AreEqual(0, resultado.Puntaje);
    }

    [TestMethod]
    public void Analizar_TelefonoDemasiadoCorto_MarcaFormatoInusual()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "123456",
            TipoContenido.Telefono);

        Assert.AreEqual(NivelRiesgo.Medio, resultado.Nivel);
        Assert.AreEqual(20, resultado.Puntaje);
    }

    [TestMethod]
    public void Analizar_LlamadaConEmergenciaFamiliar_RetornaRiesgoAlto()
    {
        const string contenido =
            "Mi hijo tuvo un accidente. " +
            "Necesito que hagas una transferencia urgente " +
            "y no le digas a nadie.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Llamada);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "emergencia familiar",
                    StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "terceros",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_LlamadaEmergenciaAntesDeFamiliar_DetectaEstafa()
    {
        const string contenido =
            "Hubo un accidente con tu hijo. Hacé una transferencia urgente y no cortes la llamada.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Llamada);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "emergencia familiar",
                    StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "terceros",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_LlamadaSolicitaCodigoYDatosAcceso_RetornaRiesgoAlto()
    {
        const string contenido =
            "Hola, llamamos del banco. Detectamos un problema con tu cuenta " +
            "para evitar el bloqueo, necesitamos que nos informes del código " +
            "que vas a recibir por S, M, E, S y confirmes tus datos de acceso.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Llamada);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 50);
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "credenciales",
                    StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "bloquear",
                    StringComparison.OrdinalIgnoreCase)));
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "llamada solicita",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_LlamadaSolicitaEnviesCodigo_RetornaRiesgoAlto()
    {
        ResultadoAnalisis resultado = _service.Analizar(
            "Necesito que me envíes tu código de seguridad para continuar.",
            TipoContenido.Llamada);

        Assert.AreEqual(NivelRiesgo.Alto, resultado.Nivel);
        Assert.IsTrue(resultado.Puntaje >= 75);
        Assert.IsTrue(
            resultado.SenalesDetectadas.Any(
                senal => senal.Contains(
                    "llamada solicita",
                    StringComparison.OrdinalIgnoreCase)));
    }

    [TestMethod]
    public void Analizar_LlamadaBancariaInformativa_NoRetornaRiesgoAlto()
    {
        const string contenido =
            "Hola, llamamos del banco para informarte que tu nueva tarjeta " +
            "ya está disponible para retirar en la sucursal.";

        ResultadoAnalisis resultado = _service.Analizar(
            contenido,
            TipoContenido.Llamada);

        Assert.AreEqual(NivelRiesgo.Bajo, resultado.Nivel);
        Assert.AreEqual(0, resultado.Puntaje);
    }
}
