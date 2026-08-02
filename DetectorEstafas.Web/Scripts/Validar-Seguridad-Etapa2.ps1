$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

Write-Host "1/5 - Verificando archivos publicos..."
& powershell -ExecutionPolicy Bypass -File ".\DetectorEstafas.Web\Scripts\Verificar-Repositorio-Publico.ps1"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "2/5 - Restaurando dependencias..."
& dotnet restore ".\DetectorEstafas.slnx"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "3/5 - Revisando paquetes vulnerables..."
$vulnerable = & dotnet list ".\DetectorEstafas.slnx" package --vulnerable --include-transitive 2>&1
$vulnerable | Write-Host
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if ($vulnerable -match "has the following vulnerable packages" -or
    $vulnerable -match "tiene los siguientes paquetes vulnerables") {
    Write-Error "Se detectaron dependencias vulnerables. No continuar con la publicacion."
    exit 1
}

Write-Host "4/5 - Compilando Release..."
& dotnet build ".\DetectorEstafas.slnx" --configuration Release --no-restore
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "5/5 - Ejecutando pruebas..."
& dotnet test ".\DetectorEstafas.slnx" --configuration Release --no-build
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "SECURITY VALIDATION PASSED" -ForegroundColor Green
