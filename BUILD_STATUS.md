# Build Status

This document provides a summary of the project's current build status and technical configuration.

## Current Status

- **Version**: v1.2.0
- **Framework**: .NET 9.0
- **Platform**: Windows 10/11 (64-bit)
- **Builds**: Automated builds for Windows are passing.
- **Releases**: The release workflow is operational.

## Technical Details

- **Architecture**: The project follows a clean architecture with a separation of concerns into Models, Services, and UI layers.
- **Dependencies**: This project has no third-party dependencies and uses only built-in .NET libraries.
- **Serialization**: JSON serialization is handled using source generation to ensure compatibility with Ahead-of-Time (AOT) compilation.
- **Console Display**: The console UI uses Unicode detection with an ASCII fallback to ensure proper display of icons.
- **Distribution**: The application is distributed as a self-contained executable that does not require a separate .NET runtime installation.

## Known Issues

- **Windows SmartScreen Warning**: Because the executable is not code-signed, Windows Defender may show a warning on first run. The workaround is to click "More info" and then "Run anyway."
