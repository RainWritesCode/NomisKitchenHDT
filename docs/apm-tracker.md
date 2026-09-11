# APM Tracker
## Architecture Overview
The APM tracker consists of 2 major parts. First one is the plugin that is shipped directly into the HS directory. It is shipped as compiled `.dll` file and exists within the HS game process itself. The second part is the actual HDT plugin that is shipped with an install executable. These two parts communicate between one another with a JSON that is written into the MMF.

### Responsibilities split
The HS plugin actually collects game data, events and other metrics. It is also the component that does the APM calculations.

The HDT plugin on the other hand just receives said data and displays it in the UI based on the user's settings.

## Installation
Wheneever the NomiKitchen plugin is initialized by the HDT the of the `Plugin.OnLoad()` is called. This method instantiates the `ApmProviderInstaller` class and immediately calls `.EnsureInstalled()` method of it. Said method checks for the existence of the shipped DLL in the proper location folder. If none exists then one is placed into said folder.

> If there is no Hearthstone BepInEx location in the config class the plugin also check for a few of the most common locations to try to determine the location on its own

> [GAP] If there already is a `.dll` updating said `.dll` currently would require user's involvement. And said deletion would require user to locate the plugins folder and delete existing `.dll` so that `ApmProviderInstaller` will recreate the newer version of it on the next `.OnLoad()`

## HS Plugin
This is a Unity plugin and it extends the Unity's MonoBehavior class.
Name of the package for this plugin is defined as `NomisKitchenHDT.Resources.com.community.hs.NomisKitchenApm.dll` with the file it is located being `com.community.hs.NomisKitchenApm.dll`.

### Workflow
#### Start
The on `.Start()` procedure is called when the plugin is initiated. It creates a MMF with the same name as HDT plugin.
> Currently these values are defined as separate `const` declarations in each plugin — `MmfName` and `MmfSize` exist independently in both `ApmBehaviour` and `ApmTracker`. There is no shared source of truth, so the two sets must be kept matching by hand.
Then the accessor is created. References to both are stored as class members.

#### Update
On every call of the `.Update()` method of the ApmBehavior class the current diff between frames is fetched via native Unity interface. Said diff is added to 2 different counters. If each of the timers exceeds defined constant the corresponding method is triggered and the counter is reset. The methods are `.SampleApm()` and `.WriteStats()`. 

##### .SampleApm()

Checks for the game to exists and for the game to be in proper phase for the sampling to make sense. Then, values such as mana and cards both on board and in hand are fetched and based on this the assumptions on the amount of actions performed are made. After all is done, these values are stored for the next tick to compare to and for accessibility in the `.WriteStats()`.
> [TODO] Properly describe APM sampling logic here

##### .WriteStats()
Takes values from members of the class internal memory and writes them in the JSON format to a MMF.

#### Dispose
MMF accessor and MMF itself is cleaned up.

## HDT plugin
This consists of multiple classes. Currently the main logic is in the `ApmTracker` class. It is instantiated during the `.OnLoad()` of the main plugin. It operates on the separate timer. Whenever the timer ticks, the `ApmTracker` class fetches updates from the MMF and invokes the `OnStatsUpdated` event. This event is then caught by the ApmOverlay form supporting code. The values are then updated and stored in the form's memory and subsequently rendered.

## IPC
The mechanism for communication between HS and HDT plugins is MMF. The file name is currently defined as `NomisKitchenApm` and is 512 bytes long. It is the JSON file exclusively written by HS side and read by HDT.
The JSON structure is as follows:
```
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "type": "object",
  "properties": {
    "inGame": {
      "type": "boolean"
    },
    "actionsThisTurn": {
      "type": "integer"
    },
    "currentApm": {
      "type": "number"
    },
    "peakApm": {
      "type": "number"
    },
    "averageApm": {
      "type": "number"
    },
    "gold": {
      "type": "integer"
    },
    "hand": {
      "type": "integer"
    },
    "board": {
      "type": "integer"
    },
    "turn": {
      "type": "integer"
    },
    "updatedTicks": {
      "type": "integer"
    }
  },
  "required": [
    "inGame",
    "actionsThisTurn",
    "currentApm",
    "peakApm",
    "averageApm",
    "gold",
    "hand",
    "board",
    "turn",
    "updatedTicks"
  ],
  "additionalProperties": false
}
```

## Glossary
* HS - Hearthstone
* HDT - Hearthstone Deck Tracker
* IPC - Inter-Process Communication
* MMF - Memory Mapped File
* BepInEx - [Bepis Injector Extensible](https://github.com/bepinex/bepinex)