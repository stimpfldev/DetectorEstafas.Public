# Checklist de release 2.2.0

## Código y validación

- [x] Versión del proyecto actualizada a 2.2.0.
- [x] `VERSION.txt` actualizado.
- [x] `RELEASE-NOTES.md` actualizado.
- [x] Refresh posterior a análisis de captura corregido y validado.
- [x] Autenticación de API comercial validada.
- [x] Headers de rate limit validados.
- [x] Dashboard administrativo validado.
- [x] Canal de contacto validado.
- [x] Validaciones funcionales principales completadas.
- [ ] Prueba física en iPhone/iPad.
- [ ] Compilación final Release 2.2.0 aprobada.
- [ ] Suite automatizada final aprobada.
- [ ] GitHub Actions `CI / Build and test` aprobado para 2.2.0.

## Seguridad y privacidad

- [x] Secretos excluidos del repositorio público.
- [x] User Secrets utilizados para configuración local sensible.
- [x] API keys no publicadas en archivos de configuración.
- [x] Contraseñas SMTP no publicadas.
- [x] Assets locales pesados excluidos del repositorio.
- [x] Configuración pública de ejemplo preparada.
- [x] Revisión del repositorio sin referencias privadas de desarrollo.
- [ ] Revisión final de secretos antes del commit de release.

## PWA y dispositivos

- [x] Manifest configurado.
- [x] Service Worker configurado.
- [x] Íconos PWA incluidos.
- [x] Instalación y uso en Android validados.
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
- [ ] Dominio configurado.
- [ ] HTTPS validado.
- [ ] SMTP productivo configurado.
- [ ] Secretos productivos configurados fuera del repositorio.
- [ ] `AllowedHosts` productivo configurado.
- [ ] Smoke test sobre producción.
- [ ] Tag `v2.2.0` creado.
- [ ] Release GitHub `v2.2.0` publicado.
- [ ] Artefactos finales generados y publicados.
