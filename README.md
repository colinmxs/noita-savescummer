# Noita Save Scummer

[![Build and Test](https://github.com/colinmxs/noita-savescummer/actions/workflows/ci.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/ci.yml)
[![Release](https://github.com/colinmxs/noita-savescummer/actions/workflows/release.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/release.yml)
[![Development Builds](https://github.com/colinmxs/noita-savescummer/actions/workflows/dev-builds.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/dev-builds.yml)

A lightweight console application for automatically backing up and restoring Noita game saves.

## Features

- **Automated Backups**: Automatically creates backups of your `save00` directory on a timer.
- **Manual Restore**: Interactively restore a previous save state.
- **Full & Partial Restores**: Restore the entire save directory (F9) or only the player data (F8).
- **Backup Preservation**: Protect important backups from automatic deletion (F7).
- **Configurable**: Set the backup frequency and the number of backups to keep.
- **Background Operation**: Runs in the console and waits for user input.

## How to Use

1.  Go to the [Releases page](https://github.com/colinmxs/noita-savescummer/releases/latest).
2.  Download `noita-savescummer-windows-x64.zip`.
3.  Extract the archive and run `noita-savescummer.exe`.

If you get a Windows Defender SmartScreen warning, click "More info," then "Run anyway." This happens because the application is not code-signed.

### First-Time Setup
When you first run the application, you will be asked to configure:
- **Backup Interval**: The time in minutes between automatic backups.
- **Backup Retention**: The maximum number of backups to store.

### Controls

- **F9**: **Full Restore**. Restores the entire save directory, including world and player data.
- **F8**: **Player-Only Restore**. Restores only `player.xml`, preserving the current world state.
- **F7**: **Preserve Backup**. Toggles a "preserved" state for a backup, protecting it from automatic cleanup.
- **P**: Pause or resume the automatic backup timer.
- **C**: Change the backup interval and retention settings.
- **Q**: Quit the application.

## Restore Options

- **Full Restore (F9)**: Reverts your game to a previous state completely. This process is now safer, restoring only essential world and player data to prevent save corruption. Use this to recover from a death or to practice a specific part of the game.

- **Player-Only Restore (F8)**: Restores your character's health, inventory, and stats from a backup, but keeps the current world map. This is useful if you want to continue exploring the current world with a previous version of your character. You can choose to either respawn at the cave entrance or at your character's last saved location.

## File Locations

- **Noita Save Directory**: `C:\Users\%USERNAME%\AppData\LocalLow\Nolla_Games_Noita\save00`
- **Backups**: `%USERPROFILE%\Documents\NoitaSaveBackups\backups\`
- **Configuration**: `%USERPROFILE%\Documents\NoitaSaveBackups\config.json`

## Building from Source

To build the application from source:

```bash
git clone https://github.com/colinmxs/noita-savescummer.git
cd noita-savescummer
dotnet build --configuration Release
dotnet run
```

The project is built with .NET 9.0 and has no external dependencies.
