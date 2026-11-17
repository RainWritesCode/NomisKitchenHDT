#Requires -Version 5.1
# Installs only the Nomi's Kitchen BepInEx mod "Nomi Can't Dance" into Hearthstone.
# Does not need Hearthstone Deck Tracker. BepInEx 5 must already be installed in the Hearthstone folder.
[CmdletBinding()]
param(
    [string]$HearthstoneDir = 'C:\Program Files (x86)\Hearthstone',
    [switch]$Uninstall
)

$ErrorActionPreference = 'Stop'
$dllName = 'com.community.hs.NomiCantDance.dll'
$src = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\Resources\$dllName"
$pluginsDir = Join-Path $HearthstoneDir 'BepInEx\plugins'
$target = Join-Path $pluginsDir $dllName

if (-not (Test-Path $pluginsDir)) {
    throw "BepInEx plugins folder not found: $pluginsDir. Install BepInEx 5 into the Hearthstone folder first."
}
if (Get-Process -Name Hearthstone -ErrorAction SilentlyContinue) {
    throw 'Hearthstone is running. Close it and run this again.'
}

if ($Uninstall) {
    if (Test-Path $target) { Remove-Item $target -Force }
    Write-Host "Removed $target"
    return
}

if (-not (Test-Path $src)) { throw "Missing $src. Extract the whole repo before running." }
Copy-Item $src $target -Force
Write-Host "Installed $target"
Write-Host "Start Hearthstone once so BepInEx creates the config, then open the settings window."
