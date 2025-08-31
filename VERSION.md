# Version Information

Current Version: 1.2.0

## Release Notes

### v1.2.0 - Backup Preservation Feature (August 31, 2025)
- **Added**: Backup preservation system (F7 key)
- **Enhanced**: Protect important backups from automatic cleanup
- **Added**: Visual preservation indicators with lock/unlock icons
- **Improved**: Preservation state persisted in JSON metadata
- **Added**: New preservation icons in IconProvider (Lock, Unlock, Pin)
- **Enhanced**: Backup cleanup logic now respects preserved backups
- **Added**: PreservationService for managing backup metadata
- **Updated**: Console interface to show F7 preservation management
- **Benefit**: Keep critical save states safe from automatic deletion
- **Feature**: Toggle preservation status for any backup individually

### v1.1.0 - Player-Only Restore Feature (August 29, 2025)
- **Added**: Player-only restore functionality (F8 key)
- **Enhanced**: Dual restore options - full save (F9) or player.xml only (F8)
- **Added**: Optional player location reset during F8 restore
- **Improved**: Interactive restore process with location choice (Y/N/ESC)
- **Enhanced**: XML manipulation to modify player position coordinates
- **Added**: New player icon in IconProvider for UI consistency
- **Updated**: Console interface to show both restore options clearly
- **Benefit**: Allows restoring character state while preserving world progress
- **Feature**: Reset player to cave entrance or keep backup position

### v1.0.4 - Windows-Only Focus (August 26, 2025)
- **Simplified**: Removed all cross-platform support (Noita is Windows-only!)
- **Optimized**: Windows-only builds and workflows
- **Streamlined**: Single Windows executable release
- **Enhanced**: Faster builds with Windows-specific optimizations
- **Cleaned**: Removed unnecessary macOS and Linux references from documentation
- **Focused**: Dedicated Windows development and testing environment

### v1.0.3 - SmartScreen Documentation & Enhanced Trust (August 26, 2025)
- **Added**: Comprehensive Windows SmartScreen workaround documentation
- **Enhanced**: Assembly metadata for improved executable trust (company, product, version info)
- **Improved**: User guidance for handling Windows security warnings
- **Updated**: README with detailed SmartScreen bypass instructions
- **Added**: Troubleshooting section for common security dialog issues

### v1.0.2 - Console Display Fix (August 26, 2025)
- **Fixed**: Console icon display issues on Windows (showing ?? instead of icons)
- **Added**: IconProvider with smart Unicode detection and ASCII fallback
- **Improved**: Console encoding handling for better Windows compatibility
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
- Windows 10/11 support with self-contained executable
- Self-contained executables with no dependencies
- Clean console interface with real-time status
- Robust error handling and recovery
- Single-instance protection

## Upcoming Features
- Configuration file export/import
- Backup compression options
- Multiple save slot support
- Backup verification checksums
