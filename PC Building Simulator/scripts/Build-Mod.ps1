# Build-Mod.ps1 — Build PCBSAccess and copy DLL to BepInEx\plugins\
# Run from game directory: .\scripts\Build-Mod.ps1

$ErrorActionPreference = "Stop"

$gameDir = Split-Path -Parent $PSScriptRoot
$projDir = Join-Path $gameDir "PCBSAccess"

Write-Host "Building PCBSAccess..." -ForegroundColor Cyan

Push-Location $projDir
try {
    dotnet build --configuration Release
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build FAILED." -ForegroundColor Red
        exit 1
    }
    Write-Host "Build succeeded. DLL copied to BepInEx\plugins\." -ForegroundColor Green
}
finally {
    Pop-Location
}
