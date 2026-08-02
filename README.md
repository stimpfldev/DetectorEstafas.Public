# Detector de Estafas

Aplicación preventiva desarrollada con ASP.NET Core MVC y SQL Server para detectar señales habituales de fraude en mensajes, enlaces, teléfonos, llamadas, capturas y audios.

**Versión estable actual: 2.0.0.** La segunda etapa funcional está cerrada y validada.

El repositorio publica código fuente para revisión técnica bajo una licencia propietaria de código visible. La ejecución productiva, explotación comercial, redistribución y publicación de derivados requieren autorización escrita.

## Funcionalidades

- Motor local de reglas con nivel y puntaje de riesgo.
- Análisis de mensajes, enlaces, teléfonos y descripciones de llamadas.
- Análisis técnico de enlaces y consulta RDAP para dominios `.ar`.
- Carga segura de capturas, OCR local y análisis del texto extraído.
- Carga y grabación de audio, transcripción local y análisis.
- Evaluación opcional y separada mediante proveedor de IA externo.
- Registro de cuentas sin inicio de sesión habilitado por el momento.
- Confirmación de correo preparada para desarrollo y SMTP.
- Catálogo de teléfonos oficiales y fuentes públicas verificadas.
- PWA responsive con recepción de contenido compartido.
- API comercial versionada con API keys, cuotas y consumo diario.
- Dashboard interno para administración de clientes API.
- Métricas, feedback y reportes comunitarios sin guardar el contenido analizado.

## Privacidad por diseño

Por defecto no se almacenan mensajes, enlaces completos, teléfonos, imágenes, audios ni transcripciones. Los archivos se procesan temporalmente y se eliminan al finalizar. La evaluación con IA externa es opcional y requiere consentimiento explícito.

No ingreses contraseñas, códigos de autenticación, datos bancarios, documentos ni información personal innecesaria.

## Arquitectura

```text
DetectorEstafas.slnx
├── DetectorEstafas.Web      ASP.NET Core MVC, EF Core y SQL Server
└── DetectorEstafas.Tests    MSTest y pruebas con dependencias simuladas
```

Flujo principal:

```text
Entrada del usuario
→ validación y normalización
→ OCR o transcripción cuando corresponde
→ motor local determinista
→ evaluación opcional de IA
→ resultado separado y explicable
→ métrica sin contenido
```

## Requisitos de desarrollo

- .NET 10 SDK.
- SQL Server LocalDB o SQL Server.
- Visual Studio compatible con .NET 10 o CLI de .NET.
- PowerShell para preparar los modelos locales.

## Puesta en marcha local

1. Restaurar paquetes:

```powershell
dotnet restore .\DetectorEstafas.slnx
```

2. Preparar OCR y Whisper desde `DetectorEstafas.Web`:

```powershell
powershell -ExecutionPolicy Bypass -File .\Scripts\Preparar-OCR.ps1
powershell -ExecutionPolicy Bypass -File .\Scripts\Preparar-Whisper.ps1
```

Los modelos no se incluyen en el repositorio público por tamaño y licencia de distribución. Deben descargarse desde sus fuentes oficiales mediante los scripts.

3. Configurar la cadena de conexión en un archivo local no versionado o mediante variables de entorno.

4. Aplicar migraciones:

```powershell
dotnet ef database update `
  --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj `
  --startup-project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
```

5. Ejecutar:

```powershell
dotnet run --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
```

6. Ejecutar pruebas:

```powershell
dotnet test .\DetectorEstafas.slnx --configuration Release
```

## Configuración sensible

No guardar secretos en `appsettings.json`, commits, capturas ni documentación. Usar `dotnet user-secrets` durante desarrollo y variables de entorno o un gestor de secretos en producción.

Ejemplo:

```powershell
dotnet user-secrets init --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
dotnet user-secrets set "InteligenciaArtificial:ApiKey" "REEMPLAZAR" --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
dotnet user-secrets set "ApiAdministracion:Secret" "REEMPLAZAR" --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
```

La configuración pública de referencia está en:

```text
DetectorEstafas.Web/appsettings.Public.example.json
```

## API comercial

Documento OpenAPI local:

```text
/openapi/v1.json
```

Endpoint principal:

```text
POST /api/v1/analisis
Header: X-Api-Key
```

Las claves completas no se almacenan en SQL Server; se conserva su hash para validación.

## Seguridad

Antes de reportar una vulnerabilidad, leer [SECURITY.md](SECURITY.md). No publiques secretos, datos personales ni detalles explotables en un issue público.

## Validación previa a publicación

Antes de publicar una nueva versión debe ejecutarse:

```powershell
powershell -ExecutionPolicy Bypass -File .\DetectorEstafas.Web\Scripts\Preparar-Repositorio-Publico.ps1
powershell -ExecutionPolicy Bypass -File .\DetectorEstafas.Web\Scripts\Verificar-Repositorio-Publico.ps1
```

También deben completarse [docs/PUBLICATION-CHECKLIST.md](docs/PUBLICATION-CHECKLIST.md) y [docs/RELEASE-CHECKLIST-2.0.0.md](docs/RELEASE-CHECKLIST-2.0.0.md).

Las funciones pospuestas para una etapa posterior son la extensión de navegador, la aplicación móvil nativa, el inicio de sesión público y la arquitectura multiempresa completa.

## Licencia

Este repositorio usa una licencia propietaria de código visible. El código puede revisarse y evaluarse, pero no puede explotarse comercialmente, redistribuirse ni utilizarse en producción sin autorización escrita. Ver [LICENSE](LICENSE).

Los componentes de terceros conservan sus propias licencias. Ver `DetectorEstafas.Web/Legal/THIRD-PARTY-NOTICES.txt`.

## Versión y cambios

Ver [RELEASE-NOTES.md](RELEASE-NOTES.md) para el alcance cerrado de la versión 2.0.0.

## Autor

Federico Stimpfl  
Desarrollador Backend .NET  
https://www.federicostimpfl.com.ar
