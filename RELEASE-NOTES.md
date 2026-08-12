# Detector de Estafas 2.1.0

Fecha de cierre: 10 de agosto de 2026.

## Objetivo

Actualizar la API comercial para utilizar períodos de consumo coherentes con cada plan, reemplazando la cuota diaria genérica.

## Cambios principales

- Plan Prueba: 14 días y 200 análisis por día.
- Plan Starter: 5.000 análisis por mes.
- Plan Growth: 25.000 análisis por mes.
- Plan A medida: cuota mensual configurable.
- Eliminación del plan Comercial genérico.
- Cómputo mensual basado en los registros diarios persistidos.
- Reinicio mensual UTC y manejo del primer período desde el inicio del plan.
- Headers de cuota con límite, restante, período y fecha de reinicio.
- Dashboard administrativo actualizado para los nuevos planes.
- Migración EF Core AddMonthlyApiQuotas.
- Migración de clientes existentes sin eliminar consumos históricos.
- 47 pruebas automatizadas aprobadas.

## Validación

- Migración aplicada correctamente sobre DetectorEstafas_Dev.
- Datos existentes preservados.
- Dashboard administrativo validado visualmente.
- Build local aprobado.
- Suite automatizada: 47/47 pruebas correctas.
- GitHub Actions pendiente hasta publicar el commit 2.1.0.

## Planes de API incluidos

La estructura técnica queda preparada para:

- Prueba: 14 días, 200 análisis/día.
- Starter: 5.000 análisis/mes.
- Growth: 25.000 análisis/mes.
- A medida: cuota mensual configurable.

Los precios comerciales y el canal de contacto no forman parte de la configuración técnica de esta versión.

## Compatibilidad

- Web responsive / PWA.
- .NET 10.
- SQL Server.
- API REST v1.

---
# Detector de Estafas 2.0.2

Fecha de cierre: 9 de agosto de 2026.

## Objetivo

Corrección de mantenimiento enfocada en mejorar la detección de riesgo en llamadas transcritas.

## Correcciones y mejoras

- Ampliada la detección de bloqueo, suspensión y variantes equivalentes.
- Mejorada la detección de solicitudes de códigos, credenciales y datos de acceso.
- Agregada una regla específica para llamadas que solicitan compartir códigos o credenciales.
- Mejorada la tolerancia del motor ante transcripciones reales de voz.
- Corregido un caso real donde una llamada sospechosa podía clasificarse con riesgo insuficiente.
- Agregadas 2 pruebas automatizadas específicas para el análisis de llamadas.

## Validación

- Compilación local aprobada.
- Suite automatizada aprobada.
- 42 pruebas automatizadas definidas.
- Validación funcional manual del flujo audio → transcripción → análisis aprobada.
- La validación final de GitHub Actions se realizará al publicar el commit final.

## Compatibilidad

- Web responsive.
- PWA instalable en navegadores compatibles.
- Instalación desde pantalla de inicio en Android y iPhone/iPad.
- .NET 10.
- SQL Server.

---


# Detector de Estafas 2.0.1

Fecha de cierre: 9 de agosto de 2026.

## Objetivo

Versión de estabilización de la aplicación web/PWA, su API y los componentes de producción.

## Correcciones y mejoras

- Restaurado el manejo global de errores y HSTS para producción.
- Eliminados middleware duplicados del pipeline HTTP.
- Corregida la composición de dependencias del registro y correo.
- Agregada la política de rate limiting del registro.
- Actualizado el User-Agent técnico a la versión 2.
- Incorporados los assets requeridos por la PWA.
- Mejorada la instalación en Android y la guía de instalación en iPhone/iPad.
- Agregado el ciclo Prueba → Comercial para clientes API.
- Agregada expiración de prueba a 14 días.
- Agregada modificación administrativa de plan y cuota.
- Reforzada la revocación y deshabilitación de clientes/claves.
- Protegido el consumo diario ante concurrencia mediante transacción serializable en SQL Server.
- Agregado CI de build y tests mediante GitHub Actions.
- Agregadas guía de usuario y guía de API comercial.
- Actualizada la política de seguridad para versión estable.

## Validación

- Compilación local aprobada.
- Suite automatizada aprobada.
- 40 pruebas automatizadas definidas.
- La validación de GitHub Actions se realizará al publicar el commit final.

## Compatibilidad

- Web responsive.
- PWA instalable en navegadores compatibles.
- Instalación desde pantalla de inicio en Android y iPhone/iPad.
- .NET 10.
- SQL Server.

---

# Detector de Estafas 2.0.0

Fecha de cierre: 2 de agosto de 2026.

## Resumen

La versión 2.0.0 incorpora capacidades multimodales, extensibilidad, API v1 y mejoras de seguridad y privacidad sobre la versión inicial.

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
- Documentación legal, política de seguridad y licencia propietaria.

## Seguridad y privacidad

- No se almacena por defecto el contenido analizado.
- Imágenes y audios se eliminan después del procesamiento.
- Las API keys completas no se guardan en SQL Server.
- Los secretos se mantienen fuera del repositorio.
- Se validaron rate limits, autenticación de API, cuotas y acceso administrativo.
- Se aprobaron 37 pruebas automatizadas en Release.

## Compatibilidad

- .NET 10.
- SQL Server.
- ASP.NET Core MVC.
- Navegadores modernos con soporte PWA y MediaRecorder.

## Publicación

El código se distribuye bajo una licencia propietaria de código visible. La revisión del código está permitida; su explotación comercial, redistribución o uso productivo requieren autorización escrita.
