# API comercial — Guía de operación

## Endpoint

```text
POST /api/v1/analisis
Content-Type: application/json
X-Api-Key: <clave>
```

Documento OpenAPI:

```text
/openapi/v1.json
```

## Solicitud

`TipoContenido` acepta:

| Valor | Tipo |
|---:|---|
| 1 | Mensaje |
| 2 | Enlace |
| 3 | Teléfono |
| 4 | Descripción de llamada |

Ejemplo:

```json
{
  "tipoContenido": 1,
  "contenido": "Texto a analizar"
}
```

## Respuesta correcta

HTTP `200`.

La respuesta incluye:

- identificador del análisis;
- nivel de riesgo;
- puntaje;
- resumen;
- señales detectadas;
- recomendaciones.

## Autenticación

Cada cliente utiliza una API key. La clave completa no se almacena en SQL Server: se conserva su hash para validación.

Las claves pueden revocarse desde el dashboard administrativo.

## Planes

### Prueba

- duración: 14 días desde la creación del cliente;
- cuota diaria configurable;
- al vencer devuelve HTTP `403` con código `prueba_expirada`.

### Comercial

- sin vencimiento automático de prueba;
- cuota diaria configurable;
- puede habilitarse o deshabilitarse desde administración.

El paso de `Prueba` a `Comercial` se realiza desde el dashboard interno.

## Cuotas

El consumo se contabiliza por cliente y día UTC.

Cuando la cuota se agota, la API rechaza nuevas solicitudes hasta el siguiente período diario.

El control de consumo utiliza persistencia en SQL Server y protección transaccional para evitar exceder la cuota ante solicitudes concurrentes.

## Administración

Ruta:

```text
/administracion/api
```

Desde el dashboard se puede:

- consultar clientes;
- ver consumo;
- habilitar o deshabilitar clientes;
- cambiar plan;
- modificar cuota diaria;
- revocar API keys.

El secreto administrativo debe mantenerse fuera del repositorio mediante secretos de desarrollo, variables de entorno o un gestor de secretos.

## Códigos relevantes

| HTTP | Significado |
|---:|---|
| 200 | Análisis realizado |
| 400 | Solicitud inválida |
| 401 | API key inválida o ausente |
| 403 | Cliente no autorizado o prueba vencida |
| 429 | Rate limit o cuota agotada |

## Producción

Requisitos mínimos:

- HTTPS;
- SQL Server persistente;
- secretos fuera de archivos versionados;
- configuración administrativa segura;
- monitoreo de errores y consumo;
- backup de base de datos.
