# Standalone install (no HDT)

Installs only the **Nomi Can't Dance** BepInEx mod (minion dance fix). It runs inside Hearthstone and does not need Hearthstone Deck Tracker.

## Requirements
- Hearthstone with BepInEx 5 installed in the game folder.

## Install
```bash
pwsh -File standalone/install-standalone.ps1 -HearthstoneDir "C:\Program Files (x86)\Hearthstone"
```
Start Hearthstone once so BepInEx creates `BepInEx\config\com.community.hs.NomiCantDance.cfg`.

## Settings
```bash
pwsh -File standalone/NomiCantDanceSettings.ps1 -HearthstoneDir "C:\Program Files (x86)\Hearthstone"
```
Edits `Enabled` and `LogFixes`. Changes apply when Hearthstone next starts.

## Uninstall
```bash
pwsh -File standalone/install-standalone.ps1 -HearthstoneDir "C:\Program Files (x86)\Hearthstone" -Uninstall
```
