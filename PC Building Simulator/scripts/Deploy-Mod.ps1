# Deploy-Mod.ps1 — Copy already-built DLL to BepInEx\plugins\ without rebuilding.
# Use this to re-deploy after a build that succeeded.
# Run from game directory: .\scripts\Deploy-Mod.ps1

$ErrorActionPreference = "Stop"

$gameDir  = Split-Path -Parent $PSScriptRoot
$dllSrc   = Join-Path $gameDir "PCBSAccess\bin\Release\net472\PCBSAccess.dll"
$pluginDir = Join-Path $gameDir "BepInEx\plugins"

if (-not (Test-Path $dllSrc)) {
    Write-Host "DLL not found: $dllSrc" -ForegroundColor Red
    Write-Host "Run Build-Mod.ps1 first." -ForegroundColor Yellow
    exit 1
}

Copy-Item $dllSrc $pluginDir -Force
Write-Host "Deployed PCBSAccess.dll to BepInEx\plugins\" -ForegroundColor Green
