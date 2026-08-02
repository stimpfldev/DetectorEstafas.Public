# Checklist de publicación pública

## 1. Resguardo

- [ ] Crear commit estable privado.
- [ ] Crear tag interno previo a publicación.
- [ ] Guardar una copia cifrada del ZIP completo.
- [ ] Confirmar que la base de datos y los secretos no estén incluidos.

## 2. Código y archivos

- [ ] Ejecutar `DetectorEstafas.Web/Scripts/Preparar-Repositorio-Publico.ps1`.
- [ ] Ejecutar `DetectorEstafas.Web/Scripts/Verificar-Repositorio-Publico.ps1`.
- [ ] Confirmar que no existan `bin`, `obj`, `.vs`, `artifacts` ni publicaciones.
- [ ] Confirmar que no existan archivos `.user`, `.pfx`, `.pubxml`, `.env` o `secrets.json`.
- [ ] Confirmar que no existan modelos `*.traineddata` o `*.bin`.
- [ ] Eliminar ZIP accidentales dentro de `wwwroot`.
- [ ] Eliminar `request.json` y archivos de prueba manual.

## 3. Secretos e historial

- [ ] Buscar claves OpenAI y otros tokens.
- [ ] Buscar secretos administrativos y API keys.
- [ ] Buscar contraseñas SMTP.
- [ ] Buscar cadenas de conexión con usuario o contraseña.
- [ ] Revisar el historial completo, no solo el árbol actual.
- [ ] Rotar cualquier secreto que haya sido versionado alguna vez.
- [ ] No confiar solamente en borrar el archivo del último commit.

## 4. Legal

- [ ] Revisar `LICENSE`.
- [ ] Revisar copyright y nombre del titular.
- [ ] Revisar `THIRD-PARTY-NOTICES.txt` contra los paquetes distribuidos.
- [ ] Confirmar las condiciones de distribución de modelos OCR y Whisper.
- [ ] Revisar política de privacidad y condiciones de uso.
- [ ] Definir canal de contacto público.
- [ ] Obtener revisión jurídica antes de explotación comercial.
- [ ] Evaluar depósito de software y registro de marca por separado.

## 5. Seguridad

- [ ] Todas las pruebas pasan en Release.
- [ ] Carga inválida de imágenes y audios es rechazada.
- [ ] Los temporales se eliminan.
- [ ] No se registra contenido sensible en logs.
- [ ] La IA falla de manera segura y conserva el resultado local.
- [ ] API sin clave devuelve 401.
- [ ] Cliente deshabilitado o clave revocada devuelve 401.
- [ ] Cuota agotada devuelve 429.
- [ ] Dashboard administrativo requiere secreto.
- [ ] No se muestra ninguna API key completa.

## 6. Presentación

- [ ] README coincide con la funcionalidad real.
- [ ] No se anuncian funciones pendientes.
- [ ] Capturas sin datos personales.
- [ ] Demo sin claves reales.
- [ ] Repositorio público sin historial privado innecesario.
- [ ] La versión visible coincide con `VERSION.txt` y el proyecto.
