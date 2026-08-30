# Detector de Estafas 2.3.0

Fecha de cierre técnico: 27 de agosto de 2026.  
Actualización de publicación: 30 de agosto de 2026.

## Objetivo

Cerrar la automatización comercial de la API para que los planes Starter y Growth puedan completar alta, pago, activación, entrega de acceso y cambios de estado sin intervención manual, manteniendo los secretos fuera del repositorio y preparando la variante de publicación para hosting compartido.

## Cambios principales

- Integradas suscripciones recurrentes Starter y Growth con Mercado Pago.
- Incorporada persistencia de suscripciones comerciales y eventos de webhook.
- Agregada validación HMAC SHA-256 de webhooks mediante `x-signature`, `x-request-id` y `data.id`.
- Implementado procesamiento idempotente de eventos para evitar duplicar clientes, activaciones y entregas.
- Pago aprobado activa automáticamente la suscripción y provisiona el cliente API.
- Pago rechazado cambia la suscripción a `Impaga` y abre un período de gracia configurable.
- Vencido el período de gracia, la suscripción pasa a `Suspendida` y el cliente API se deshabilita.
- La cancelación conserva el acceso hasta la fecha final informada y luego lo deshabilita.
- Incorporada entrega temporal y de un solo uso de la API key.
- Prevenida la activación duplicada del plan Prueba.
- Cuota del plan Prueba ajustada a 20 análisis por día durante 14 días.
- Incorporadas regresiones automatizadas para prueba duplicada, firma válida/inválida, rechazo, gracia, cancelación, Growth e idempotencia.
- Corregido el validador del repositorio público para aceptar placeholders explícitos y diferenciar assets locales ignorados de archivos trackeados.
- Retirado el logging diagnóstico temporal utilizado durante la validación del webhook.
- Adaptado el paquete de publicación a las capacidades del hosting compartido seleccionado.

## Validación

- Suscripción Starter creada y cobro de prueba aprobado mediante Mercado Pago.
- Webhook firmado procesado correctamente con HTTP `200`.
- Suscripción confirmada como `Activa` en SQL Server.
- Cliente Starter habilitado y asociado a la suscripción.
- Entrega one-time de API key creada.
- Próxima renovación registrada.
- Idempotencia validada repitiendo el mismo webhook: no aumentaron eventos, clientes ni entregas.
- Build Release aprobado.
- Suite automatizada: 62/62 pruebas correctas, 0 errores, 0 omitidas.
- Validación de seguridad aprobada con `SECURITY VALIDATION PASSED`.
- Revisión de dependencias vulnerables sin bloqueo.
- Secretos reales no publicados en archivos versionados.

## Publicación

La publicación productiva utiliza configuración externa o local no versionada para dominio, HTTPS, SMTP, cadena de conexión, secretos y precios comerciales definitivos.

La prueba física definitiva en iPhone/iPad continúa pendiente por falta de dispositivo y no invalida las funcionalidades verificadas en las demás plataformas.

---

# Detector de Estafas 2.2.0

Fecha de cierre: 24 de agosto de 2026.

## Objetivo

Cerrar la etapa funcional previa a publicación, incorporando mejoras de compatibilidad móvil, validación funcional, API comercial y estabilidad general de la aplicación web/PWA.

## Cambios principales

- Corregida la navegación al refrescar después de procesar capturas.
- Mejorada la interfaz principal y el comportamiento responsive.
- Recalibrados puntajes y umbrales del motor de detección para reducir clasificaciones de riesgo insuficientes.
- Agregadas regresiones automatizadas para escenarios de riesgo Bajo, Medio y Alto.
- Mejorada la detección de transferencias urgentes, bloqueo de cuentas, credenciales y solicitudes asociadas a entidades financieras.
- Mejoradas las consultas RDAP para dominios `.ar`.
- Corregidos y validados los resultados compartidos mediante vínculos protegidos y con vencimiento.
- Revisado el comportamiento PWA y Service Worker.
- Validada la autenticación de la API comercial.
- Validados consumo, cuotas y headers de rate limit de la API.
- Validado el dashboard administrativo.
- Validado el canal de contacto configurable.

## Compatibilidad

- Web responsive.
- PWA instalable.
- Android validado.
- Compatibilidad para iPhone/iPad implementada, pendiente de prueba física.
- .NET 10.
- SQL Server.
- API REST v1.

---

# Detector de Estafas 2.1.0

Fecha de cierre: 10 de agosto de 2026.

## Objetivo

Actualizar la API comercial para utilizar períodos de consumo coherentes con cada plan, reemplazando la cuota diaria genérica.

## Cambios principales

- Plan Prueba: 14 días y 200 análisis por día en esta versión.
- Plan Starter: 5.000 análisis por mes.
- Plan Growth: 25.000 análisis por mes.
- Plan A medida: cuota mensual configurable.
- Eliminación del plan Comercial genérico.
- Cómputo mensual basado en los registros diarios persistidos.
- Reinicio mensual UTC y manejo del primer período desde el inicio del plan.
- Headers de cuota con límite, restante, período y fecha de reinicio.
- Dashboard administrativo actualizado para los nuevos planes.
- Migración EF Core `AddMonthlyApiQuotas`.
- Migración de clientes existentes sin eliminar consumos históricos.
- 47 pruebas automatizadas aprobadas.

---

# Detector de Estafas 2.0.2

Fecha de cierre: 9 de agosto de 2026.

## Objetivo

Corrección de mantenimiento enfocada en mejorar la detección de riesgo en descripciones de llamadas.

## Correcciones y mejoras

- Ampliada la detección de bloqueo, suspensión y variantes equivalentes.
- Mejorada la detección de solicitudes de códigos, credenciales y datos de acceso.
- Agregada una regla específica para llamadas que solicitan compartir códigos o credenciales.
- Agregadas pruebas automatizadas específicas para el análisis de llamadas.

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

---

# Detector de Estafas 2.0.0

Fecha de cierre: 2 de agosto de 2026.

## Resumen

La versión 2.0.0 incorporó capacidades de captura/OCR, extensibilidad, API v1 y mejoras de seguridad y privacidad sobre la versión inicial.

## Funciones incorporadas

- Capturas PNG/JPEG con validación real, almacenamiento temporal aislado, OCR local y eliminación automática.
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
- Las imágenes temporales se eliminan después del procesamiento.
- Las API keys completas no se guardan en SQL Server.
- Los secretos se mantienen fuera del repositorio.
- Se validaron rate limits, autenticación de API, cuotas y acceso administrativo.

## Compatibilidad

- .NET 10.
- SQL Server.
- ASP.NET Core MVC.
- Navegadores modernos con soporte PWA.

## Publicación

El código se distribuye bajo una licencia propietaria de código visible. La revisión del código está permitida; su explotación comercial, redistribución o uso productivo requieren autorización escrita.
