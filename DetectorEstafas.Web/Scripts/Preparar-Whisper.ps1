$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$modelFolder = Join-Path $projectRoot "WhisperModels"
$modelPath = Join-Path $modelFolder "ggml-base.bin"
$modelUrl = "https://huggingface.co/ggerganov/whisper.cpp/resolve/main/ggml-base.bin"
New-Item -ItemType Directory -Force -Path $modelFolder | Out-Null
if (Test-Path $modelPath) { Write-Host "El modelo ya existe: $modelPath"; exit 0 }
Write-Host "Descargando modelo local Whisper base..."
Invoke-WebRequest -Uri $modelUrl -OutFile $modelPath
Write-Host "Modelo preparado: $modelPath"
