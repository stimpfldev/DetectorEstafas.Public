"use strict";

(() => {
    const language =
        (document.documentElement.lang || "es")
            .toLowerCase();

    if (language !== "en") {
        return;
    }

    const translations = new Map([
        ["Analizar contenido sospechoso", "Analyze suspicious content"],
        ["Inicio", "Home"],
        ["Planes", "Plans"],
        ["Contacto", "Contact"],
        ["Crear cuenta", "Create account"],
        ["Instalar aplicación", "Install app"],
        ["Herramienta preventiva para identificar señales de riesgo antes de responder, abrir un enlace o transferir dinero.", "A preventive tool to identify warning signs before replying, opening a link, or transferring money."],
        ["El resultado es orientativo y no garantiza que una comunicación sea segura o fraudulenta.", "Results are for guidance only and do not guarantee that a communication is safe or fraudulent."],
        ["Privacidad", "Privacy"],
        ["Condiciones de uso", "Terms of use"],
        ["Licencias y terceros", "Licenses and third parties"],
        ["Información del sitio", "Site information"],

        ["Protección digital simple", "Simple digital protection"],
        ["Detectá señales de estafa", "Detect scam warning signs"],
        ["antes de actuar.", "before you act."],
        ["Analizá mensajes, enlaces, números telefónicos o llamadas sospechosas y recibí una evaluación clara del riesgo.", "Analyze suspicious messages, links, phone numbers, or calls and get a clear risk assessment."],
        ["Resultado inmediato", "Immediate result"],
        ["Explicación en lenguaje simple", "Plain-language explanation"],
        ["Recomendaciones concretas", "Actionable recommendations"],
        ["Antes de responder", "Before you respond"],
        ["Verificá las señales de riesgo.", "Check the warning signs."],
        ["Nueva entrada", "New input"],
        ["Validar una captura", "Validate a screenshot"],
        ["Cargá una imagen PNG o JPEG de hasta 5 MB. El OCR se ejecuta localmente, el texto se analiza con el motor existente y la imagen se elimina automáticamente.", "Upload a PNG or JPEG image up to 5 MB. OCR runs locally, the text is analyzed by the existing engine, and the image is deleted automatically."],
        ["Captura de pantalla", "Screenshot"],
        ["PNG o JPEG · máximo 5 MB · en móvil podés usar cámara o galería", "PNG or JPEG · max 5 MB · on mobile you can use the camera or gallery"],
        ["Agregar una evaluación complementaria con IA externa. Se enviará únicamente el contenido procesado y el resultado local.", "Add a complementary assessment using external AI. Only the processed content and local result will be sent."],
        ["Extraer y analizar texto", "Extract and analyze text"],
        ["Captura rechazada", "Screenshot rejected"],
        ["Texto extraído, analizado e imagen eliminada", "Text extracted and analyzed; image deleted"],
        ["El texto fue limitado a 5000 caracteres.", "The text was limited to 5,000 characters."],
        ["Vista previa temporal de la captura validada", "Temporary preview of the validated screenshot"],
        ["Validar un audio", "Validate audio"],
        ["Cargá un archivo MP3 o WAV de hasta 10 MB. El audio se valida, se transcribe localmente, se analiza y se elimina automáticamente.", "Upload an MP3 or WAV file up to 10 MB. The audio is validated, transcribed locally, analyzed, and deleted automatically."],
        ["Audio o grabación", "Audio or recording"],
        ["Elegir audio", "Choose audio"],
        ["MP3, WAV, OGG/OPUS, M4A o AAC · máximo 10 MB · en móvil podés elegir un archivo o grabar", "MP3, WAV, OGG/OPUS, M4A or AAC · max 10 MB · on mobile you can choose a file or record"],
        ["Confirmo que tengo autorización para procesar esta grabación. El audio se eliminará después del análisis.", "I confirm that I am authorized to process this recording. The audio will be deleted after analysis."],
        ["Iniciar grabación", "Start recording"],
        ["Detener", "Stop"],
        ["Podés cargar un archivo o grabar hasta 60 segundos.", "You can upload a file or record up to 60 seconds."],
        ["Transcribir y analizar audio", "Transcribe and analyze audio"],
        ["Audio rechazado", "Audio rejected"],
        ["Audio transcripto, analizado y eliminado", "Audio transcribed, analyzed, and deleted"],
        ["La transcripción fue limitada a 5000 caracteres.", "The transcription was limited to 5,000 characters."],
        ["Análisis preventivo", "Preventive analysis"],
        ["¿Qué recibiste?", "What did you receive?"],
        ["Seleccioná el tipo de contenido y pegá la información que querés verificar.", "Select the content type and paste the information you want to check."],
        ["Servicio disponible", "Service available"],
        ["¿Qué querés analizar?", "What do you want to analyze?"],
        ["Tipo de contenido", "Content type"],
        ["Mensaje sospechoso", "Suspicious message"],
        ["Pegá el mensaje que recibiste.", "Paste the message you received."],
        ["Mensaje", "Message"],
        ["Enlace sospechoso", "Suspicious link"],
        ["Pegá la URL completa. Ejemplo: https://sitio-ejemplo.com", "Paste the full URL. Example: https://example-site.com"],
        ["Enlace", "Link"],
        ["Número telefónico sospechoso", "Suspicious phone number"],
        ["Ingresá el número con característica. Ejemplo: +54 11 1234 5678", "Enter the full phone number, including area code. Example: +54 11 1234 5678"],
        ["Número telefónico", "Phone number"],
        ["Descripción de la llamada", "Call description"],
        ["Describí qué te dijeron en la llamada, qué te pidieron y si te apuraron.", "Describe what they told you on the call, what they asked for, and whether they pressured you."],
        ["Llamada", "Call"],
        ["Captura", "Screenshot"],
        ["Analizá primero el contenido principal y luego revisá el resultado.", "Analyze the main content first, then review the result."],
        ["Máximo 5000 caracteres", "Maximum 5,000 characters"],
        ["Analizar contenido", "Analyze content"],
        ["No ingreses contraseñas, códigos de seguridad, datos completos de tarjetas ni información bancaria sensible.", "Do not enter passwords, security codes, full card details, or sensitive banking information."],
        ["Resultado del análisis", "Analysis result"],
        ["Señales detectadas", "Detected warning signs"],
        ["No se encontraron señales automáticas importantes.", "No significant automatic warning signs were found."],
        ["Qué hacer ahora", "What to do now"],
        ["Identificación pública del teléfono", "Public phone identification"],
        ["Fuente:", "Source:"],
        ["No se atribuye este número a una persona o entidad sin una fuente pública confiable.", "This number is not attributed to a person or organization without a reliable public source."],
        ["Enlaces analizados", "Analyzed links"],
        ["Dominio", "Domain"],
        ["Formato no válido", "Invalid format"],
        ["Conexión HTTPS", "HTTPS connection"],
        ["Sin HTTPS", "No HTTPS"],
        ["Registro del dominio", "Domain registration"],
        ["Consultando registro oficial…", "Checking official registry…"],
        ["No se detectaron señales técnicas básicas.", "No basic technical warning signs were detected."],
        ["Evaluación complementaria con IA", "Complementary AI assessment"],
        ["Confianza informada por el proveedor:", "Confidence reported by provider:"],
        ["¿Considerás que fue un intento de estafa?", "Do you think this was a scam attempt?"],
        ["Podés reportarlo anónimamente sin compartir el contenido ingresado.", "You can report it anonymously without sharing the content you entered."],
        ["Seleccioná una categoría", "Select a category"],
        ["Reportar como sospechoso", "Report as suspicious"],
        ["¿Este resultado te ayudó?", "Did this result help you?"],
        ["Tu respuesta mejora el detector.", "Your feedback improves the detector."],
        ["Sí, me ayudó", "Yes, it helped"],
        ["No fue suficiente", "It was not enough"],
        ["Compartir resultado", "Share result"],
        ["Realizar otro análisis", "Run another analysis"],
        ["Este resultado es orientativo. La ausencia de señales detectadas no garantiza que el contenido sea seguro. No se comparte el contenido ingresado.", "This result is for guidance only. The absence of detected warning signs does not guarantee that the content is safe. The entered content is not shared."],
        ["Proceso simple", "Simple process"],
        ["Cómo funciona", "How it works"],
        ["Tres pasos para evaluar una comunicación sospechosa.", "Three steps to assess a suspicious communication."],
        ["Ingresá el contenido", "Enter the content"],
        ["Pegá el mensaje, enlace, teléfono o descripción de la llamada recibida.", "Paste the message, link, phone number, or description of the call you received."],
        ["Analizamos las señales", "We analyze the warning signs"],
        ["El sistema busca patrones de urgencia, engaño, solicitudes de dinero y datos sensibles.", "The system looks for patterns of urgency, deception, requests for money, and sensitive data."],
        ["Recibí recomendaciones", "Get recommendations"],
        ["Obtené un nivel de riesgo, las señales encontradas y las acciones recomendadas.", "Get a risk level, the warning signs found, and recommended actions."],

        ["El contenido intenta generar urgencia.", "The content attempts to create urgency."],
        ["Solicita o menciona credenciales de seguridad.", "It requests or mentions security credentials."],
        ["Solicita o menciona una operación de dinero.", "It requests or mentions a money transaction."],
        ["Promete un premio o beneficio inesperado.", "It promises an unexpected prize or benefit."],
        ["Solicita instalar o utilizar una herramienta de acceso remoto.", "It asks you to install or use a remote-access tool."],
        ["Amenaza con bloquear o suspender una cuenta.", "It threatens to block or suspend an account."],
        ["Solicita o menciona datos personales o bancarios.", "It requests or mentions personal or banking information."],
        ["Contiene un enlace acortado que oculta el destino real.", "It contains a shortened link that hides the real destination."],
        ["Utiliza el nombre de una empresa u organismo conocido.", "It uses the name of a well-known company or organization."],
        ["Una entidad financiera solicita una credencial que nunca debería compartirse por este medio.", "A financial institution is asking for a credential that should never be shared through this channel."],
        ["El enlace no tiene un formato válido.", "The link does not have a valid format."],
        ["El enlace utiliza un protocolo no permitido.", "The link uses a protocol that is not allowed."],
        ["No se pudo identificar el dominio.", "The domain could not be identified."],
        ["No utiliza una conexión HTTPS.", "It does not use an HTTPS connection."],
        ["Utiliza una dirección IP en lugar de un dominio.", "It uses an IP address instead of a domain."],
        ["El dominio contiene caracteres internacionales codificados.", "The domain contains encoded international characters."],
        ["El enlace contiene información antes del dominio que puede resultar engañosa.", "The link contains information before the domain that may be misleading."],
        ["El dominio contiene una cantidad inusual de guiones.", "The domain contains an unusual number of hyphens."],
        ["El dominio contiene una cantidad inusual de subdominios.", "The domain contains an unusual number of subdomains."],
        ["Utiliza un puerto de conexión no habitual.", "It uses an unusual connection port."],
        ["El enlace es inusualmente largo.", "The link is unusually long."],
        ["El enlace contiene una cantidad elevada de parámetros.", "The link contains a large number of parameters."],
        ["El número telefónico tiene un formato inusual.", "The phone number has an unusual format."],
        ["El número contiene una secuencia repetitiva inusual.", "The number contains an unusual repeated sequence."],
        ["La persona intenta impedir que consultes con terceros.", "The caller is trying to prevent you from checking with other people."],

        ["Solicitud de dinero o transferencia", "Request for money or transfer"],
        ["Solicitud de código, clave o datos", "Request for a code, password, or data"],
        ["Suplantación de banco, empresa u organismo", "Impersonation of a bank, company, or organization"],
        ["Premio, inversión o beneficio falso", "Fake prize, investment, or benefit"],
        ["Amenaza o falsa emergencia", "Threat or fake emergency"],
        ["Otro comportamiento sospechoso", "Other suspicious behavior"],
        ["Seleccioná una categoría.", "Select a category."],
        ["No se pudo validar la solicitud.", "The request could not be validated."],
        ["Respuesta registrada. Gracias.", "Response recorded. Thank you."],
        ["No se pudo registrar la respuesta.", "The response could not be recorded."],
        ["Reporte registrado", "Report recorded"],
        ["Gracias. El reporte anónimo fue registrado.", "Thank you. The anonymous report was recorded."],
        ["No se pudo registrar el reporte.", "The report could not be recorded."],
        ["Resultado compartido.", "Result shared."],
        ["Resultado copiado al portapapeles.", "Result copied to the clipboard."],
        ["No se pudo compartir el resultado.", "The result could not be shared."],
        ["Consulta completada.", "Lookup completed."],
        ["El registro oficial no está disponible en este momento.", "The official registry is not available right now."],

        ["Planes para integrar AlertaEstafa", "Plans to integrate AlertaEstafa"],
        ["Para desarrolladores y empresas que necesitan utilizar el análisis de AlertaEstafa desde sus propios sistemas mediante API.", "For developers and businesses that need to use AlertaEstafa analysis from their own systems through the API."],
        ["¿Querés probar la API primero?", "Want to try the API first?"],
        ["Probar gratis", "Try for free"],
        ["por mes", "per month"],
        ["Clave de acceso individual", "Individual access key"],
        ["Alta automática", "Automatic activation"],
        ["Suscripción recurrente mediante Mercado Pago", "Recurring subscription through Mercado Pago"],
        ["Suscribirme", "Subscribe"],
        ["A medida", "Custom"],
        ["Consultar", "Contact us"],
        ["Capacidad personalizada", "Custom capacity"],
        ["Cuota mensual configurable", "Configurable monthly quota"],
        ["Condiciones comerciales personalizadas", "Custom commercial terms"],
        ["Atención directa", "Direct support"],
        ["Contactar", "Contact"],
        ["¿Cómo funciona la suscripción?", "How does the subscription work?"],
        ["Elegís Starter o Growth, completás tus datos y continuás a Mercado Pago. Una vez aprobado el pago, el sistema procesa la confirmación y habilita el acceso técnico a la API.", "Choose Starter or Growth, enter your details, and continue to Mercado Pago. Once payment is approved, the system processes the confirmation and enables technical API access."],
        ["Los precios publicados son referencias comerciales en USD. El cobro mediante Mercado Pago se realiza en pesos argentinos por el importe equivalente definido para la suscripción.", "Published prices are commercial references in USD. Mercado Pago charges in Argentine pesos using the equivalent amount configured for the subscription."],
        ["Prueba gratuita", "Free trial"],
        ["Acepto la", "I accept the"],
        ["política de privacidad", "privacy policy"],
        ["Acepto las", "I accept the"],
        ["condiciones de uso", "terms of use"],
        ["Activar prueba", "Activate trial"],
        ["Precio de referencia en USD. El cobro mediante Mercado Pago se realiza en pesos argentinos por el importe equivalente definido para la suscripción.", "Reference price in USD. Mercado Pago charges in Argentine pesos using the equivalent amount configured for the subscription."],
        ["Continuar a Mercado Pago", "Continue to Mercado Pago"],
        ["Cancelar y volver a Planes", "Cancel and return to Plans"],
        ["El medio de pago y las renovaciones se administran mediante Mercado Pago. La factura fiscal se emite por separado.", "Payment method and renewals are managed through Mercado Pago. The tax invoice is issued separately."],
        ["Estado de suscripción", "Subscription status"],
        ["Estado actual:", "Current status:"],
        ["Volver a planes", "Back to plans"],

        ["Nombre", "Name"],
        ["Correo electrónico", "Email"],
        ["Contraseña", "Password"],
        ["Confirmar contraseña", "Confirm password"],
        ["Ingresá tu correo electrónico.", "Enter your email address."],
        ["Ingresá un correo electrónico válido.", "Enter a valid email address."],
        ["Ingresá una contraseña.", "Enter a password."],
        ["La contraseña debe tener al menos 10 caracteres.", "The password must be at least 10 characters long."],
        ["Confirmá la contraseña.", "Confirm the password."],
        ["Las contraseñas no coinciden.", "The passwords do not match."],
        ["Ingresá tu nombre.", "Enter your name."],
        ["Ingresá tu email.", "Enter your email."],
        ["El email no es válido.", "The email address is not valid."],
        ["El análisis anónimo continúa disponible. El registro requiere confirmación de correo.", "Anonymous analysis remains available. Registration requires email confirmation."],
        ["Modo desarrollo:", "Development mode:"],
        ["usá este enlace para confirmar el correo sin configurar todavía un servidor SMTP.", "use this link to confirm the email without configuring an SMTP server yet."],
        ["Confirmar correo registrado", "Confirm registered email"],
        ["Mínimo 10 caracteres, con mayúscula, minúscula, número y símbolo.", "At least 10 characters, including uppercase, lowercase, a number, and a symbol."],
        ["El registro se utiliza para crear y confirmar la cuenta.", "Registration is used to create and confirm the account."],
        ["¿Necesitás integrar AlertaEstafa en otro sistema?", "Need to integrate AlertaEstafa into another system?"],
        ["Los planes pagos corresponden al acceso técnico por API para desarrolladores y empresas.", "Paid plans provide technical API access for developers and businesses."],
        ["Ver planes", "View plans"],
        ["Confirmación de correo", "Email confirmation"],
        ["Correo confirmado", "Email confirmed"],
        ["El registro quedó confirmado correctamente.", "Registration was confirmed successfully."],
        ["No se pudo confirmar el correo", "The email could not be confirmed"],
        ["El enlace es inválido, está incompleto o ya no puede utilizarse.", "The link is invalid, incomplete, or can no longer be used."],
        ["Volver al analizador", "Back to analyzer"],

        ["¿Necesitás comunicarte?", "Need to get in touch?"],
        ["Este canal está previsto para consultas generales, soporte, privacidad y propuestas relacionadas con AlertaEstafa.", "This channel is for general inquiries, support, privacy matters, and proposals related to AlertaEstafa."],
        ["Correo de contacto", "Contact email"],
        ["Podés escribir al siguiente correo:", "You can email us at:"],
        ["El correo de contacto se habilitará próximamente.", "The contact email will be enabled soon."],

        ["Clave de acceso", "Access key"],
        ["Tu clave de acceso", "Your access key"],
        ["El enlace no es válido, ya fue utilizado o expiró. Contactá a soporte si necesitás regenerar la clave.", "The link is invalid, has already been used, or has expired. Contact support if you need to regenerate the key."],
        ["Copiá esta clave ahora. No volverá a mostrarse.", "Copy this key now. It will not be shown again."],
        ["Copiar", "Copy"],
        ["Guardala en un lugar seguro. Si vas a integrar Detector de Estafas con otro sistema, usala como X-Api-Key al llamar a POST /api/v1/analisis.", "Store it in a safe place. If you integrate AlertaEstafa with another system, use it as X-Api-Key when calling POST /api/v1/analisis."],
        ["Por seguridad, la clave de acceso se muestra una sola vez. Presioná el botón cuando estés listo para copiarla y guardarla.", "For security, the access key is shown only once. Press the button when you are ready to copy and store it."],
        ["Mostrar clave de acceso", "Show access key"],

        ["Enlace no disponible", "Link unavailable"],
        ["Resultado compartido", "Shared result"],
        ["Este enlace no está disponible", "This link is unavailable"],
        ["El vínculo es inválido, fue modificado o ya venció.", "The link is invalid, was modified, or has expired."],
        ["Ir al analizador", "Go to analyzer"],
        ["Tipo analizado", "Analyzed type"],
        ["Fecha", "Date"],
        ["Por privacidad, este enlace no incluye el mensaje, enlace, teléfono, imagen, audio ni transcripción originales.", "For privacy, this link does not include the original message, link, phone number, image, audio, or transcription."],
        ["Analizar otro contenido", "Analyze other content"],

        ["Información legal", "Legal information"],
        ["Política de privacidad", "Privacy Policy"],
        ["Última actualización: 30 de agosto de 2026.", "Last updated: August 30, 2026."],
        ["1. Finalidad del servicio", "1. Purpose of the service"],
        ["AlertaEstafa permite analizar mensajes, enlaces, teléfonos, descripciones de llamadas y capturas para identificar señales habituales de fraude.", "AlertaEstafa lets users analyze messages, links, phone numbers, call descriptions, and screenshots to identify common fraud warning signs."],
        ["El resultado es automático y orientativo. No confirma por sí solo que una comunicación sea legítima o fraudulenta.", "The result is automated and for guidance only. By itself, it does not confirm that a communication is legitimate or fraudulent."],
        ["2. Contenido analizado", "2. Analyzed content"],
        ["El contenido se procesa para generar el resultado solicitado. Por defecto, la aplicación no guarda en su base de datos mensajes, enlaces completos, números telefónicos ni capturas.", "Content is processed to generate the requested result. By default, the application does not store messages, full links, phone numbers, or screenshots in its database."],
        ["No ingreses contraseñas, códigos de autenticación, datos bancarios, documentos ni información personal innecesaria.", "Do not enter passwords, authentication codes, banking details, documents, or unnecessary personal information."],
        ["3. Capturas y OCR", "3. Screenshots and OCR"],
        ["Las capturas aceptadas se almacenan temporalmente fuera del contenido público de la aplicación, se procesan y se eliminan al finalizar. También existe una limpieza de archivos temporales abandonados.", "Accepted screenshots are stored temporarily outside the application's public content, processed, and deleted when processing finishes. Abandoned temporary files are also cleaned up."],
        ["El texto obtenido por OCR no se guarda por defecto en la base de datos. Puede permanecer brevemente en memoria durante el análisis y mostrarse en la respuesta al usuario.", "Text obtained through OCR is not stored in the database by default. It may remain briefly in memory during analysis and be shown in the response to the user."],
        ["4. Evaluación opcional mediante IA externa", "4. Optional assessment using external AI"],
        ["La evaluación de IA es complementaria, se muestra separada del resultado local y solo se ejecuta cuando el usuario presta consentimiento explícito.", "The AI assessment is complementary, is shown separately from the local result, and only runs when the user gives explicit consent."],
        ["En ese caso puede enviarse al proveedor configurado una versión limitada del texto necesario para la evaluación. No se envían archivos binarios ni imágenes cuando no sean necesarios para el flujo configurado.", "In that case, a limited version of the text needed for the assessment may be sent to the configured provider. Binary files and images are not sent when they are not required by the configured flow."],
        ["El proveedor externo aplica sus propios términos, políticas de retención, seguridad y tratamiento. Esas condiciones deben verificarse antes de cada publicación o cambio de proveedor.", "The external provider applies its own terms and retention, security, and processing policies. Those conditions must be reviewed before each release or provider change."],
        ["5. Registro de cuentas y correo", "5. Account registration and email"],
        ["El registro de cuenta puede conservar correo electrónico, hash de contraseña, estado de confirmación, fecha de registro y datos técnicos propios de ASP.NET Core Identity.", "Account registration may retain the email address, password hash, confirmation status, registration date, and technical data used by ASP.NET Core Identity."],
        ["Las contraseñas no se guardan en texto plano. El contenido analizado no se incorpora al registro de cuenta.", "Passwords are not stored in plain text. Analyzed content is not added to the account record."],
        ["El correo de confirmación puede enviarse mediante un proveedor SMTP configurado por el responsable del servicio. Ese proveedor puede procesar los datos técnicos necesarios para entregar el mensaje.", "The confirmation email may be sent through an SMTP provider configured by the service operator. That provider may process the technical data required to deliver the message."],
        ["6. Métricas, feedback y reportes", "6. Metrics, feedback, and reports"],
        ["Para evaluar y proteger el servicio pueden conservarse:", "To evaluate and protect the service, the following may be retained:"],
        ["fecha y hora del análisis;", "analysis date and time;"],
        ["tipo de contenido;", "content type;"],
        ["nivel y puntaje de riesgo;", "risk level and score;"],
        ["cantidad de señales;", "number of warning signs;"],
        ["origen general del análisis;", "general analysis source;"],
        ["respuesta de utilidad;", "usefulness feedback;"],
        ["categoría cerrada de reporte comunitario.", "closed-category community report."],
        ["Estas métricas no incluyen el contenido completo analizado.", "These metrics do not include the full analyzed content."],
        ["7. API comercial y administración", "7. Commercial API and administration"],
        ["Para clientes API se conservan datos de la cuenta técnica, plan, cuota, estado, prefijo de clave, hash de la clave, consumo diario y fechas de uso. La clave completa no se almacena en SQL Server.", "For API clients, technical account data, plan, quota, status, key prefix, key hash, daily usage, and usage dates are retained. The full key is not stored in SQL Server."],
        ["El dashboard administrativo utiliza una sesión técnica temporal protegida. No debe mostrar claves completas ni contenido analizado.", "The administrative dashboard uses a protected temporary technical session. It must not display full keys or analyzed content."],
        ["8. Dirección IP, límites y logs", "8. IP address, limits, and logs"],
        ["La dirección IP puede utilizarse temporalmente en memoria para limitar solicitudes abusivas. La aplicación no la guarda deliberadamente en sus tablas funcionales actuales.", "The IP address may be used temporarily in memory to limit abusive requests. The application does not deliberately store it in its current functional tables."],
        ["El proveedor de alojamiento, proxy o infraestructura puede generar logs técnicos propios conforme a su configuración y política.", "The hosting, proxy, or infrastructure provider may generate its own technical logs according to its configuration and policy."],
        ["9. Servicios y fuentes externas", "9. External services and sources"],
        ["Para dominios .ar puede enviarse únicamente el dominio normalizado al servicio RDAP de NIC Argentina. Las referencias telefónicas pueden enlazar fuentes públicas oficiales u organizacionales.", "For .ar domains, only the normalized domain may be sent to NIC Argentina's RDAP service. Phone references may link to official or organizational public sources."],
        ["No se afirma que un número pertenezca legalmente a una persona particular.", "The service does not claim that a number legally belongs to a particular person."],
        ["10. Cookies, almacenamiento local y analítica web", "10. Cookies, local storage, and web analytics"],
        ["Se utilizan cookies técnicas para antiforgery, Identity cuando corresponde y sesión administrativa. La PWA puede almacenar archivos estáticos para instalación y funcionamiento sin conexión.", "Technical cookies are used for antiforgery, Identity where applicable, and administrative sessions. The PWA may store static files for installation and offline operation."],
        ["AlertaEstafa puede utilizar Google Analytics para obtener métricas agregadas sobre el uso del sitio, como visitas, páginas consultadas y funcionamiento general de las herramientas.", "AlertaEstafa may use Google Analytics to obtain aggregate site-usage metrics such as visits, pages viewed, and general tool performance."],
        ["Cuando Google Analytics está habilitado, su funcionamiento respeta las preferencias de privacidad aplicables según la región del visitante. En las regiones configuradas para requerir una decisión previa, la medición permanece restringida hasta que el usuario establezca sus preferencias mediante el mecanismo de consentimiento correspondiente.", "When Google Analytics is enabled, its operation follows the privacy preferences applicable to the visitor's region. In regions configured to require a prior choice, measurement remains restricted until the user sets their preferences through the applicable consent mechanism."],
        ["11. Conservación y eliminación", "11. Retention and deletion"],
        ["Los archivos temporales se eliminan tras el procesamiento y por tareas de limpieza. Los registros técnicos, cuentas y consumos se conservan durante el tiempo necesario para operar, asegurar, auditar y mejorar el servicio.", "Temporary files are deleted after processing and through cleanup tasks. Technical records, accounts, and usage data are retained for as long as necessary to operate, secure, audit, and improve the service."],
        ["Los períodos concretos de conservación, eliminación y respaldo deben establecerse en la política operativa aplicable.", "Specific retention, deletion, and backup periods must be established in the applicable operating policy."],
        ["12. Derechos y contacto", "12. Rights and contact"],
        ["Las consultas sobre acceso, corrección, eliminación o privacidad deben dirigirse mediante el canal publicado en: www.federicostimpfl.com.ar.", "Requests concerning access, correction, deletion, or privacy should be submitted through the channel published at: www.federicostimpfl.com.ar."],
        ["Este documento debe revisarse cuando cambien las prácticas de tratamiento, conservación o los proveedores utilizados.", "This document must be reviewed whenever processing or retention practices, or the providers used, change."],

        ["1. Alcance", "1. Scope"],
        ["AlertaEstafa es una herramienta preventiva que identifica señales técnicas y patrones frecuentemente asociados con fraudes.", "AlertaEstafa is a preventive tool that identifies technical warning signs and patterns frequently associated with fraud."],
        ["2. Resultado orientativo", "2. Guidance-only result"],
        ["El nivel, puntaje y explicaciones son estimaciones automáticas. Pueden existir falsos positivos, falsos negativos, datos incompletos y situaciones no contempladas.", "The level, score, and explanations are automated estimates. False positives, false negatives, incomplete data, and unanticipated situations may occur."],
        ["Un riesgo bajo no garantiza que una comunicación sea segura.", "A low risk level does not guarantee that a communication is safe."],
        ["3. Motor local e inteligencia artificial", "3. Local engine and artificial intelligence"],
        ["El resultado local determinista constituye la evaluación principal. La IA, cuando se habilita y autoriza, es complementaria y puede fallar, responder de forma incompleta o interpretar incorrectamente el contenido.", "The deterministic local result is the primary assessment. AI, when enabled and authorized, is complementary and may fail, respond incompletely, or misinterpret the content."],
        ["Ningún resultado debe utilizarse como única base para decisiones financieras, legales, médicas, de seguridad o entrega de datos personales.", "No result should be used as the sole basis for financial, legal, medical, security, or personal-data disclosure decisions."],
        ["4. Uso responsable", "4. Responsible use"],
        ["No está permitido utilizar el servicio para:", "The service may not be used for:"],
        ["actividades ilegales o fraudulentas;", "illegal or fraudulent activities;"],
        ["hostigar, acusar o identificar públicamente a terceros;", "harassing, accusing, or publicly identifying third parties;"],
        ["cargar datos personales ajenos sin base válida;", "uploading another person's personal data without a valid basis;"],
        ["subir malware o archivos diseñados para evadir controles;", "uploading malware or files designed to evade controls;"],
        ["eludir límites, cuotas, autenticación o controles de seguridad;", "bypassing limits, quotas, authentication, or security controls;"],
        ["realizar automatización masiva sin autorización;", "performing mass automation without authorization;"],
        ["extraer, copiar o explotar comercialmente el servicio sin licencia.", "extracting, copying, or commercially exploiting the service without a license."],
        ["5. Capturas", "5. Screenshots"],
        ["El usuario debe contar con autorización suficiente para procesar capturas y comunicaciones. No debe cargar conversaciones privadas o datos de terceros cuando no sea necesario o legítimo.", "The user must have sufficient authorization to process screenshots and communications. Private conversations or third-party data must not be uploaded when doing so is unnecessary or illegitimate."],
        ["6. Registro y correo", "6. Registration and email"],
        ["El usuario debe proporcionar un correo propio y mantenerlo actualizado. No se permite registrar identidades falsas, automatizar altas o abusar de los mecanismos de confirmación.", "The user must provide an email address they control and keep it up to date. Registering false identities, automating sign-ups, or abusing confirmation mechanisms is not allowed."],
        ["7. API y dashboard", "7. API and dashboard"],
        ["El acceso API requiere una clave válida, un cliente habilitado y cuota disponible. Las claves son personales para cada integración y no deben compartirse públicamente.", "API access requires a valid key, an enabled client, and available quota. Keys are specific to each integration and must not be shared publicly."],
        ["El servicio puede limitar, suspender, revocar o bloquear accesos por abuso, riesgo, incumplimiento, consumo excesivo o razones operativas.", "The service may limit, suspend, revoke, or block access because of abuse, risk, non-compliance, excessive usage, or operational reasons."],
        ["8. Fuentes telefónicas y externas", "8. Phone and external sources"],
        ["Las referencias de teléfonos y dominios dependen de fuentes públicas o servicios externos que pueden estar incompletos, desactualizados o no disponibles.", "Phone and domain references depend on public sources or external services that may be incomplete, outdated, or unavailable."],
        ["El servicio no identifica legalmente personas particulares ni garantiza la titularidad actual de un número.", "The service does not legally identify private individuals or guarantee the current ownership of a number."],
        ["9. Reportes comunitarios", "9. Community reports"],
        ["Los reportes representan una percepción del usuario. No constituyen una denuncia, sentencia ni confirmación pública de fraude.", "Reports reflect a user's perception. They do not constitute a complaint, judgment, or public confirmation of fraud."],
        ["10. Disponibilidad y cambios", "10. Availability and changes"],
        ["No se garantiza funcionamiento ininterrumpido, ausencia total de errores ni disponibilidad permanente de proveedores, modelos o fuentes externas.", "Uninterrupted operation, complete absence of errors, and permanent availability of providers, models, or external sources are not guaranteed."],
        ["Las funciones, límites, proveedores, planes y condiciones pueden cambiar antes de la publicación definitiva o durante la evolución del producto.", "Features, limits, providers, plans, and conditions may change before a definitive release or as the product evolves."],
        ["11. Propiedad intelectual y licencia", "11. Intellectual property and license"],
        ["El código propio, diseño, reglas originales, documentación, nombre e identidad visual están protegidos. El repositorio público, cuando exista, será de código visible bajo licencia propietaria y no constituirá software open source.", "Original code, design, rules, documentation, name, and visual identity are protected. Any public repository, when available, is source-visible under a proprietary license and does not constitute open-source software."],
        ["La visualización o fork técnico no concede derecho de uso productivo, redistribución, comercialización, marca blanca ni creación de productos derivados sin autorización escrita.", "Viewing or technically forking the repository does not grant rights for production use, redistribution, commercialization, white-labeling, or creation of derivative products without written authorization."],
        ["Los componentes de terceros conservan sus licencias respectivas.", "Third-party components retain their respective licenses."],
        ["12. Limitación de responsabilidad", "12. Limitation of liability"],
        ["El usuario conserva la responsabilidad sobre sus decisiones, el contenido procesado y el cumplimiento legal aplicable. El servicio se ofrece sin garantía de exactitud absoluta, disponibilidad o adecuación a un caso particular.", "The user remains responsible for their decisions, processed content, and applicable legal compliance. The service is provided without a guarantee of absolute accuracy, availability, or suitability for a particular case."],
        ["13. Revisión jurídica", "13. Legal review"],
        ["Estas condiciones constituyen una base funcional. Antes de explotación comercial deben ser revisadas por un profesional jurídico habilitado en Argentina.", "These terms constitute a functional baseline. Before commercial exploitation, they should be reviewed by a qualified legal professional in Argentina."],

        ["Licencias y servicios de terceros", "Third-party licenses and services"],
        ["Componentes y fuentes utilizados por la aplicación.", "Components and sources used by the application."],
        ["Plataforma y framework de Microsoft utilizados bajo licencia MIT.", "Microsoft platform and framework used under the MIT License."],
        ["Copyright de Microsoft Corporation y sus colaboradores.", "Copyright Microsoft Corporation and contributors."],
        ["Biblioteca utilizada para componentes y comportamiento visual responsivo, distribuida bajo licencia MIT.", "Library used for components and responsive visual behavior, distributed under the MIT License."],
        ["Copyright de Bootstrap Authors y sus colaboradores.", "Copyright Bootstrap Authors and contributors."],
        ["Biblioteca JavaScript incluida por las dependencias visuales y de validación del proyecto, distribuida bajo licencia MIT.", "JavaScript library included through the project's visual and validation dependencies, distributed under the MIT License."],
        ["Copyright de jQuery Project y sus colaboradores.", "Copyright jQuery Project and contributors."],
        ["NIC Argentina y RDAP", "NIC Argentina and RDAP"],
        ["La aplicación consulta el servicio RDAP de NIC Argentina para obtener información registral disponible sobre dominios terminados en .ar.", "The application queries NIC Argentina's RDAP service to obtain available registration information for domains ending in .ar."],
        ["AlertaEstafa no representa, controla ni mantiene el servicio de NIC Argentina y no existe afiliación o aprobación implícita.", "AlertaEstafa does not represent, control, or maintain NIC Argentina's service, and no affiliation or endorsement is implied."],
        ["Conservación de avisos", "Preservation of notices"],
        ["Los avisos de copyright y los textos de licencia incluidos con cada biblioteca deben conservarse en el código fuente y en los paquetes publicados.", "Copyright notices and license texts included with each library must be preserved in source code and published packages."],

        ["Ya existe una prueba o acceso API asociado a ese correo.", "A trial or API access is already associated with that email address."],
        ["El plan seleccionado no es válido.", "The selected plan is not valid."],
        ["Los pagos todavía no están habilitados.", "Payments are not enabled yet."],
        ["El precio de cobro del plan no está configurado.", "The plan's charge amount is not configured."],
        ["Ya existe una suscripción activa o pendiente para ese correo.", "An active or pending subscription already exists for that email address."],
        ["No fue posible iniciar el pago en este momento.", "The payment could not be started at this time."],
        ["Se alcanzó el límite temporal de solicitudes.", "The temporary request limit has been reached."],
        ["Volver", "Back"],
        ["Ocurrió un error", "An error occurred"],
        ["Intentá nuevamente en unos minutos.", "Please try again in a few minutes."]
    ]);

    const skippedTags = new Set([
        "SCRIPT",
        "STYLE",
        "CODE",
        "PRE",
        "TEXTAREA",
        "NOSCRIPT"
    ]);

    const normalized = value =>
        value.replace(/\s+/g, " ").trim();

    function riskName(value) {
        switch (value) {
            case "Alto":
                return "High";
            case "Medio":
                return "Medium";
            case "Bajo":
                return "Low";
            default:
                return value;
        }
    }

    function translateDynamic(value) {
        let match = value.match(/^Riesgo (Alto|Medio|Bajo)$/);
        if (match) {
            return `${riskName(match[1])} risk`;
        }

        match = value.match(/^(Alto|Medio|Bajo)$/);
        if (match) {
            return riskName(match[1]);
        }

        match = value.match(/^(\d[\d.,]*) análisis por mes$/);
        if (match) {
            return `${match[1]} analyses per month`;
        }

        match = value.match(/^Prueba gratuita durante 14 días, con hasta (\d+) análisis por día\.$/);
        if (match) {
            return `Free trial for 14 days, with up to ${match[1]} analyses per day.`;
        }

        match = value.match(/^Tenés 14 días con hasta (\d+) análisis por día\.$/);
        if (match) {
            return `You have 14 days with up to ${match[1]} analyses per day.`;
        }

        match = value.match(/^USD ([\d.,]+) \/ mes · ([\d.,]+) análisis mensuales\.$/);
        if (match) {
            return `USD ${match[1]} / month · ${match[2]} monthly analyses.`;
        }

        match = value.match(/^Confianza: (.+)$/);
        if (match) {
            return `Confidence: ${match[1]}`;
        }

        match = value.match(/^Registrado: (.+)$/);
        if (match) {
            return `Registered: ${match[1]}`;
        }

        match = value.match(/^Antigüedad: (\d+) días$/);
        if (match) {
            return `Age: ${match[1]} days`;
        }

        match = value.match(/^(.+) · verificada el (.+)$/);
        if (match) {
            return `${match[1]} · verified on ${match[2]}`;
        }

        match = value.match(/^Tu plan (.+) está activo\. Si se generó una nueva clave de acceso, recibirás por email un enlace temporal para verla una sola vez\.$/);
        if (match) {
            return `Your ${match[1]} plan is active. If a new access key was generated, you will receive a temporary one-time link by email.`;
        }

        match = value.match(/^Estado actual: (.+)\. Mercado Pago puede demorar unos instantes en confirmar la operación\. Cuando el pago quede aprobado, tu acceso se activará automáticamente\.$/);
        if (match) {
            return `Current status: ${match[1]}. Mercado Pago may take a few moments to confirm the transaction. Once payment is approved, your access will be activated automatically.`;
        }

        match = value.match(/^(.+): (El enlace no tiene un formato válido\.|El enlace utiliza un protocolo no permitido\.|No se pudo identificar el dominio\.|No utiliza una conexión HTTPS\.|Utiliza una dirección IP en lugar de un dominio\.|El dominio contiene caracteres internacionales codificados\.|El enlace contiene información antes del dominio que puede resultar engañosa\.|El dominio contiene una cantidad inusual de guiones\.|El dominio contiene una cantidad inusual de subdominios\.|Utiliza un puerto de conexión no habitual\.|El enlace es inusualmente largo\.|El enlace contiene una cantidad elevada de parámetros\.)$/);
        if (match) {
            const translatedSignal =
                translations.get(match[2]) ?? match[2];
            return `${match[1]}: ${translatedSignal}`;
        }

        return null;
    }

    function translate(value) {
        const key = normalized(value);

        if (!key) {
            return null;
        }

        return translations.get(key) ??
            translateDynamic(key);
    }

    function shouldSkip(element) {
        if (!element) {
            return true;
        }

        if (skippedTags.has(element.tagName)) {
            return true;
        }

        return element.closest("[data-i18n-skip]") !== null;
    }

    function translateTextNode(node) {
        const parent = node.parentElement;

        if (shouldSkip(parent)) {
            return;
        }

        const original = node.nodeValue ?? "";
        const translated = translate(original);

        if (!translated) {
            return;
        }

        const leading = original.match(/^\s*/)?.[0] ?? "";
        const trailing = original.match(/\s*$/)?.[0] ?? "";

        node.nodeValue =
            `${leading}${translated}${trailing}`;
    }

    function translateAttributes(element) {
        if (shouldSkip(element)) {
            return;
        }

        [
            "placeholder",
            "title",
            "aria-label",
            "alt",
            "data-label",
            "data-placeholder"
        ].forEach(attributeName => {
            if (!element.hasAttribute(attributeName)) {
                return;
            }

            const original =
                element.getAttribute(attributeName) ?? "";

            const translated = translate(original);

            if (translated) {
                element.setAttribute(
                    attributeName,
                    translated);
            }
        });
    }

    function translateElement(root) {
        if (!(root instanceof Element)) {
            return;
        }

        translateAttributes(root);

        root.querySelectorAll("*")
            .forEach(translateAttributes);

        const walker = document.createTreeWalker(
            root,
            NodeFilter.SHOW_TEXT
        );

        let node = walker.nextNode();

        while (node) {
            translateTextNode(node);
            node = walker.nextNode();
        }
    }

    function translateTitle() {
        const suffix = " - AlertaEstafa";

        if (!document.title.endsWith(suffix)) {
            return;
        }

        const title = document.title.slice(
            0,
            -suffix.length);

        const translated = translate(title);

        if (translated) {
            document.title = `${translated}${suffix}`;
        }
    }

    function translatePage() {
        translateTitle();
        translateElement(document.body);
    }

    translatePage();

    const observer = new MutationObserver(mutations => {
        mutations.forEach(mutation => {
            if (mutation.type === "characterData") {
                translateTextNode(mutation.target);
                return;
            }

            mutation.addedNodes.forEach(node => {
                if (node.nodeType === Node.TEXT_NODE) {
                    translateTextNode(node);
                }
                else if (node instanceof Element) {
                    translateElement(node);
                }
            });
        });
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true,
        characterData: true
    });

    window.AlertaEstafaI18n = {
        language: "en",
        translate
    };
})();
