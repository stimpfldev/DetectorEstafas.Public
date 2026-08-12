# Resguardo y recuperación

## Objetivo

El proyecto mantiene separados:

1. código fuente versionado;
2. release ejecutable;
3. base de datos;
4. secretos;
5. modelos locales OCR y Whisper.

Un backup correcto no debe depender de una sola ubicación.

## Código fuente

La rama estable del repositorio público contiene el código fuente publicable de la versión vigente.

Debe existir además un repositorio privado de respaldo sincronizado con el último estado estable.

Los archivos locales ignorados por Git no forman parte de este respaldo.

## Release ejecutable

Cada versión publicada debe conservar:

```text
DetectorEstafas-<versión>.zip
DetectorEstafas-<versión>.zip.sha256
```

El SHA-256 permite comprobar que el paquete no fue alterado.

## Base de datos

La base SQL Server debe respaldarse por separado mediante el mecanismo disponible en el entorno de producción.

El repositorio de código no sustituye un backup de base de datos.

## Secretos

No guardar en Git:

- cadenas de conexión con credenciales;
- claves de IA;
- secretos administrativos;
- contraseñas SMTP;
- API keys reales;
- archivos `.user`, `.env`, `secrets.json` o equivalentes.

En desarrollo utilizar User Secrets. En producción utilizar variables de entorno o un gestor de secretos.

## Modelos locales

Los siguientes archivos se reconstruyen desde sus scripts y no deben versionarse:

```text
OcrData/spa.traineddata
WhisperModels/ggml-base.bin
```

Preparación:

```powershell
powershell -ExecutionPolicy Bypass -File .\DetectorEstafas.Web\Scripts\Preparar-OCR.ps1
powershell -ExecutionPolicy Bypass -File .\DetectorEstafas.Web\Scripts\Preparar-Whisper.ps1
```

## Recuperación mínima

Para recuperar el proyecto desde cero:

1. clonar el repositorio estable;
2. restaurar paquetes NuGet;
3. preparar OCR y Whisper;
4. restaurar o crear SQL Server;
5. aplicar migraciones;
6. restaurar secretos fuera de Git;
7. ejecutar build y tests;
8. iniciar la aplicación;
9. validar API, dashboard y funciones principales.

## Regla de cierre

Después de cada release estable:

1. publicar código y tag;
2. comprobar CI;
3. publicar ZIP y SHA-256;
4. sincronizar el repositorio privado de backup;
5. confirmar que el backup apunta al mismo estado estable.
