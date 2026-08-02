using System.Text.RegularExpressions;
using DetectorEstafas.Web.Models.Telefonos;

namespace DetectorEstafas.Web.Services.Telefonos;

public sealed partial class IdentificacionTelefonoService
    : IIdentificacionTelefonoService
{
    private static readonly DateTime FechaVerificacionUtc =
        new(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);

    private static readonly IReadOnlyDictionary<string, EntradaTelefono> Catalogo =
        CrearCatalogo();

    public ResultadoIdentificacionTelefono Identificar(string numeroIngresado)
    {
        string normalizado = Normalizar(numeroIngresado);

        if (Catalogo.TryGetValue(normalizado, out EntradaTelefono? entrada))
        {
            return new ResultadoIdentificacionTelefono
            {
                Encontrado = true,
                NumeroNormalizado = normalizado,
                Clasificacion = entrada.Clasificacion,
                Entidad = entrada.Entidad,
                Descripcion = entrada.Descripcion,
                NivelConfianza = entrada.NivelConfianza,
                FuenteNombre = entrada.FuenteNombre,
                FuenteUrl = entrada.FuenteUrl,
                FechaConsultaUtc = FechaVerificacionUtc
            };
        }

        return new ResultadoIdentificacionTelefono
        {
            Encontrado = false,
            NumeroNormalizado = normalizado,
            Clasificacion = "Número sin información pública verificada",
            NivelConfianza = "Sin información"
        };
    }

    private static string Normalizar(string? numeroIngresado)
    {
        string numero = SoloDigitos().Replace(numeroIngresado ?? string.Empty, string.Empty);

        if (numero.StartsWith("0054", StringComparison.Ordinal))
        {
            numero = numero[4..];
        }
        else if (numero.StartsWith("54", StringComparison.Ordinal) && numero.Length > 10)
        {
            numero = numero[2..];
        }

        // En números geográficos argentinos, el prefijo internacional puede incluir 9.
        if (numero.StartsWith('9') && numero.Length == 11)
        {
            numero = numero[1..];
        }

        // Se conserva el cero inicial de líneas 0800/0810.
        if (!numero.StartsWith("0800", StringComparison.Ordinal) &&
            !numero.StartsWith("0810", StringComparison.Ordinal))
        {
            numero = numero.TrimStart('0');
        }

        return numero;
    }

    private static IReadOnlyDictionary<string, EntradaTelefono> CrearCatalogo()
    {
        Dictionary<string, EntradaTelefono> catalogo =
            new(StringComparer.Ordinal);

        Agregar(catalogo, new EntradaTelefono(
            ["911"],
            "Entidad oficial verificada",
            "Central de Emergencias Nacional",
            "Emergencias",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/tema/emergencias"));

        Agregar(catalogo, new EntradaTelefono(
            ["100"],
            "Entidad oficial verificada",
            "Bomberos",
            "Emergencias por incendios",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/tema/emergencias"));

        Agregar(catalogo, new EntradaTelefono(
            ["102"],
            "Entidad oficial verificada",
            "Línea 102",
            "Atención especializada sobre derechos de niñas, niños y adolescentes; no es un servicio de emergencia",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/capital-humano/familia/ninez-y-adolescencia/linea-102"));

        Agregar(catalogo, new EntradaTelefono(
            ["103"],
            "Entidad oficial verificada",
            "Defensa Civil",
            "Asistencia ante desastres y emergencias",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/tema/emergencias"));

        Agregar(catalogo, new EntradaTelefono(
            ["105"],
            "Entidad oficial verificada",
            "Emergencia ambiental",
            "Denuncias ante posibles catástrofes ambientales",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/tema/emergencias"));

        Agregar(catalogo, new EntradaTelefono(
            ["106"],
            "Entidad oficial verificada",
            "Emergencia náutica",
            "Asistencia ante emergencias náuticas",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/tema/emergencias"));

        Agregar(catalogo, new EntradaTelefono(
            ["107"],
            "Entidad oficial verificada",
            "Emergencias médicas",
            "Número de emergencias médicas utilizado en distintas jurisdicciones",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/salud/primerosauxilios/rcp/adultos"));

        Agregar(catalogo, new EntradaTelefono(
            ["130"],
            "Entidad oficial verificada",
            "ANSES",
            "Canal telefónico oficial para trámites y consultas",
            "Alta",
            "ANSES",
            "https://www.anses.gob.ar/institucional/canales-de-atencion"));

        Agregar(catalogo, new EntradaTelefono(
            ["137"],
            "Entidad oficial verificada",
            "Línea 137",
            "Atención a víctimas de violencia familiar y sexual",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/justicia/derechofacil/leysimple/ciberacoso"));

        Agregar(catalogo, new EntradaTelefono(
            ["144"],
            "Entidad oficial verificada",
            "Línea 144",
            "Atención, contención y asesoramiento ante situaciones de violencia de género",
            "Alta",
            "Argentina.gob.ar",
            "https://www.argentina.gob.ar/linea-144"));

        Agregar(catalogo, new EntradaTelefono(
            ["145"],
            "Entidad oficial verificada",
            "Línea 145",
            "Denuncias por trata y explotación de personas",
            "Alta",
            "Ministerio de Seguridad Nacional",
            "https://www.argentina.gob.ar/seguridad"));

        Agregar(catalogo, new EntradaTelefono(
            ["08109992722"],
            "Entidad oficial verificada",
            "ARCA",
            "Canal telefónico oficial para consultas relacionadas con monotributo",
            "Alta",
            "ARCA",
            "https://www.arca.gob.ar/monotributo/ayuda/contacto.asp"));

        Agregar(catalogo, new EntradaTelefono(
            ["1143483500", "43483500"],
            "Entidad oficial verificada",
            "Banco Central de la República Argentina",
            "Teléfono institucional publicado por el BCRA",
            "Alta",
            "BCRA",
            "https://www.bcra.gob.ar/"));

        Agregar(catalogo, new EntradaTelefono(
            ["08106664444"],
            "Entidad oficial verificada",
            "Banco de la Nación Argentina",
            "Centro de Atención al Cliente",
            "Alta",
            "Banco Nación",
            "https://www.bna.com.ar/Home/Contactenos"));

        Agregar(catalogo, new EntradaTelefono(
            ["08008885465", "1143195465", "43195465"],
            "Organización encontrada en una fuente pública oficial",
            "Red Link",
            "Línea informada por Banco Nación para denuncias por robo, hurto o extravío de tarjeta",
            "Media-Alta",
            "Banco Nación",
            "https://www.bna.com.ar/Home/TelefonosUtiles"));

        return catalogo;
    }

    private static void Agregar(
        IDictionary<string, EntradaTelefono> catalogo,
        EntradaTelefono entrada)
    {
        foreach (string numero in entrada.Numeros)
        {
            catalogo[numero] = entrada;
        }
    }

    [GeneratedRegex("[^0-9]")]
    private static partial Regex SoloDigitos();

    private sealed record EntradaTelefono(
        IReadOnlyList<string> Numeros,
        string Clasificacion,
        string Entidad,
        string Descripcion,
        string NivelConfianza,
        string FuenteNombre,
        string FuenteUrl);
}
