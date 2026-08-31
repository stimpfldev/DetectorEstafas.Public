using System.Net;
using System.Text.RegularExpressions;
using DetectorEstafas.Web.Models;

namespace DetectorEstafas.Web.Services;

public class AnalizadorEstafasService : IAnalizadorEstafasService
{
    private static readonly Regex UrlRegex = new(
        @"(?:(?:https?://)|(?:www\.))[^\s<>\[\]{}""']+",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant |
        RegexOptions.Compiled);

    private static readonly Regex EntidadFinancieraRegex = new(
        @"\b(banco|entidad bancaria|mercado pago|billetera virtual|tarjeta de crédito|tarjeta de credito)\b",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant |
        RegexOptions.Compiled);

    private static readonly Regex SolicitudCredencialRegex = new(
        @"\b(ingres(?:á|a|e|ar|ás|as|es|en)|indic(?:á|a|e|ar)|indiqu(?:e|es|en)|indicame|inform(?:á|a|e|ar|ás|as|es|en)|informame|envi(?:á|a|e|ar|ás|as|e|es|en)|enví(?:a|e|es|en|as)|enviame|envíame|compart(?:í|i|a|ir|ís|is|as|an|e|es)|compartime|confirm(?:á|a|e|ar|ás|as|es|en)|confirmame|dict(?:á|a|e|ar|ás|as|es|en)|dictame|decime|decinos|proporcion(?:á|a|e|ar|ás|as|es|en)|solicit(?:amos|an|a|ar|á|e|es|en)|pedimos|piden|pedirte|pedir|pedime|pedinos|pide|pidas|pidan|mand(?:á|a|e|ar|ás|as|es|en)|mandame|pas(?:á|a|e|ar|ás|as|es|en)|pasame)\b.{0,120}\b(pin|clave|contraseña|contrasena|token|código de seguridad|codigo de seguridad|código de verificación|codigo de verificacion|datos de acceso|credenciales)\b",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant |
        RegexOptions.Singleline |
        RegexOptions.Compiled);

    private static readonly Regex NegacionInmediataRegex = new(
        @"\b(no|nunca|jamás|jamas)\s+(?:(?:me|nos|te|le|les|lo|la|los|las|se)\s+)?$",
        RegexOptions.IgnoreCase |
        RegexOptions.CultureInvariant |
        RegexOptions.Compiled);

    private static readonly IReadOnlyList<ReglaAnalisis> Reglas =
        new List<ReglaAnalisis>
        {
            new(
                @"\b(urgente|inmediatamente|ahora mismo|último aviso|ultimo aviso)\b",
                20,
                "El contenido intenta generar urgencia."),

            new(
                @"\b(contraseña|contrasena|clave|pin|token|código de seguridad|codigo de seguridad|datos de acceso|credenciales(?: de acceso)?|c[oó]digo\b.{0,50}\b(?:recibir|llegar))\b",
                35,
                "Solicita o menciona credenciales de seguridad."),

            new(
                @"\b(transferencia|transferir|depositar|depósito|deposito|pagar|pago inmediato)\b",
                30,
                "Solicita o menciona una operación de dinero."),

            new(
                @"\b(premio|ganaste|sorteo|beneficio exclusivo|recompensa)\b",
                25,
                "Promete un premio o beneficio inesperado."),

            new(
                @"\b(anydesk|teamviewer|acceso remoto|control remoto)\b",
                45,
                "Solicita instalar o utilizar una herramienta de acceso remoto."),

            new(
                @"\b(bloque(?:o|ar|ada|ado|adas|ados|aremos|arán|aran|an|en)|suspend(?:ida|ido|idas|idos|er|erán|eran|en)|suspensión|suspension|inhabilitada|inhabilitado|cerrar.*cuenta)\b",
                30,
                "Amenaza con bloquear o suspender una cuenta."),

            new(
                @"\b(dni|documento|número de tarjeta|numero de tarjeta|cbu|cvu|datos bancarios)\b",
                30,
                "Solicita o menciona datos personales o bancarios."),

            new(
                @"\b(bit\.ly|tinyurl\.com|t\.co|cutt\.ly|shorturl\.at)\b",
                20,
                "Contiene un enlace acortado que oculta el destino real."),

            new(
                @"\b(mercado pago|banco nación|banco nacion|anses|afip|arca|correo argentino)\b",
                15,
                "Utiliza el nombre de una empresa u organismo conocido.")
        };

