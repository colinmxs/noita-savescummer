# Build Status

This document provides a real-time snapshot of the current build and deployment status of the Noita Save Scummer application.

> **📋 For operational procedures and release workflows, see [RELEASE_STRATEGY.md](./RELEASE_STRATEGY.md)**

## Current Status

✅ **Version**: v1.0.4 (Latest Stable)  
✅ **Build Status**: Windows builds successful  
✅ **Release Automation**: GitHub Actions fully operational  
✅ **Platform Support**: Windows 10/11 (64-bit)  

## Technical Configuration

| Component | Status | Details |
|-----------|---------|---------|
| **Framework** | ✅ Active | .NET 9.0 |
| **Architecture** | ✅ Clean | Separated concerns (Models/Services/UI) |
| **Dependencies** | ✅ Minimal | Built-in .NET libraries only |
| **JSON Serialization** | ✅ Fixed | Source generation for AOT compatibility |
| **Console Display** | ✅ Fixed | Unicode detection with ASCII fallback |
| **Build Type** | ✅ Optimized | Self-contained executables |

## GitHub Actions Status

| Workflow | Trigger | Status | Purpose |
|----------|---------|---------|---------|
| **CI** (`ci.yml`) | Push/PR | ✅ Active | Windows build validation |
| **Release** (`release.yml`) | Version tags | ✅ Active | Production releases with Windows executable |
| **Dev Builds** (`dev-builds.yml`) | Main branch | ✅ Active | Development artifacts for Windows |

## Current Release Artifacts

**Latest Release**: [v1.0.4](https://github.com/colinmxs/noita-savescummer/releases/latest)

| Platform | Artifact Name | Status |
|----------|---------------|---------|
| Windows x64 | `noita-savescummer-windows-x64.zip` | ✅ Available |

## Known Issues & Solutions

| Issue | Status | Solution |
|-------|---------|----------|
| **Windows SmartScreen Warning** | 🔄 Planned | Working on code signing implementation |
| **Self-contained Size** | ✅ Optimized | Using PublishTrimmed and partial trimming |

### Windows SmartScreen Warning
- **Issue**: "Microsoft Defender SmartScreen prevented an unrecognized app" 
- **Cause**: Unsigned executable without established reputation
- **Workaround**: Click "More info" → "Run anyway" or unblock in Properties
- **Permanent Fix**: Code signing certificate (in progress)

## Recent Fixes & Improvements

- **v1.0.4**: Windows-only focus, removed unnecessary cross-platform code
- **v1.0.3**: SmartScreen documentation and enhanced executable trust metadata
- **v1.0.2**: Console icon display compatibility (Windows ?? fix)
- **v1.0.1**: JSON serialization for self-contained builds
- **v1.0.0**: Initial stable release with full feature set

## Quick Build Test

```bash
# Verify local build works
dotnet clean && dotnet build --configuration Release

# Test Windows build (example)
dotnet publish -c Release -r win-x64 --self-contained -o test-build
./test-build/noita-savescummer.exe
rm -rf test-build
```

## Monitoring Links

- **Actions**: [GitHub Actions](https://github.com/colinmxs/noita-savescummer/actions)
- **Releases**: [GitHub Releases](https://github.com/colinmxs/noita-savescummer/releases)
- **Issues**: [GitHub Issues](https://github.com/colinmxs/noita-savescummer/issues)

---

**Last Updated**: August 26, 2025  
**Next Scheduled Review**: As needed based on releases

> **💡 Tip**: This document reflects current status. For detailed operational procedures, version history, and release management, see [RELEASE_STRATEGY.md](./RELEASE_STRATEGY.md)
