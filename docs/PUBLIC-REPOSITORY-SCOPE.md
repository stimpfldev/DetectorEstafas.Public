# Alcance recomendado del repositorio público

## Publicar

- aplicación web y pruebas;
- motor local de análisis;
- validación segura de archivos;
- interfaces de OCR, audio e IA;
- integración con proveedores sin claves;
- migraciones y modelo de datos;
- PWA;
- API versionada;
- documentación técnica y legal;
- configuraciones de ejemplo.

## No publicar

- `user-secrets`;
- claves de IA, SMTP, API o administración;
- bases de datos o respaldos;
- datos reales de usuarios o clientes;
- modelos binarios grandes;
- artefactos compilados;
- archivos de publicación o certificados;
- capturas con información personal;
- scripts internos con credenciales;
- documentos comerciales, precios negociados o información contractual;
- repositorios o herramientas privadas futuras de emisión y gestión de claves.

## Recomendación de historial

La primera publicación pública puede realizarse desde una copia limpia con un único commit inicial, preservando el repositorio privado completo como fuente de verdad. Esto evita exponer archivos eliminados, pruebas descartadas, secretos rotados o decisiones internas presentes en el historial anterior.
