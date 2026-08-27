$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..\..")
Set-Location $root

$findings = @()

$patterns = @(
    @{ Name = "OpenAI API key"; Regex = "sk-[A-Za-z0-9_-]{20,}" },
    @{ Name = "Private key"; Regex = "-----BEGIN (RSA |EC |OPENSSH )?PRIVATE KEY-----" },
    @{ Name = "Configured secret"; Regex = '"(ApiKey|Secret|Password)"\s*:\s*"(?!(REEMPLAZAR|UNA-CLAVE|<CONFIGURAR_EXTERNAMENTE)[^"]*)[^"\r\n]+"' },
    @{ Name = "Connection password"; Regex = "(?i)(Password|Pwd)\s*=\s*[^;\s]+" }
)

$excludedExtensions = @(
    ".dll", ".exe", ".pdb", ".bin", ".traineddata",
    ".ico", ".png", ".jpg", ".jpeg"
)

# bin, obj, .vs and artifacts are generated locally and are ignored here.
# Local runtime assets such as *.user, OCR and Whisper models are allowed
# when ignored by Git. The tracked-file check below rejects them if committed.
$sourceFiles = Get-ChildItem -Path . -Recurse -File | Where-Object {
    $_.FullName -notmatch "\\(bin|obj|\.vs|artifacts|\.git)\\" -and
    $excludedExtensions -notcontains $_.Extension.ToLowerInvariant()
}

foreach ($file in $sourceFiles) {
    $content = Get-Content -LiteralPath $file.FullName -Raw -ErrorAction SilentlyContinue

    if ($null -eq $content) {
        continue
    }

    foreach ($pattern in $patterns) {
        if ($content -match $pattern.Regex) {
            $findings += "Possible $($pattern.Name): $($file.FullName)"
        }
    }
}

$gitCommand = Get-Command git -ErrorAction SilentlyContinue

if ($null -ne $gitCommand -and (Test-Path ".git")) {
    $trackedFiles = git ls-files

    foreach ($entry in $trackedFiles) {
        if ($entry -match "(?i)(^|/)(bin|obj|\.vs|artifacts)/") {
            $findings += "Generated file tracked by Git: $entry"
        }
        elseif ($entry -match "(?i)(request\.json|secrets\.json|\.user$|\.suo$|\.pfx$|\.p12$|\.pem$|\.key$|OcrData/.+\.traineddata$|WhisperModels/.+\.bin$|wwwroot/.+\.(zip|7z|rar)$)") {
            $findings += "Sensitive tracked file: $entry"
        }
    }
}

$uniqueFindings = @($findings | Sort-Object -Unique)

if ($uniqueFindings.Count -gt 0) {
    Write-Host "PUBLIC REPOSITORY CHECK FAILED" -ForegroundColor Red

    foreach ($finding in $uniqueFindings) {
        Write-Host "- $finding" -ForegroundColor Red
    }

    exit 1
}

Write-Host "PUBLIC REPOSITORY CHECK PASSED" -ForegroundColor Green
Write-Host "No secrets or forbidden tracked files were detected by the current rules."
Write-Host "Review Git history manually and rotate any secret that was ever committed."
exit 0
