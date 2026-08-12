# Detector de Estafas 2.0.0

Fecha de cierre: 2 de agosto de 2026.

## Resumen

La versiÃ³n 2.0.0 cierra la segunda etapa funcional y convierte la versiÃ³n inicial en una plataforma multimodal, extensible y preparada para validaciÃ³n pÃºblica y comercial controlada.

## Funciones incorporadas

- Capturas PNG/JPEG con validaciÃ³n real, almacenamiento temporal aislado, OCR local y eliminaciÃ³n automÃ¡tica.
- Audios MP3/WAV, grabaciÃ³n desde navegador, transcripciÃ³n local y anÃ¡lisis del texto obtenido.
- EvaluaciÃ³n opcional mediante IA externa, separada del motor determinista, con consentimiento, timeout y fallback.
- Registro de cuentas y confirmaciÃ³n de correo sin inicio de sesiÃ³n pÃºblico.
- IdentificaciÃ³n prudente de lÃ­neas oficiales y organizacionales mediante fuentes pÃºblicas.
- PWA mejorada para uso mÃ³vil y recepciÃ³n de contenido compartido.
- API comercial v1 con API keys, cuotas, consumo diario y OpenAPI.
- Persistencia de clientes API, hashes de claves y consumo en SQL Server.
- Dashboard interno protegido para administrar clientes y revocar claves.
- DocumentaciÃ³n legal, polÃ­tica de seguridad, licencia propietaria y preparaciÃ³n del repositorio pÃºblico.

## Seguridad y privacidad

- No se almacena por defecto el contenido analizado.
- ImÃ¡genes y audios se eliminan despuÃ©s del procesamiento.
- Las API keys completas no se guardan en SQL Server.
- Los secretos se mantienen fuera del repositorio.
- Se validaron rate limits, autenticaciÃ³n de API, cuotas y acceso administrativo.
- Se aprobaron 37 pruebas automatizadas en Release.

## Compatibilidad

- .NET 10.
- SQL Server.
- ASP.NET Core MVC.
- Navegadores modernos con soporte PWA y MediaRecorder.

## PublicaciÃ³n

El cÃ³digo se distribuye bajo una licencia propietaria de cÃ³digo visible. La revisiÃ³n del cÃ³digo estÃ¡ permitida; su explotaciÃ³n comercial, redistribuciÃ³n o uso productivo requieren autorizaciÃ³n escrita.
