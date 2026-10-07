# Version History

This document contains the release history and changelog for the Noita Save Scummer.

## Version 2.0.0
*   **Critical Fix**: Restored worlds no longer have misaligned backgrounds or missing structures. Backups are checked for writes during the copy and retried. Restores are blocked while Noita runs. A full restore swaps in a complete `save00` atomically, so no files from the newer run survive (including `session_numbers.salakieli` and new chunks).
*   **Critical Fix**: Preserved backups (F7) were lost in released builds because the preservation file used reflection-based JSON, which trimmed builds disable.
*   **Fix**: A failed backup no longer leaves partial folders, retries every 3 seconds or deletes good backups through retention.
*   **Fix**: Player-only position reset writes invariant-culture numbers (it used to write `215,000000` on non-English Windows).
*   **Fix**: Single-instance mutex no longer crashes on exit; Ctrl+C and Q shut down cleanly.
*   **Fix**: Countdowns over one hour, settings resetting other options, status messages overwriting controls, flicker.
*   **Feature**: Automatic `[CLEAN]` backup when Noita exits; skip unchanged timed backups.
*   **Feature**: Global hotkeys Ctrl+Alt+F5 (quick-save) and Ctrl+Alt+F9 (quick-load: close Noita, restore, relaunch).
*   **Feature**: Automatic undo backup before every restore (U to undo).
*   **Feature**: Full restore keeps current unlocks/stats by default.
*   **Feature**: B = back up now. Paged menus show every backup with tags. Recent backups appear on the main screen. Path overrides.
*   **Dev**: xUnit test suite (30 tests), CI runs tests and a trimmed publish, release notes link here.

## Version 1.2.1
*   **Critical Fix**: Overhauled the full restore (F9) logic to prevent world corruption. The restore process now only copies essential files (`player.xml`, `world_state.xml`, and the `world` folder), significantly improving safety and reliability.

## Version 1.2.0
*   **Feature**: Added a backup preservation system (F7) to protect important backups from automatic deletion.

## Version 1.1.0
*   **Feature**: Implemented a "Player-Only" restore option (F8) to restore the player's state while preserving the current world.

## Version 1.0.4
*   **Improvement**: Removed cross-platform code to focus exclusively on Windows.

## Version 1.0.3
*   **Docs**: Added instructions to the README for bypassing the Windows Defender SmartScreen warning.

## Version 1.0.2
*   **Fix**: Corrected an issue where console icons would not display correctly on some Windows systems.

## Version 1.0.1
*   **Fix**: Resolved a JSON serialization error that occurred in self-contained builds.

## Version 1.0.0
*   **Initial Release**: First public release of the application. Includes core features for automatic backup and restore.
