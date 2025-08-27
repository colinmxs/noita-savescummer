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
- **Instant Restore**: Press F9 to choose and restore from your backup history
- **Pause/Resume**: Pause the backup timer when needed with the P key
- **Persistent Configuration**: Your settings are saved and remembered
- **Single Instance Protection**: Prevents multiple copies from running simultaneously
- **Clean Interface**: Always-visible status with minimal clutter
- **Automatic Cleanup**: Old backups are automatically removed when limit is reached
- **Error Recovery**: Graceful handling of common issues with helpful guidance

## How to Run

### 📥 Download Pre-built Executables (Recommended)

**Latest Stable Release:**
1. Go to the [Releases page](https://github.com/colinmxs/noita-savescummer/releases/latest)
2. Download the appropriate file for your system:
   - **Windows**: `noita-savescummer-windows-x64.zip`
   - **macOS (Intel)**: `noita-savescummer-macos-x64.tar.gz`
   - **macOS (Apple Silicon)**: `noita-savescummer-macos-arm64.tar.gz`
   - **Linux**: `noita-savescummer-linux-x64.tar.gz`
3. Extract the archive
4. Run the executable (see instructions below)

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

**macOS/Linux:**
```bash
# Extract, make executable, and run
chmod +x noita-savescummer
./noita-savescummer
```

### Prerequisites
- **No additional software required!** (Self-contained executables)
- Windows 10/11, macOS 10.15+, or modern Linux distribution
- Noita installed in the standard location

### Installation & Usage

1. **First-time setup**: 
   - Enter your preferred backup interval in minutes
   - Choose how many backup versions to keep (recommended: 5-20)
4. **Let it run**: The application will backup your saves automatically
5. **Restore anytime**: Press F9 to select and restore from available backups

### Controls

- **F9**: Select and restore from available backup versions
- **P**: Pause/Resume the backup timer
- **C**: Configure backup interval and version count
- **Q**: Quit application safely

## Technical Details

### Default Paths
- **Noita Save Location**: `C:\Users\%USERNAME%\AppData\LocalLow\Nolla_Games_Noita\save00`
- **Backup Storage**: `%USERPROFILE%\Documents\NoitaSaveBackups\backups\`
- **Configuration**: `%USERPROFILE%\Documents\NoitaSaveBackups\config.json`

### Backup System
- **Timestamped Backups**: Each backup is stored with format `YYYY-MM-DD_HH-MM-SS`
- **Automatic Cleanup**: Old backups beyond your configured limit are automatically removed
- **Interactive Restore**: Choose from available backup versions when restoring
- **Version Management**: Configurable number of backup versions to maintain

### Requirements
- **Self-contained executables** - No .NET runtime installation needed!
- Cross-platform: Windows 10/11, macOS 10.15+, or modern Linux
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
