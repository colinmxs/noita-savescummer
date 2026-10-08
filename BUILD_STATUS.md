# Build Status

## Current Status

- **Version**: v2.0.0
- **Framework**: .NET 9.0
- **Platform**: Windows 10/11 (64-bit)
- **Tests**: xUnit suite in `tests/NoitaSaveScummer.Tests` (37 tests), run by CI
- **Publish**: trimmed single-file `win-x64` publish is warning-free and smoke-tested in CI

## Technical Details

- **Architecture**: Models / Services / UI layers. Services do no console I/O and are tested against temp directories.
- **Dependencies**: No third-party dependencies in the app (xUnit in tests only).
- **Serialization**: Source-generated JSON only. Reflection-based JSON is disabled in every configuration so mistakes fail in tests, not in release.
- **Console Display**: Diff-based renderer (no flicker); Unicode icons with ASCII fallback.

## Known Issues

- **Windows SmartScreen Warning**: The executable is not code-signed. Click "More info" then "Run anyway."
- **Noita running as administrator**: the app cannot force-close it; close Noita manually before restoring.