    public ResultadoAnalisis Analizar(
        string contenido,
        TipoContenido tipoContenido)
    {
        string contenidoNormalizado =
            WebUtility.HtmlDecode(contenido).Trim();

        int puntaje = 0;
        List<string> senales = new();

        foreach (ReglaAnalisis regla in Reglas)
        {
            if (!Regex.IsMatch(
                    contenidoNormalizado,
                    regla.Patron,
                    RegexOptions.IgnoreCase |
                    RegexOptions.CultureInvariant |
                    RegexOptions.Singleline))
            {
                continue;
            }

            puntaje += regla.Puntaje;
            senales.Add(regla.Descripcion);
        }

        AnalizarSolicitudCredencialesDeEntidad(
            contenidoNormalizado,
            ref puntaje,
            senales);

        AnalizarSegunTipo(
            contenidoNormalizado,
            tipoContenido,
            ref puntaje,
            senales);

        List<EnlaceAnalizado> enlacesAnalizados =
            ObtenerEnlacesAnalizados(
                contenidoNormalizado,
                tipoContenido);

        if (enlacesAnalizados.Count > 0)
        {
            puntaje += enlacesAnalizados.Max(enlace => enlace.Puntaje);

            foreach (EnlaceAnalizado enlace in enlacesAnalizados)
            {
                foreach (string senalEnlace in enlace.Senales)
                {
                    string identificador =
                        string.IsNullOrWhiteSpace(enlace.Dominio)
                            ? "El enlace"
                            : enlace.Dominio;

                    senales.Add($"{identificador}: {senalEnlace}");
                }
            }
        }

        puntaje = Math.Clamp(puntaje, 0, 100);
        NivelRiesgo nivel = ObtenerNivelRiesgo(puntaje);

        return new ResultadoAnalisis
        {
            Puntaje = puntaje,
            Nivel = nivel,
            Resumen = ObtenerResumen(nivel),
            SenalesDetectadas = senales
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList(),
            Recomendaciones = ObtenerRecomendaciones(nivel),
            EnlacesAnalizados = enlacesAnalizados
        };
    }

    private static void AnalizarSolicitudCredencialesDeEntidad(
        string contenido,
        ref int puntaje,
        List<string> senales)
    {
        if (!EntidadFinancieraRegex.IsMatch(contenido) ||
            !ContieneSolicitudCredencial(contenido))
        {
            return;
        }

        puntaje += 45;
        senales.Add(
            "Una entidad financiera solicita una credencial que nunca debería compartirse por este medio.");
    }

    private static bool ContieneSolicitudCredencial(string contenido)
    {
        MatchCollection solicitudes =
            SolicitudCredencialRegex.Matches(contenido);

        foreach (Match solicitud in solicitudes)
        {
            int inicioContexto = Math.Max(0, solicitud.Index - 32);
            string contextoPrevio = contenido[
                inicioContexto..solicitud.Index];

            if (!NegacionInmediataRegex.IsMatch(contextoPrevio))
            {
                return true;
            }
        }

        return false;
    }

    private static List<EnlaceAnalizado> ObtenerEnlacesAnalizados(
        string contenido,
        TipoContenido tipoContenido)
    {
        List<string> enlaces = new();

        if (tipoContenido == TipoContenido.Enlace)
        {
            enlaces.Add(contenido);
        }
        else
        {
            MatchCollection coincidencias = UrlRegex.Matches(contenido);

            foreach (Match coincidencia in coincidencias)
            {
                string enlace = LimpiarEnlaceExtraido(coincidencia.Value);

                if (!string.IsNullOrWhiteSpace(enlace))
                {
                    enlaces.Add(enlace);
                }
            }
        }

        return enlaces
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(5)
            .Select(AnalizarEnlaceIndividual)
            .ToList();
    }

