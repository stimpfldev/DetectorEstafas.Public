# Checklist de release 2.3.0

## Código y validación

- [x] Versión del proyecto actualizada a 2.3.0.
- [x] `VERSION.txt` actualizado.
- [x] `RELEASE-NOTES.md` actualizado.
- [x] `README.md` actualizado.
- [x] `COMMERCIAL-API.md` actualizado.
- [x] Plan Prueba configurado en 20 análisis por día durante 14 días.
- [x] Prevención de prueba duplicada validada por test.
- [x] Suscripción Starter aprobada validada end-to-end.
- [x] Cliente Starter provisionado y habilitado.
- [x] Entrega one-time de API key creada.
- [x] Webhook firmado aceptado con HTTP 200.
- [x] Idempotencia validada sin duplicar eventos, clientes ni entregas.
- [x] Pago rechazado → Impaga cubierto por regresión automatizada.
- [x] Período de gracia → Suspensión cubierto por regresión automatizada.
- [x] Cancelación y fin de acceso cubiertos por regresión automatizada.
- [x] Growth aprobado e idempotencia cubiertos por regresión automatizada.
- [x] Logging diagnóstico temporal retirado.
- [x] Build Release aprobado.
- [x] Suite automatizada final: 62/62 correctas, 0 errores, 0 omitidas.
- [ ] GitHub Actions `CI / Build and test` aprobado para 2.3.0.

## Seguridad

- [x] Secretos reales excluidos del repositorio público.
- [x] Mercado Pago Access Token fuera de archivos versionados.
- [x] Mercado Pago Webhook Secret fuera de archivos versionados.
- [x] User Secrets utilizados para configuración local sensible.
- [x] Contraseñas SMTP no publicadas.
- [x] API keys no publicadas.
- [x] OCR/Whisper locales ignorados por Git.
- [x] `.csproj.user` ignorado por Git.
- [x] Placeholders de producción diferenciados de secretos reales.
- [x] Revisión de dependencias vulnerables ejecutada.
- [x] `SECURITY VALIDATION PASSED`.

## Automatización comercial

- [x] Persistencia de suscripciones comerciales.
- [x] Persistencia de eventos de webhook.
- [x] Firma HMAC SHA-256 validada.
- [x] Webhooks idempotentes.
- [x] Pago aprobado activa la suscripción.
- [x] Pago rechazado inicia período de gracia.
- [x] Vencimiento de gracia suspende acceso.
- [x] Cancelación mantiene acceso hasta fecha final.
- [x] Fin de acceso cancelado deshabilita cliente.
- [x] Entrega temporal de API key de un solo uso.
- [x] Próxima renovación registrada cuando Mercado Pago la informa.

## Documentación

- [x] `README.md` actualizado a 2.3.0.
- [x] `VERSION.txt` actualizado a 2.3.0.
- [x] `RELEASE-NOTES.md` actualizado a 2.3.0.
- [x] `COMMERCIAL-API.md` actualizado con automatización y cuota Prueba de 20/día.
- [x] Configuración sensible documentada sin valores reales.
- [x] Publicación productiva diferenciada del cierre técnico.

## Cierre Git

- [ ] Pull request `feature/commercial-automation-2.3.0` → `main` creado.
- [ ] CI del pull request aprobado.
- [ ] Pull request revisado.
- [ ] Pull request mergeado a `main`.
- [ ] `main` con CI aprobado.
- [ ] `git status` local limpio después de sincronizar.

## Limitación conocida no bloqueante

- [ ] Validación física definitiva en iPhone/iPad.

La ausencia de la prueba física en iPhone/iPad está documentada y no invalida las funcionalidades verificadas en las demás plataformas.
