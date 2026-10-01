param([switch]$SkipInstall)

$ErrorActionPreference = 'Stop'
$calculatorSource = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../third_party/mhwilds-calculator'))
$archivePath = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../MHWsTool/Data/calculator.zip'))
Push-Location $calculatorSource
try {
    $env:NEXT_TELEMETRY_DISABLED = '1'
    if (-not $SkipInstall) {
        & npm.cmd ci --no-audit --no-fund
        if ($LASTEXITCODE -ne 0) { throw 'Calculator dependency installation failed.' }
    }
    & npm.cmd run build
    if ($LASTEXITCODE -ne 0) { throw 'Calculator static export failed.' }
    Copy-Item -LiteralPath 'LICENSE.md' -Destination 'out/LICENSE.md' -Force
    # ZipFile preserves _next (hidden files are omitted by Compress-Archive).
    $temporaryArchive = "$archivePath.tmp"
    if (Test-Path -LiteralPath $temporaryArchive) { Remove-Item -LiteralPath $temporaryArchive }
    [IO.Compression.ZipFile]::CreateFromDirectory((Join-Path $calculatorSource 'out'), $temporaryArchive)
    Move-Item -LiteralPath $temporaryArchive -Destination $archivePath -Force
    Write-Host "Bundled offline calculator: $archivePath"
}
finally { Pop-Location }
