# Version Information

Current Version: 1.0.2

## Release Notes

### v1.0.2 - Console Display Fix (August 26, 2025)
- **Fixed**: Console icon display issues on Windows (showing ?? instead of icons)
- **Added**: IconProvider with smart Unicode detection and ASCII fallback
- **Improved**: Console encoding handling for better cross-platform compatibility
- **Enhanced**: Automatic UTF-8 enablement on Windows for Unicode support

### v1.0.1 - JSON Serialization Fix (August 26, 2025)
- **Fixed**: Application crash when saving configuration in self-contained builds
- **Added**: JSON source generation context for AOT compatibility
- **Improved**: Configuration directory creation to ensure save path exists
- **Enhanced**: Error handling for configuration save operations

### v1.0.0 - Initial Release (August 26, 2025)
- Automated backup system with configurable intervals
- Multiple versioned backups with automatic cleanup
- Interactive restore with backup selection menu
- Pause/Resume functionality for timer control
- Cross-platform support (Windows, macOS, Linux)
- Self-contained executables with no dependencies
- Clean console interface with real-time status
- Robust error handling and recovery
- Single-instance protection

## Upcoming Features
- Configuration file export/import
- Backup compression options
- Multiple save slot support
- Backup verification checksums
