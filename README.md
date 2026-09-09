# Nomi's Kitchen

A Hearthstone Deck Tracker plugin for Battlegrounds streamers. Ships two quality-of-life features:

- **APM overlay** : a small draggable card showing your live actions-per-minute, peak APM, and average APM. Same algorithm as Firestone's overlay.
- **Disable abbreviation** : an in-game change that disabled abbreviated number `1.2k` / `5M` of attack and health values back into full digits. Ships a small BepInEx plugin under the hood and you can turn it on and off from the settings window.

## Install

**1. Get Hearthstone Deck Tracker**
   Download and install [HDT](https://hsdecktracker.net/). Launch it at least once so the config folders exist.

**2. Download the latest release**
   Grab the `NomisKitchenHDT-vX.Y.Z.zip` from the [Releases](../../releases) page and extract it anywhere.

**3. Run the installer**
   Run the exe installer. It drops the DLL into the right HDT plugin folder and installs the required libraries.

**4. Turn it on inside HDT**
   Restart HDT. Open Options → Tracker → Plugins. Check the box next to "Nomi's Kitchen". Click "Settings" to configure it.


## Using the "Disable abbreviation" toggle

When you flip the toggle on in the settings window, the plugin drops the companion DLL. When you flip it off, the plugin removes it. Either way you need to restart Hearthstone for the change to take effect.

## Using the APM overlay

Turn it on in the settings window. A draggable card appears in the top-left of the Hearthstone window. The Position of the window is remembered across HDT restarts.

## Uninstall

Open `%APPDATA%\HearthstoneDeckTracker\Plugins\NomisKitchenHDT\` and delete the folder. If you had the "Disable abbreviation" toggle on, the companion BepInEx DLL is left in place and emove it from `<Hearthstone>\BepInEx\plugins\com.community.hs.NomiHatesAbbreviation.dll` 


## License

GNU AGPL v3. See [LICENSE](LICENSE). Any fork or derivative, including anything served over a network, must keep its source public under the same license.

See [NOTICE](NOTICE) for the author's request on AI-generated reuse.