    private static EnlaceAnalizado AnalizarEnlaceIndividual(
        string valorIngresado)
    {
        EnlaceAnalizado resultado = new()
        {
            ValorIngresado = valorIngresado
        };

        string enlaceNormalizado = NormalizarEnlace(valorIngresado);

        if (!Uri.TryCreate(
                enlaceNormalizado,
                UriKind.Absolute,
                out Uri? uri))
        {
            resultado.Puntaje = 40;
            resultado.Nivel = NivelRiesgo.Medio;
            resultado.Senales.Add(
                "El enlace no tiene un formato válido.");
            return resultado;
        }

        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
        {
            resultado.Puntaje = 40;
            resultado.Nivel = NivelRiesgo.Medio;
            resultado.Senales.Add(
                "El enlace utiliza un protocolo no permitido.");
            return resultado;
        }

        if (string.IsNullOrWhiteSpace(uri.Host))
        {
            resultado.Puntaje = 40;
            resultado.Nivel = NivelRiesgo.Medio;
            resultado.Senales.Add(
                "No se pudo identificar el dominio.");
            return resultado;
        }

        resultado.EsValido = true;
        resultado.Dominio = uri.IdnHost;
        resultado.UsaHttps = uri.Scheme == Uri.UriSchemeHttps;

        int puntaje = 0;

        if (!resultado.UsaHttps)
        {
            puntaje += 20;
            resultado.Senales.Add(
                "No utiliza una conexión HTTPS.");
        }

        if (IPAddress.TryParse(uri.Host, out _))
        {
            puntaje += 35;
            resultado.Senales.Add(
                "Utiliza una dirección IP en lugar de un dominio.");
        }

        if (uri.IdnHost.Contains(
                "xn--",
                StringComparison.OrdinalIgnoreCase))
        {
            puntaje += 35;
            resultado.Senales.Add(
                "El dominio contiene caracteres internacionales codificados.");
        }

        if (!string.IsNullOrWhiteSpace(uri.UserInfo))
        {
            puntaje += 35;
            resultado.Senales.Add(
                "El enlace contiene información antes del dominio que puede resultar engañosa.");
        }

        if (uri.Host.Count(caracter => caracter == '-') >= 3)
        {
            puntaje += 15;
            resultado.Senales.Add(
                "El dominio contiene una cantidad inusual de guiones.");
        }

        int cantidadPartesDominio = uri.Host.Split(
            '.',
            StringSplitOptions.RemoveEmptyEntries).Length;

        if (cantidadPartesDominio > 4)
        {
            puntaje += 15;
            resultado.Senales.Add(
                "El dominio contiene una cantidad inusual de subdominios.");
        }

        if (!uri.IsDefaultPort)
        {
            puntaje += 15;
            resultado.Senales.Add(
                "Utiliza un puerto de conexión no habitual.");
        }

        if (uri.AbsoluteUri.Length > 180)
        {
            puntaje += 15;
            resultado.Senales.Add(
                "El enlace es inusualmente largo.");
        }

        int cantidadParametros = uri.Query
            .TrimStart('?')
            .Split('&', StringSplitOptions.RemoveEmptyEntries)
            .Length;

        if (cantidadParametros > 5)
        {
            puntaje += 15;
            resultado.Senales.Add(
                "El enlace contiene una cantidad elevada de parámetros.");
        }

        resultado.Puntaje = Math.Clamp(puntaje, 0, 100);
        resultado.Nivel = ObtenerNivelRiesgo(resultado.Puntaje);

        return resultado;
    }

    private static string NormalizarEnlace(string enlace)
    {
        string resultado = LimpiarEnlaceExtraido(enlace.Trim());

        if (resultado.StartsWith(
                "http://",
                StringComparison.OrdinalIgnoreCase) ||
            resultado.StartsWith(
                "https://",
                StringComparison.OrdinalIgnoreCase))
        {
            return resultado;
        }

        return $"https://{resultado}";
    }

    private static string LimpiarEnlaceExtraido(string enlace)
    {
        return enlace.TrimEnd(
            '.', ',', ';', ':', '!', '?', ')', ']', '}');
    }

    private static void AnalizarSegunTipo(
        string contenido,
        TipoContenido tipoContenido,
        ref int puntaje,
        List<string> senales)
    {
        switch (tipoContenido)
        {
            case TipoContenido.Telefono:
                AnalizarTelefono(contenido, ref puntaje, senales);
                break;

            case TipoContenido.Llamada:
                AnalizarLlamada(contenido, ref puntaje, senales);
                break;
        }
    }

