# Symphony of the Night Khaos Tools

[![(latest) release | GitHub](https://img.shields.io/github/release/TalicZealot/SotnKhaosTools.svg?logo=github&logoColor=333333&style=popout)](https://github.com/TalicZealot/SotnKhaosTools/releases/latest)

A tool for Twitch chat interaction while playin the [Castlevania:Symphony of the Night Randomizer](https://sotn.io).

This tool and the accompanying library and app are open source. The idea is to implement these features with the perspective of SotN players and provide the source for other developers to learn from and contribute just like the randomizer itself.

## Associated Projects
* [SotnRandoTools](https://github.com/TalicZealot/SotnRandoTools)
* [SotnApi](https://github.com/TalicZealot/SotnApi)
* [SimpleLatestReleaseUpdater](https://github.com/TalicZealot/SimpleLatestReleaseUpdater)
* [SotN Randomizer Source](https://github.com/3snowp7im/SotN-Randomizer)

## Table of Contents

- [Symphony of the Night Khaos Tools](#symphony-of-the-night-khaos-tools)
  - [Installation](#installation)
  - [Usage](#usage)
  - [Updating](#updating)
  - [Additional Features](#Additional-Features)
  - [Khaos-Setup](#Khaos-Setup)
  - [Useful links](#useful-links)
  - [Contributors](#contributors)
  - [Special Thanks](#special-thanks)

## Installation
This tool requires Bizhawk version 2.8 or higher.
Download the full version from the [latest release](https://github.com/TalicZealot/SotnKhaosTools/releases/latest) that looks like this `SotnKhaosTools-x.x.x.zip`
Right click on it and select `Extract all...` then navigate to your BizHawk 2.7+ folder and press `Extract`.
File structure should look like this:
```
BizHawk
└───ExternalTools
│   │   SotnKhaosTools.dll
│   │
│   └───SotnKhaosTools
│   │     │   SotnApi.dll
│   │     │   ...
│   └───TwitchLib.Api
│   └───TwitchLib.Api.Helix.Models
```

## Usage
After launching the game in BizHawk open through ```Tools > Extarnal Tool > Symphony of the Night Randomizer Tools```
Set your preferences and open the tool you want to use. You can then minimize the main tools window, but don't close it.
Every tool's window possition and the Tracker's size are all saved and will open where you last left them.
If the Extarnal Tool says that the game is not supported for the tool and BizHawk is displaying a question mark in the lower left corner your rom is either not recognized or you have to make sure the cue file is pointing to the correct files. I recommend creating a separate folder for Randomizer where you copy both tracks and the cue and replace track1 every time you randomize.

## Updating
On lunching the tool it will check for a new release and inform the user. If there is a newer release the update button apepars. Clicking it shuts down BizHawk and updates the tool. If it displays "Installation failed" please run the updater manually by going to ```BizHawk\ExternalTools\SotnKhaosTools\Updater\SimpleLatestReleaseUpdater.exe``` or get the [latest release](https://github.com/TalicZealot/SotnKhaosTools/releases/latest) from GitHub and update manually. If you get an error notifying you that your system lacks the necessary .NET version to run the updater click [the link](https://dotnet.microsoft.com/download/dotnet/5.0/runtime?utm_source=getdotnetcore&utm_medium=referral) and download the x64 and x86 redistributable packages for desktop apps.

## Additional Features
Contains an identical autotracker to SotnRandoTools.

## Khaos-Setup
Inside the folder ```BizHawk\ExternalTools\SotnKhaosTools\Khaos\Overlay\``` you will find ```meter.html```, ```action-queue.html``` and ```timers.html``` which you can add in OBS as web sources.
After starting Khaos you will be able to connect to Twitch and start Auto Khaos.
* Clicking "Connect to Twitch" will take you to a website to confirm that you allow SotN Rando Tools to manage Channel Points and see your subscribers. After accepting it will create custom Channel Point rewards for every action and listen for redemptions. When you disconnect or close Khaos the Custom Rewards should get deleted. Please wait for about 20 seconds for all the rewards to get deleted by the API. Redemptions get automatically fulfilled after about a minute and a half, before that the streamer can see them in the redemptions panel in the Khaos window and have the option to refund it.
* Auto Khaos automatically activates random actions periodically, adhering to the cooldowns. Action frequency is dependant on difficulty setting.

## Useful links
* [SotN Randomizer](https://sotn.io)
* [Latest BizHawk release](https://github.com/TASVideos/BizHawk/releases/latest)

## Contributors
* [3snowp7im](https://github.com/3snowp7im) - SotN Randomizer developer
* [fatihG](https://twitter.com/fatihG_) - Familiar card icons, replay system concept.

## Special Thanks
* asdheyb
* fatihG
* EmilyNyx
* DinnerDog
* Gods666thChild
* LordalexZader
* ziggypigster
* chrisaegrimm