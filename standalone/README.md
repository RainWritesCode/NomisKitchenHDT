# Standalone app (no HDT)

`NomiKitchenStandalone.exe` installs and configures the Nomi's Kitchen BepInEx mods without Hearthstone Deck Tracker. BepInEx 5 must already be installed in the Hearthstone folder.

## Mods
| Tab | Mod | Settings |
|---|---|---|
| Minion dance fix | com.community.hs.NomiCantDance | Enable, log corrected slots |
| Disable abbreviation | com.community.hs.NomiHatesAbbreviation | Install = on, uninstall = off |
| APM provider | com.community.hs.NomisKitchenApm | Debug VerifySampler |

The APM provider only publishes data to shared memory. It shows nothing on screen without an overlay.

## Build
```bash
cd standalone/NomiKitchen
dotnet publish -c Release
```
The exe is in `bin/Release/net10.0-windows/win-x64/publish/`. The mod DLLs are embedded from `Resources/`.

## Run
Start the exe, check the Hearthstone folder, then Install or change settings. Close Hearthstone before installing or uninstalling. Settings apply when Hearthstone next starts.

## Adding a mod
Embed its DLL in `NomiKitchen.csproj` and add a `PluginInfo` record in `Plugins.cs`. The tab and install logic come from that record.

## Test
```bash
NomiKitchenStandalone.exe --selftest "C:\path\to\Hearthstone"
```
Runs install, reinstall, uninstall and config edits, then prints PASS or FAIL per check.
