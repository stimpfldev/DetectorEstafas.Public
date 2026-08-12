# Detector de Estafas

AplicaciÃ³n preventiva desarrollada con ASP.NET Core MVC y SQL Server para detectar seÃ±ales habituales de fraude en mensajes, enlaces, telÃ©fonos, llamadas, capturas y audios.

**VersiÃ³n estable actual: 2.0.0.** La segunda etapa funcional estÃ¡ cerrada y validada.

El repositorio publica cÃ³digo fuente para revisiÃ³n tÃ©cnica bajo una licencia propietaria de cÃ³digo visible. La ejecuciÃ³n productiva, explotaciÃ³n comercial, redistribuciÃ³n y publicaciÃ³n de derivados requieren autorizaciÃ³n escrita.

## Funcionalidades

- Motor local de reglas con nivel y puntaje de riesgo.
- AnÃ¡lisis de mensajes, enlaces, telÃ©fonos y descripciones de llamadas.
- AnÃ¡lisis tÃ©cnico de enlaces y consulta RDAP para dominios `.ar`.
- Carga segura de capturas, OCR local y anÃ¡lisis del texto extraÃ­do.
- Carga y grabaciÃ³n de audio, transcripciÃ³n local y anÃ¡lisis.
- EvaluaciÃ³n opcional y separada mediante proveedor de IA externo.
- Registro de cuentas y confirmación de correo.
- ConfirmaciÃ³n de correo preparada para desarrollo y SMTP.
- CatÃ¡logo de telÃ©fonos oficiales y fuentes pÃºblicas verificadas.
- PWA responsive con recepciÃ³n de contenido compartido.
- API comercial versionada con API keys, cuotas y consumo diario.
- Dashboard interno para administraciÃ³n de clientes API.
- MÃ©tricas, feedback y reportes comunitarios sin guardar el contenido analizado.

## Privacidad por diseÃ±o

Por defecto no se almacenan mensajes, enlaces completos, telÃ©fonos, imÃ¡genes, audios ni transcripciones. Los archivos se procesan temporalmente y se eliminan al finalizar. La evaluaciÃ³n con IA externa es opcional y requiere consentimiento explÃ­cito.

No ingreses contraseÃ±as, cÃ³digos de autenticaciÃ³n, datos bancarios, documentos ni informaciÃ³n personal innecesaria.

## Arquitectura

```text
DetectorEstafas.slnx
â”œâ”€â”€ DetectorEstafas.Web      ASP.NET Core MVC, EF Core y SQL Server
â””â”€â”€ DetectorEstafas.Tests    MSTest y pruebas con dependencias simuladas
```

Flujo principal:

```text
Entrada del usuario
â†’ validaciÃ³n y normalizaciÃ³n
â†’ OCR o transcripciÃ³n cuando corresponde
â†’ motor local determinista
â†’ evaluaciÃ³n opcional de IA
â†’ resultado separado y explicable
â†’ mÃ©trica sin contenido
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

Los modelos no se incluyen en el repositorio pÃºblico por tamaÃ±o y licencia de distribuciÃ³n. Deben descargarse desde sus fuentes oficiales mediante los scripts.

3. Configurar la cadena de conexiÃ³n en un archivo local no versionado o mediante variables de entorno.

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

## ConfiguraciÃ³n sensible

No guardar secretos en `appsettings.json`, commits, capturas ni documentaciÃ³n. Usar `dotnet user-secrets` durante desarrollo y variables de entorno o un gestor de secretos en producciÃ³n.

Ejemplo:

```powershell
dotnet user-secrets init --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
dotnet user-secrets set "InteligenciaArtificial:ApiKey" "REEMPLAZAR" --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
dotnet user-secrets set "ApiAdministracion:Secret" "REEMPLAZAR" --project .\DetectorEstafas.Web\DetectorEstafas.Web.csproj
```

La configuraciÃ³n pÃºblica de referencia estÃ¡ en:

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

Las claves completas no se almacenan en SQL Server; se conserva su hash para validaciÃ³n.

## Seguridad

Antes de reportar una vulnerabilidad, leer [SECURITY.md](SECURITY.md). No publiques secretos, datos personales ni detalles explotables en un issue pÃºblico.

## ValidaciÃ³n previa a publicaciÃ³n

Antes de publicar una nueva versiÃ³n debe ejecutarse:

```powershell
powershell -ExecutionPolicy Bypass -File .\DetectorEstafas.Web\Scripts\Preparar-Repositorio-Publico.ps1
powershell -ExecutionPolicy Bypass -File .\DetectorEstafas.Web\Scripts\Verificar-Repositorio-Publico.ps1
```

TambiÃ©n deben completarse [docs/PUBLICATION-CHECKLIST.md](docs/PUBLICATION-CHECKLIST.md) y [docs/RELEASE-CHECKLIST-2.0.0.md](docs/RELEASE-CHECKLIST-2.0.0.md).


## Licencia

Este repositorio usa una licencia propietaria de cÃ³digo visible. El cÃ³digo puede revisarse y evaluarse, pero no puede explotarse comercialmente, redistribuirse ni utilizarse en producciÃ³n sin autorizaciÃ³n escrita. Ver [LICENSE](LICENSE).

Los componentes de terceros conservan sus propias licencias. Ver `DetectorEstafas.Web/Legal/THIRD-PARTY-NOTICES.txt`.

## VersiÃ³n y cambios

Ver [RELEASE-NOTES.md](RELEASE-NOTES.md) para el alcance cerrado de la versiÃ³n 2.0.0.

## Autor

Federico Stimpfl  
Desarrollador Backend .NET  
https://www.federicostimpfl.com.ar
