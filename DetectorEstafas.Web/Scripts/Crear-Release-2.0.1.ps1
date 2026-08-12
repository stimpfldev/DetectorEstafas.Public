$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

$version = "2.0.1"

$solutionPath = Join-Path $root "DetectorEstafas.slnx"
$projectPath = Join-Path $root "DetectorEstafas.Web\DetectorEstafas.Web.csproj"

$artifactsRoot = Join-Path $root "artifacts"
$publishPath = Join-Path $artifactsRoot "DetectorEstafas-$version"
$zipPath = Join-Path $artifactsRoot "DetectorEstafas-$version.zip"
$hashPath = "$zipPath.sha256"

$ocrModel = Join-Path $root "DetectorEstafas.Web\OcrData\spa.traineddata"
$whisperModel = Join-Path $root "DetectorEstafas.Web\WhisperModels\ggml-base.bin"

Write-Host "1/6 - Verificando modelos locales..."

if (-not (Test-Path $ocrModel)) {
    throw "Falta el modelo OCR: $ocrModel"
}

if (-not (Test-Path $whisperModel)) {
    throw "Falta el modelo Whisper: $whisperModel"
}

Write-Host "2/6 - Restaurando dependencias..."

dotnet restore $solutionPath

if ($LASTEXITCODE -ne 0) {
    throw "Fallo la restauracion de dependencias."
}

Write-Host "3/6 - Compilando Release..."

dotnet build $solutionPath `
    --configuration Release `
    --no-restore

if ($LASTEXITCODE -ne 0) {
    throw "Fallo la compilacion Release."
}

Write-Host "4/6 - Ejecutando pruebas..."

dotnet test $solutionPath `
    --configuration Release `
    --no-build

if ($LASTEXITCODE -ne 0) {
    throw "Fallaron las pruebas."
}

Write-Host "5/6 - Publicando aplicacion..."

if (Test-Path $publishPath) {
    Remove-Item $publishPath -Recurse -Force
}

if (Test-Path $zipPath) {
    Remove-Item $zipPath -Force
}

if (Test-Path $hashPath) {
    Remove-Item $hashPath -Force
}

New-Item `
    -ItemType Directory `
    -Path $artifactsRoot `
    -Force | Out-Null

dotnet publish $projectPath `
    --configuration Release `
    --no-build `
    --output $publishPath

if ($LASTEXITCODE -ne 0) {
    throw "Fallo la publicacion Release."
}

Write-Host "6/6 - Generando ZIP y SHA-256..."

Compress-Archive `
    -Path (Join-Path $publishPath "*") `
    -DestinationPath $zipPath `
    -CompressionLevel Optimal `
    -Force

$hash = Get-FileHash `
    -Path $zipPath `
    -Algorithm SHA256

"$($hash.Hash)  DetectorEstafas-$version.zip" |
    Set-Content `
        -Path $hashPath `
        -Encoding ASCII

Write-Host ""
Write-Host "RELEASE 2.0.1 CREATED" -ForegroundColor Green
Write-Host "Carpeta: $publishPath"
Write-Host "ZIP: $zipPath"
Write-Host "SHA-256: $hashPath"