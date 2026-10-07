# Product: Noita Save Scummer

Windows console app that backs up the Noita save directory and restores it via hotkeys.

## Users and goal
Noita players who want to undo deaths or retry sections. Core promise: never lose a save, never corrupt a save. Data safety beats every other concern.

## How Noita saves (why consistency matters)
- `save00/world/` is streamed to disk during play: per-chunk `world_X_Y.png_petri`, `entities_N.bin`, plus `world_pixel_scenes.bin`, `world_sim.bin`, `world_tree.bin` and `.stream_info` (pixel-scene background list, camera values, seed, loaded-chunk table).
- These files are only mutually consistent when written together, which happens on Save & Quit. Mixing files from different moments causes offset/missing backgrounds and missing structures.
- Per-run state: `player.xml`, `world_state.xml`, `session_numbers.salakieli`, `world/`. Cross-run progress: `persistent/`, `stats/`, `mod_config.xml`, `mod_settings.bin` (see `SaveLayout`).
- Noita overwrites save00 from memory when it exits, so never restore while it runs.

## Features
- Timed backups (verified unchanged during copy, `[LIVE]`), automatic backup on Noita exit (`[CLEAN]`), skip unchanged
- F9 full restore (atomic directory swap, optional keep-current-progress), F8 player-only restore (optional spawn reset)
- Undo backup before every restore (U), preservation (F7), B backup now, P pause, C settings, Q quit
- Global hotkeys Ctrl+Alt+F5 quick-save / Ctrl+Alt+F9 quick-load (close Noita, restore newest, relaunch)

## Storage
- Backups: `%USERPROFILE%\Documents\NoitaSaveBackups\backups\<yyyy-MM-dd_HH-mm-ss[_n]>\` + `backup.json` metadata
- Preservation: `backups\preserved_backups.json`; config: `NoitaSaveBackups\config.json`
- In-progress artifacts: `backups\.partial-*`, `save00.scummer-staging`, `save00.scummer-old` (recovered on startup)

## Product rules
- Any write into save00 is destructive: take an undo backup first, build the replacement fully, then swap with renames.
- Never restore while Noita runs; never back up mid-write without verifying the copy.
- Keep README.md and VERSION.md in sync with behavior changes.
