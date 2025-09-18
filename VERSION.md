# Version History

This document contains the release history and changelog for the Noita Save Scummer.

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
