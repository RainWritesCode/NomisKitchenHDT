#Requires -Version 5.1

$ErrorActionPreference = 'Stop'

$pluginName = 'NomisKitchenHDT'
$dllName = 'NomisKitchenHDT.dll'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$dllPath = Join-Path $scriptDir $dllName

if (-not (Test-Path $dllPath)) {
    Write-Host "Could not find $dllName next to this installer." -ForegroundColor Red
    Write-Host "Make sure you extracted the whole zip before running install.ps1." -ForegroundColor Red
    Read-Host "Press Enter to exit"
    exit 1
}

$pluginDir = Join-Path $env:APPDATA "HearthstoneDeckTracker\Plugins\$pluginName"
if (-not (Test-Path $pluginDir)) {
    New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
}

Copy-Item $dllPath $pluginDir -Force

Write-Host ""
Write-Host "Installed $pluginName to:" -ForegroundColor Green
Write-Host "  $pluginDir" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:"
Write-Host "  1. Restart Hearthstone Deck Tracker."
Write-Host "  2. Open Options -> Tracker -> Plugins."
Write-Host "  3. Check the box next to Nomi's Kitchen."
Write-Host "  4. Click Settings on Nomi's Kitchen to configure it."
Write-Host ""
Read-Host "Press Enter to exit"
