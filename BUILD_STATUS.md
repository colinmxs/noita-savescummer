# Build Status

This document tracks the current build and deployment status of the Noita Save Scummer application.

## Current Setup

✅ **Application Code**: Clean architecture with separated concerns  
✅ **Build Configuration**: .NET 9.0 with Release configuration  
✅ **Cross-Platform Publishing**: Windows x64 self-contained builds working  
✅ **GitHub Actions**: CI/CD workflows configured for automated builds  

## GitHub Actions Workflows

### 1. Continuous Integration (`ci.yml`)
- **Trigger**: Every push and pull request
- **Purpose**: Build and test the application
- **Platforms**: Windows, macOS, Linux
- **Framework**: .NET 9.0

### 2. Release Builds (`release.yml`)
- **Trigger**: Git tags (v*.*.*)
- **Purpose**: Create releases with cross-platform executables
- **Artifacts**: 
  - Windows x64: `noita-savescummer-windows-x64.exe`
  - macOS Intel: `noita-savescummer-macos-x64`
  - macOS ARM: `noita-savescummer-macos-arm64`
  - Linux x64: `noita-savescummer-linux-x64`

### 3. Development Builds (`dev-builds.yml`)
- **Trigger**: Pushes to main branch
- **Purpose**: Latest development builds
- **Retention**: 90 days
- **Artifacts**: Cross-platform binaries for testing

## Next Steps

To activate the automated builds:
1. Commit and push all changes to GitHub
2. Create a release tag (e.g., `git tag v1.0.0`) to trigger release builds
3. Development builds will automatically run on main branch pushes

## Manual Build Commands

For local testing:
```bash
# Build release configuration
dotnet build --configuration Release

# Publish self-contained for Windows
dotnet publish -c Release -r win-x64 --self-contained -o publish/win-x64

# Publish self-contained for macOS (Intel)
dotnet publish -c Release -r osx-x64 --self-contained -o publish/osx-x64

# Publish self-contained for macOS (ARM)
dotnet publish -c Release -r osx-arm64 --self-contained -o publish/osx-arm64

# Publish self-contained for Linux
dotnet publish -c Release -r linux-x64 --self-contained -o publish/linux-x64
```

Last Updated: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
