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

Cada cliente utiliza una API key.

La clave completa no se almacena en SQL Server: se conserva su hash para validación.

Las claves pueden revocarse desde el dashboard administrativo.

## Planes y cuotas

### Prueba

- duración: 14 días desde la creación del cliente;
- límite: 200 análisis por día;
- período de cuota: diario UTC;
- al vencer devuelve HTTP `403` con código `prueba_expirada`.

### Starter

- límite: 5.000 análisis por mes;
- período de cuota: mensual UTC;
- no tiene vencimiento automático de prueba.

### Growth

- límite: 25.000 análisis por mes;
- período de cuota: mensual UTC;
- no tiene vencimiento automático de prueba.

### A medida

- cuota mensual configurable desde administración;
- destinado a necesidades superiores o condiciones comerciales personalizadas.

## Períodos de consumo

El consumo continúa registrándose diariamente en SQL Server mediante `ApiConsumosDiarios`.

Para el plan `Prueba`, la cuota se calcula sobre el día UTC actual.

Para `Starter`, `Growth` y `A medida`, la aplicación suma los consumos correspondientes al período mensual UTC actual.

Si un plan mensual comienza después del primer día del mes, el primer período comienza en la fecha de inicio del plan y finaliza al comenzar el mes siguiente.

Los períodos mensuales siguientes se reinician el primer día de cada mes UTC.

## Agotamiento de cuota

Cuando una cuota se agota, la API devuelve HTTP `429`.

Códigos:

```text
cuota_diaria_agotada
cuota_mensual_agotada
```

El control de consumo utiliza persistencia en SQL Server y protección transaccional para evitar exceder la cuota ante solicitudes concurrentes.

## Headers de cuota

Las respuestas autenticadas incluyen información del período:

```text
X-RateLimit-Limit
X-RateLimit-Remaining
X-RateLimit-Period
X-RateLimit-Reset
```

`X-RateLimit-Period` indica:

```text
day
month
```

`X-RateLimit-Reset` contiene el instante UTC de reinicio del período.

## Administración

Ruta:

```text
/administracion/api
```

Desde el dashboard se puede:

- consultar clientes;
- ver consumo y cuota restante;
- habilitar o deshabilitar clientes;
- cambiar entre Prueba, Starter, Growth y A medida;
- establecer una cuota mensual personalizada para A medida;
- revocar API keys.

El secreto administrativo debe mantenerse fuera del repositorio mediante User Secrets, variables de entorno o un gestor de secretos.

## Códigos HTTP relevantes

| HTTP | Significado |
|---:|---|
| 200 | Análisis realizado |
| 400 | Solicitud inválida |
| 401 | API key inválida o ausente |
| 403 | Prueba vencida o cliente no autorizado |
| 429 | Rate limit o cuota agotada |

## Producción

Requisitos mínimos:

- HTTPS;
- SQL Server persistente;
- secretos fuera de archivos versionados;
- configuración administrativa segura;
- monitoreo de errores y consumo;
- backup de base de datos.
