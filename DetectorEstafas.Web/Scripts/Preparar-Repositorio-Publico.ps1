$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

$pathsToRemove = @(
    ".vs",
    "artifacts",
    "request.json",
    "DetectorEstafas.Web\bin",
    "DetectorEstafas.Web\obj",
    "DetectorEstafas.Tests\bin",
    "DetectorEstafas.Tests\obj"
)

foreach ($path in $pathsToRemove) {
    if (Test-Path $path) {
        Remove-Item $path -Recurse -Force
        Write-Host "Eliminado: $path"
    }
}

Get-ChildItem -Path "DetectorEstafas.Web\wwwroot" -Recurse -File -Include *.zip,*.7z,*.rar -ErrorAction SilentlyContinue |
    ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "Eliminado paquete accidental: $($_.FullName)"
    }

Get-ChildItem -Path "DetectorEstafas.Web\OcrData" -File -Filter *.traineddata -ErrorAction SilentlyContinue |
    ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "Eliminado modelo OCR local: $($_.Name)"
    }

Get-ChildItem -Path "DetectorEstafas.Web\WhisperModels" -File -Filter *.bin -ErrorAction SilentlyContinue |
    ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "Eliminado modelo Whisper local: $($_.Name)"
    }

Get-ChildItem -Path . -Recurse -File -Include *.user,*.suo,*.pfx,*.p12,*.key,*.pem,*.pubxml.user -ErrorAction SilentlyContinue |
    ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "Eliminado archivo local/sensible: $($_.FullName)"
    }

Get-ChildItem -Path . -File -Include LEEME-BLOQUE-*.txt,LEEME-CORRECCION-*.txt -ErrorAction SilentlyContinue |
    ForEach-Object {
        Remove-Item $_.FullName -Force
        Write-Host "Eliminada nota interna de implementación: $($_.Name)"
    }

Write-Host "Preparación física terminada. Ejecutá Verificar-Repositorio-Publico.ps1 antes de publicar."
