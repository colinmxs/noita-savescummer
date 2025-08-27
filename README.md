# Noita Save Scummer

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

### Prerequisites
- Windows operating system
- .NET 9.0 Runtime
- Noita installed in the standard location

### Installation & Usage

1. **Download** the latest release or build from source
2. **Run** `noita-savescummer.exe`
3. **First-time setup**: 
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
- Built with .NET 9.0
- No external dependencies
- Minimal system resource usage
- Compatible with Windows 10/11

## Troubleshooting

### Common Issues

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
