# Noita Save Scummer

[![Build and Test](https://github.com/colinmxs/noita-savescummer/actions/workflows/ci.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/ci.yml)
[![Release](https://github.com/colinmxs/noita-savescummer/actions/workflows/release.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/release.yml)
[![Development Builds](https://github.com/colinmxs/noita-savescummer/actions/workflows/dev-builds.yml/badge.svg)](https://github.com/colinmxs/noita-savescummer/actions/workflows/dev-builds.yml)

A lightweight console application for automated backup and restoration of Noita game saves.

## Purpose

This application provides automated, timer-based backups of your Noita save files with instant restore capabilities. Perfect for experimentation, speedrunning practice, or recovering from unfortunate in-game events.

## Features

- **Automated Backups**: Configurable timer-based backups of your Noita save directory
- **Multiple Versions**: Keep multiple timestamped backup versions (configurable count)
- **Interactive Restore**: Select from available backup versions when restoring
- **Full Save Restore**: Press F9 to choose and restore complete save (world + player)
- **Player-Only Restore**: Press F8 to restore just player.xml (preserves world state)
- **Pause/Resume**: Pause the backup timer when needed with the P key
- **Persistent Configuration**: Your settings are saved and remembered
- **Single Instance Protection**: Prevents multiple copies from running simultaneously
- **Clean Interface**: Always-visible status with minimal clutter
- **Automatic Cleanup**: Old backups are automatically removed when limit is reached
- **Error Recovery**: Graceful handling of common issues with helpful guidance

## How to Run

### 📥 Download Pre-built Executable (Recommended)

**Latest Stable Release:**
1. Go to the [Releases page](https://github.com/colinmxs/noita-savescummer/releases/latest)
2. Download **noita-savescummer-windows-x64.zip**
3. Extract the archive
4. Run `noita-savescummer.exe`

**Development Builds (Latest Features):**
- Check the [dev-latest release](https://github.com/colinmxs/noita-savescummer/releases/tag/dev-latest) for cutting-edge builds

### 🚀 Running the Application

**Windows:**
```bash
# Extract and run
noita-savescummer.exe
```

**⚠️ Windows Security Notice:**
If Windows Defender SmartScreen blocks the app with "Microsoft Defender SmartScreen prevented an unrecognized app from starting":

1. **Click "More info"** in the SmartScreen dialog
2. **Click "Run anyway"** at the bottom
3. **Alternative**: Right-click the exe → Properties → General tab → Check "Unblock" → Apply → OK

This happens because the executable isn't code-signed yet. The application is safe - you can verify by checking the source code in this repository.

### Prerequisites
- **No additional software required!** (Self-contained executable)
- Windows 10/11 (64-bit)
- Noita installed in the standard location

### Installation & Usage

1. **First-time setup**: 
   - Enter your preferred backup interval in minutes
   - Choose how many backup versions to keep (recommended: 5-20)
4. **Let it run**: The application will backup your saves automatically
5. **Restore options**: 
   - Press F9 for full save restore (world + player)
   - Press F8 for player-only restore (preserves world, restores character)
   - **For F8**: Exit to main menu first, then start a new run after restoring

### Controls

- **F9**: Select and restore from available backup versions (full save)
- **F8**: Select and restore player.xml only (preserves world/progress)
- **P**: Pause/Resume the backup timer
- **C**: Configure backup interval and version count
- **Q**: Quit application safely

### Restore Types Explained

**Full Restore (F9)**: 
- Restores the entire save directory including world state, progress, and player data
- Use when you want to completely revert to a previous save state
- Perfect for undoing major world changes or catastrophic events

**Player-Only Restore (F8)**:
- Restores only the player.xml file (character stats, inventory, spells, etc.)
- Preserves the current world state and progress
- **IMPORTANT**: You must start a new run in Noita before using player-only restore
- The game needs to be at the character selection/starting screen for player.xml to take effect
- **Location Reset Option**: Choose whether to reset player to starting position or keep current location
- Ideal for character experimentation, spell testing, or recovering from player death
- Maintains world exploration progress, unlocks, and environmental changes

### Player-Only Restore Workflow

1. **Exit current run**: If you're in an active Noita run, exit to main menu
2. **Use player restore**: Press F8 in the save scummer and select your backup
3. **Choose location option**: 
   - **Yes**: Reset player to cave entrance (starting position)
   - **No**: Keep player at their current location from the backup
4. **Start new run**: Launch a new run in Noita - your restored character will load
5. **Continue playing**: Your character state is restored while keeping world progress

> **Note**: Player-only restore will not work if you try to continue an existing run. You must start fresh for the player.xml changes to be applied.

> **Location Reset**: If you choose to reset location, your character will spawn at the cave entrance regardless of where they were when the backup was made. This is useful for escaping dangerous areas or starting fresh exploration.

## Technical Details

### Default Paths
- **Noita Save Location**: `C:\Users\%USERNAME%\AppData\LocalLow\Nolla_Games_Noita\save00`
- **Backup Storage**: `%USERPROFILE%\Documents\NoitaSaveBackups\backups\`
- **Configuration**: `%USERPROFILE%\Documents\NoitaSaveBackups\config.json`

### Backup System
- **Timestamped Backups**: Each backup is stored with format `YYYY-MM-DD_HH-MM-SS`
- **Automatic Cleanup**: Old backups beyond your configured limit are automatically removed
- **Dual Restore Options**: Choose between full save restore or player-only restore
- **Full Restore (F9)**: Restores complete save including world state and player data
- **Player Restore (F8)**: Restores only player.xml, preserving current world/progress
- **Location Reset**: Optional player location reset to starting position during F8 restore
- **Version Management**: Configurable number of backup versions to maintain

### Requirements
- **Self-contained executable** - No .NET runtime installation needed!
- Windows 10/11 (64-bit)
- Minimal system resource usage
- Works with standard Noita installation paths

## Building from Source

If you prefer to build from source or contribute to development:

```bash
git clone https://github.com/colinmxs/noita-savescummer.git
cd noita-savescummer
dotnet build --configuration Release
dotnet run
```

### Development
- Built with .NET 9.0
- Clean architecture with separated concerns
- Comprehensive error handling
- Async/await for responsive operations

## Troubleshooting

### Common Issues

**"Microsoft Defender SmartScreen prevented an unrecognized app from starting"**
- Click "More info" then "Run anyway" in the SmartScreen dialog
- Alternative: Right-click executable → Properties → Check "Unblock" → Apply
- This occurs because the executable isn't code-signed (working on fixing this)
- The application is completely safe - source code is available for verification

**"Noita save directory not found"**
- Ensure Noita has been run at least once to create the save directory
- Check that Noita is installed in the standard Steam/GOG location

**"Permission denied"**
- Run as administrator if backup location is restricted
- Ensure backup directory is writable

**"Application already running"**
- Only one instance can run at a time
- Check system tray or task manager for existing process

## Building from Source

```bash
git clone https://github.com/colinmxs/noita-savescummer.git
cd noita-savescummer
dotnet build --configuration Release
dotnet run
```

## Contributing

This project maintains zero external dependencies and focuses on simplicity and reliability. Please follow the established coding standards and test thoroughly on Windows systems.

## License

MIT License - Feel free to modify and distribute as needed.
