# Checklist de release 2.2.0

## Código y validación

- [x] Versión del proyecto actualizada a 2.2.0.
- [x] `VERSION.txt` actualizado.
- [x] `RELEASE-NOTES.md` actualizado.
- [x] Soporte MP3 y WAV validado.
- [x] Soporte AAC, OGG y M4A validado en Android.
- [x] Normalización de audio mediante FFmpeg validada.
- [x] Refresh posterior a análisis de audio corregido y validado.
- [x] Refresh posterior a análisis de captura corregido y validado.
- [x] Autenticación de API comercial validada.
- [x] Headers de rate limit validados.
- [x] Dashboard administrativo validado.
- [x] Canal de contacto validado.
- [x] 41 de 42 validaciones funcionales completadas.
- [ ] Prueba física en iPhone/iPad.
- [ ] Compilación final Release 2.2.0 aprobada.
- [ ] Suite automatizada final aprobada.
- [ ] GitHub Actions `CI / Build and test` aprobado para 2.2.0.

## Seguridad y privacidad

- [x] Secretos excluidos del repositorio público.
- [x] User Secrets utilizados para configuración local sensible.
- [x] API keys no publicadas en archivos de configuración.
- [x] Contraseñas SMTP no publicadas.
- [x] Modelos OCR/Whisper excluidos del repositorio.
- [x] Ruta local de FFmpeg eliminada de la configuración versionada.
- [x] Configuración pública de ejemplo preparada.
- [x] Revisión del repositorio sin referencias privadas de desarrollo.
- [ ] Revisión final de secretos antes del commit de release.

## Audio y FFmpeg

- [x] FFMpegCore 5.4.0 incorporado.
- [x] FFmpeg y ffprobe utilizados como dependencias externas.
- [x] README documenta el requisito de FFmpeg.
- [x] `appsettings.Public.example.json` incluye `FFmpeg:BinaryFolder`.
- [x] FFmpeg no se incluye como binario en el repositorio.
- [x] Licencia de la build de desarrollo verificada.
- [x] `THIRD-PARTY-NOTICES.txt` actualizado con FFMpegCore y FFmpeg.

## PWA y dispositivos

- [x] Manifest configurado.
- [x] Service Worker configurado.
- [x] Íconos PWA incluidos.
- [x] Instalación y uso en Android validados.
- [x] Selector de audio compatible con Android validado.
- [x] Fallback de selector preparado para navegadores sin File System Access API.
- [x] Compatibilidad web/PWA para iPhone/iPad implementada.
- [ ] Validación física definitiva en iPhone/iPad.

La ausencia de la prueba física en iPhone/iPad queda documentada y no invalida las funcionalidades verificadas en las demás plataformas.

## Documentación

- [x] README actualizado.
- [x] `VERSION.txt` actualizado.
- [x] `RELEASE-NOTES.md` actualizado.
- [x] `THIRD-PARTY-NOTICES.txt` actualizado.
- [x] Configuración pública de ejemplo actualizada.
- [x] Guía de usuario existente.
- [x] Guía de API comercial existente.
- [x] Política de seguridad existente.
- [x] Procedimiento de backup y recuperación existente.
- [ ] Revisión final de consistencia de versión 2.2.0 en todo el repositorio.

## Publicación

Estos puntos se completarán durante la publicación productiva:

- [ ] Hosting configurado.
- [ ] Base de datos productiva configurada.
- [ ] `FFmpeg:BinaryFolder` configurado en producción.
- [ ] Dominio configurado.
- [ ] HTTPS validado.
- [ ] SMTP productivo configurado.
- [ ] Secretos productivos configurados fuera del repositorio.
- [ ] `AllowedHosts` productivo configurado.
- [ ] Smoke test sobre producción.
- [ ] Tag `v2.2.0` creado.
- [ ] Release GitHub `v2.2.0` publicado.
- [ ] Artefactos finales generados y publicados.