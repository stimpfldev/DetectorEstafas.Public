$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $PSScriptRoot
$ocrDirectory = Join-Path $projectRoot "OcrData"
$destination = Join-Path $ocrDirectory "spa.traineddata"
$url = "https://raw.githubusercontent.com/tesseract-ocr/tessdata_fast/main/spa.traineddata"

New-Item -ItemType Directory -Path $ocrDirectory -Force | Out-Null

Write-Host "Descargando modelo OCR oficial en español..."
Invoke-WebRequest -Uri $url -OutFile $destination -UseBasicParsing

if (-not (Test-Path $destination)) {
    throw "No se pudo crear spa.traineddata."
}

$size = (Get-Item $destination).Length
if ($size -lt 1000000) {
    Remove-Item $destination -Force
    throw "El archivo descargado no parece válido."
}

Write-Host "OCR preparado: $destination"
