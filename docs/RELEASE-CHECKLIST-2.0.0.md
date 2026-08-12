# Checklist de release 2.0.0

## Antes del commit final

- [ ] Ejecutar `Validar-Seguridad-Etapa2.ps1`.
- [ ] Confirmar 37/37 pruebas aprobadas.
- [ ] Confirmar que `VERSION.txt`, `.csproj`, README y release notes muestran 2.0.0.
- [ ] Revisar `git status` y `git diff`.
- [ ] Confirmar que no se publican modelos, binarios, secretos ni notas internas `LEEME-BLOQUE-*`.

## Resguardo

- [ ] Crear copia privada cifrada de la soluciÃ³n completa.
- [ ] Conservar respaldo de base y migraciones.
- [ ] Guardar secretos en un gestor separado.
- [ ] Rotar cualquier secreto que haya aparecido en el historial.

## Release

- [ ] Ejecutar `Crear-Release-2.0.0.ps1`.
- [ ] Verificar el contenido de `artifacts/DetectorEstafas-2.0.0`.
- [ ] Ejecutar la publicaciÃ³n en una base de validaciÃ³n limpia.
- [ ] Confirmar ZIP y archivo `.sha256`.

## Git

```powershell
git status
git add .
git commit -m "Release Detector de Estafas 2.0.0"
git tag -a v2.0.0 -m "Detector de Estafas 2.0.0"
git show v2.0.0 --stat
```

Publicar el commit y el tag Ãºnicamente despuÃ©s de revisar el repositorio remoto y su visibilidad.

## GitHub

- [ ] Repositorio bajo la visibilidad decidida.
- [ ] DescripciÃ³n y temas correctos.
- [ ] Rama principal protegida cuando corresponda.
- [ ] Secret scanning y Dependabot habilitados si estÃ¡n disponibles.
- [ ] Release de GitHub creada desde `v2.0.0`.
- [ ] Adjuntar ZIP y SHA-256 solo si se decidiÃ³ distribuir binarios.
- [ ] No incluir base de datos, secretos ni datos reales.
