# Detector de Estafas 2.0.0

Fecha de cierre: 2 de agosto de 2026.

## Resumen

La versión 2.0.0 cierra la segunda etapa funcional y convierte la versión inicial en una plataforma multimodal, extensible y preparada para validación pública y comercial controlada.

## Funciones incorporadas

- Capturas PNG/JPEG con validación real, almacenamiento temporal aislado, OCR local y eliminación automática.
- Audios MP3/WAV, grabación desde navegador, transcripción local y análisis del texto obtenido.
- Evaluación opcional mediante IA externa, separada del motor determinista, con consentimiento, timeout y fallback.
- Registro de cuentas y confirmación de correo sin inicio de sesión público.
- Identificación prudente de líneas oficiales y organizacionales mediante fuentes públicas.
- PWA mejorada para uso móvil y recepción de contenido compartido.
- API comercial v1 con API keys, cuotas, consumo diario y OpenAPI.
- Persistencia de clientes API, hashes de claves y consumo en SQL Server.
- Dashboard interno protegido para administrar clientes y revocar claves.
- Documentación legal, política de seguridad, licencia propietaria y preparación del repositorio público.

## Seguridad y privacidad

- No se almacena por defecto el contenido analizado.
- Imágenes y audios se eliminan después del procesamiento.
- Las API keys completas no se guardan en SQL Server.
- Los secretos se mantienen fuera del repositorio.
- Se validaron rate limits, autenticación de API, cuotas y acceso administrativo.
- Se aprobaron 37 pruebas automatizadas en Release.

## Funciones pospuestas

- Extensión Chrome/Edge.
- Aplicación móvil nativa.
- Inicio de sesión público e historial personal.
- Multiempresa completo, facturación y administración comercial avanzada.

## Compatibilidad

- .NET 10.
- SQL Server.
- ASP.NET Core MVC.
- Navegadores modernos con soporte PWA y MediaRecorder.

## Publicación

El código se distribuye bajo una licencia propietaria de código visible. La revisión del código está permitida; su explotación comercial, redistribución o uso productivo requieren autorización escrita.