    private static void AnalizarTelefono(
        string contenido,
        ref int puntaje,
        List<string> senales)
    {
        string soloDigitos = Regex.Replace(
            contenido,
            @"\D",
            string.Empty);

        bool esCodigoCorto = soloDigitos.Length == 3;

        if (!esCodigoCorto &&
            (soloDigitos.Length < 8 || soloDigitos.Length > 15))
        {
            puntaje += 20;
            senales.Add(
                "El número telefónico tiene un formato inusual.");
        }

        if (contenido.Contains(
                "0000",
                StringComparison.OrdinalIgnoreCase) ||
            contenido.Contains(
                "1111",
                StringComparison.OrdinalIgnoreCase))
        {
            puntaje += 15;
            senales.Add(
                "El número contiene una secuencia repetitiva inusual.");
        }
    }

    private static void AnalizarLlamada(
        string contenido,
        ref int puntaje,
        List<string> senales)
    {
        bool intentaAislar = Regex.IsMatch(
            contenido,
            @"\b(no\s+(?:cortes|corte|cuelgues|cuelgue)(?:\s+la\s+llamada)?|no\s+(?:le\s+|se\s+lo\s+)?(?:digas|diga|cuentes|cuente)\s+a\s+nadie|no\s+hables\s+con\s+nadie|no\s+informar|mantener\s+en\s+secreto|manten(?:é|e)\s+(?:esto\s+)?en\s+secreto|cortar\s+la\s+llamada)\b",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant);

        if (intentaAislar)
        {
            puntaje += 40;
            senales.Add(
                "La persona intenta impedir que consultes con terceros.");
        }

        bool emergenciaFamiliar = Regex.IsMatch(
            contenido,
            @"(?:\b(familiar|hijo|hija|nieto|nieta)\b.{0,100}\b(accidente|detenido|detenida|emergencia|hospital|secuestrado|secuestrada)\b|\b(accidente|detenido|detenida|emergencia|hospital|secuestrado|secuestrada)\b.{0,100}\b(familiar|hijo|hija|nieto|nieta)\b)",
            RegexOptions.IgnoreCase |
            RegexOptions.CultureInvariant |
            RegexOptions.Singleline);

        if (emergenciaFamiliar)
        {
            puntaje += 45;
            senales.Add(
                "Describe una posible falsa emergencia familiar.");
        }

        if (ContieneSolicitudCredencial(contenido))
        {
            puntaje += 40;
            senales.Add(
                "La llamada solicita compartir códigos o credenciales de acceso.");
        }
    }

    private static NivelRiesgo ObtenerNivelRiesgo(int puntaje)
    {
        if (puntaje >= 50)
        {
            return NivelRiesgo.Alto;
        }

        if (puntaje >= 20)
        {
            return NivelRiesgo.Medio;
        }

        return NivelRiesgo.Bajo;
    }

    private static string ObtenerResumen(NivelRiesgo nivel)
    {
        return nivel switch
        {
            NivelRiesgo.Alto =>
                "Se detectaron señales fuertes de riesgo. No realices ninguna acción solicitada.",
            NivelRiesgo.Medio =>
                "Se detectaron señales que requieren precaución y verificación independiente.",
            _ =>
                "No se detectaron señales fuertes, pero esto no garantiza que el contenido sea seguro."
        };
    }

    private static List<string> ObtenerRecomendaciones(NivelRiesgo nivel)
    {
        if (nivel == NivelRiesgo.Alto)
        {
            return new List<string>
            {
                "No respondas el mensaje ni continúes la llamada.",
                "No abras enlaces ni descargues archivos.",
                "No compartas contraseñas, códigos, tokens ni datos bancarios.",
                "Contactá a la organización mediante su sitio o teléfono oficial.",
                "Conservá el mensaje o captura como evidencia."
            };
        }

        if (nivel == NivelRiesgo.Medio)
        {
            return new List<string>
            {
                "No actúes con urgencia.",
                "Verificá la situación utilizando un canal oficial.",
                "No compartas datos personales o bancarios.",
                "Consultá con una persona de confianza antes de continuar."
            };
        }

        return new List<string>
        {
            "Verificá siempre el remitente y el destino de los enlaces.",
            "No compartas códigos de seguridad ni contraseñas.",
            "Utilizá canales oficiales para confirmar la comunicación."
        };
    }

    private sealed record ReglaAnalisis(
        string Patron,
        int Puntaje,
        string Descripcion);
}
