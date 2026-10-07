# Noita Save Scummer

[![Build and Test](https://github.com/colinmxs/noita-savescummer/actions/workflows/ci.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/ci.yml)
[![Release](https://github.com/colinmxs/noita-savescummer/actions/workflows/release.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/release.yml)
[![Development Builds](https://github.com/colinmxs/noita-savescummer/actions/workflows/dev-builds.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/dev-builds.yml)

A lightweight console application for automatically backing up and restoring Noita game saves.

## Features

Backs up your Noita save on a timer and every time you Save & Quit, and restores it with one key (or one in-game hotkey) without corrupting the world.

## How to Use

1.  Go to the [Releases page](https://github.com/colinmxs/noita-savescummer/releases/latest).
2.  Download `noita-savescummer-windows-x64.zip`.
3.  Extract the archive and run `noita-savescummer.exe`.

If you get a Windows Defender SmartScreen warning, click "More info," then "Run anyway." This happens because the application is not code-signed.

### Recommended workflow
- **Save & Quit** in Noita whenever you reach a spot you want to keep. The app takes a `[CLEAN]` backup the moment Noita exits. These are the most reliable.
- Timed backups also run while you play. They are only kept if no file changed during the copy, and are tagged `[LIVE]`.
- After a restore, start Noita and choose **Continue**.

### Controls (app window)

| Key | Action |
|-----|--------|
| F9  | Full restore: world + player from a backup |
| F8  | Player-only restore: backup's player into the current world (optionally at spawn) |
| F7  | Preserve / unpreserve a backup (never auto-deleted) |
| F6  | Wand tools: save a wand as a template, or give a saved wand to the current run |
| U   | Undo the last restore |
| B   | Back up now |
| P   | Pause / resume the timer |
| C   | Settings |
| Q   | Quit |

Menus show every backup (page with arrows / PgUp / PgDn, pick with 1-9 or Enter).

### Global hotkeys (work while Noita is focused)

| Key | Action |
|-----|--------|
| Ctrl+Alt+F5 | Quick-save: back up now |
| Ctrl+Alt+F9 | Quick-load: close Noita, restore the newest backup, relaunch Noita via Steam |

### Starting a run with a wand (e.g. black hole)
1. Get the wand once in any run, **Save & Quit**, press **F6** then **S** and pick it. It's saved to `NoitaSaveBackups\wand_templates\`.
2. In a new run, **Save & Quit** right after starting, press **F6** then **G** and pick the template.
3. Start Noita and choose **Continue**. The wand is in your first free wand slot. **U** undoes it.

The wand is copied exactly as Noita saved it, so no mods are involved and the run is not marked as modded.

## Why restores used to break the world (fixed in 2.0)

Noita streams the world to disk while you play. `save00/world/` holds one file per chunk plus `.stream_info`, which stores the pixel-scene **background list**, camera values and the table of loaded chunks. These files are only consistent with each other when written together, which happens on Save & Quit.

Version 1.x could mix moments in time in three ways, and each one produces offset or missing backgrounds and structures:
1. It copied `save00` while Noita was writing, so one backup could hold chunks from different moments.
2. It restored while Noita was running. The game then overwrote the restored files from memory on exit.
3. Full restore copied files over the current save and left `session_numbers.salakieli` behind, so files from the newer run survived.

Version 2.0:
- Verifies each backup was not modified during the copy and retries if it was.
- Backs up automatically when Noita exits.
- Refuses to restore while Noita is running, or force-closes it if you choose.
- Builds the restored `save00` completely, then swaps it in with a directory rename, so no newer-run files survive.

## File Locations

- **Noita Save Directory**: `%USERPROFILE%\AppData\LocalLow\Nolla_Games_Noita\save00`
- **Backups**: `%USERPROFILE%\Documents\NoitaSaveBackups\backups\`
- **Configuration**: `%USERPROFILE%\Documents\NoitaSaveBackups\config.json`

Override with `--save-path <dir>` / `--backup-path <dir>` or the `NOITA_SAVE_PATH` / `NOITA_SCUMMER_BACKUP_PATH` environment variables.

By default a full restore keeps your current unlocks, stats and mod settings (`persistent/`, `stats/`, `mod_config.xml`, `mod_settings.bin`). Turn this off in settings to restore an exact copy.

## Building from Source

```bash
git clone https://github.com/colinmxs/noita-savescummer.git
cd noita-savescummer
dotnet build --configuration Release
dotnet test
dotnet run --project noita-savescummer.csproj
```

The project is built with .NET 9.0. The app has no third-party dependencies; the test project uses xUnit.
