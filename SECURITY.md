# Política de seguridad

## Versiones cubiertas

Mientras la versión 2.0.0 permanezca en estado RC, solo se reciben reportes sobre la rama principal vigente. Las versiones anteriores pueden no recibir correcciones.

## Cómo reportar una vulnerabilidad

No abras un issue público con:

- pasos de explotación;
- claves, tokens o credenciales;
- información personal;
- archivos maliciosos;
- datos de clientes;
- rutas internas de producción.

Usá el canal de contacto publicado en:

https://www.federicostimpfl.com.ar

Incluí, cuando sea posible:

- componente afectado;
- versión o commit;
- impacto observado;
- pasos mínimos para reproducir;
- evidencia sanitizada;
- propuesta de mitigación.

## Tratamiento del reporte

El proyecto intentará:

1. confirmar recepción;
2. evaluar impacto y reproducibilidad;
3. priorizar la corrección;
4. coordinar la divulgación responsable;
5. publicar una corrección sin exponer datos sensibles.

No se ofrece recompensa económica ni programa formal de bug bounty.

## Alcance

Son especialmente relevantes los reportes relacionados con:

- carga de imágenes o audios;
- traversal, ejecución o persistencia indebida de archivos;
- fuga de contenido analizado;
- autenticación por API key;
- acceso cruzado a clientes API;
- bypass de cuotas;
- sesión administrativa;
- inyección de prompt o respuesta no validada de IA;
- exposición de secretos;
- CSP, antiforgery, rate limiting y encabezados de seguridad.
