# Release Strategy & Repository Operations Guide

This document outlines the release strategy, versioning scheme, and operational procedures for the Noita Save Scummer repository. This guide is intended for maintainers and AI assistants working on this project.

> **📋 Companion Documents:**
> - [`BUILD_STATUS.md`](./BUILD_STATUS.md) - Current technical status and build health
> - [`VERSION.md`](./VERSION.md) - Release history and changelog
> - [`README.md`](./README.md) - User-facing documentation and features

## 📋 Table of Contents

- [Repository Structure](#repository-structure)
- [Versioning Strategy](#versioning-strategy)
- [GitHub Actions Workflows](#github-actions-workflows)
- [Release Process](#release-process)
- [Hotfix Process](#hotfix-process)
- [Development Workflow](#development-workflow)
- [Common Operations](#common-operations)
- [Troubleshooting](#troubleshooting)

## 🏗️ Repository Structure

```
noita-savescummer/
├── .github/workflows/          # CI/CD automation
│   ├── ci.yml                 # Continuous Integration
│   ├── release.yml            # Release builds
│   └── dev-builds.yml         # Development artifacts
├── src/                       # Source code (clean architecture)
│   ├── Models/               # Data models
│   ├── Services/             # Business logic
│   └── UI/                   # User interface
├── Program.cs                # Application entry point
├── README.md                 # User documentation
├── VERSION.md                # Version history
├── BUILD_STATUS.md           # Build status tracking
└── RELEASE_STRATEGY.md       # This document
```

## 🔢 Versioning Strategy

### Semantic Versioning (SemVer)
We follow semantic versioning: `MAJOR.MINOR.PATCH`

- **MAJOR** (x.0.0): Breaking changes, major feature overhauls
- **MINOR** (0.x.0): New features, significant enhancements (backward compatible)
- **PATCH** (0.0.x): Bug fixes, small improvements (backward compatible)

### Version Examples
- `v1.0.0`: Initial stable release
- `v1.0.1`: Bug fix (JSON serialization issue)
- `v1.0.2`: Bug fix (icon display issue)
- `v1.1.0`: New feature (e.g., backup scheduling)
- `v2.0.0`: Breaking change (e.g., configuration format change)

### Tagging Convention
- **Format**: `vX.Y.Z` (e.g., `v1.0.2`)
- **Release Tags**: Trigger production builds and GitHub releases
- **Pre-release Tags**: Use `-alpha`, `-beta`, `-rc` suffixes (e.g., `v1.1.0-beta.1`)

## 🚀 GitHub Actions Workflows

### 1. Continuous Integration (`ci.yml`)
- **Trigger**: Push to any branch, all pull requests
- **Purpose**: Validate code quality and Windows builds
- **Platform**: Windows 10/11 (64-bit)
- **Actions**: Build, test, validate

### 2. Release Builds (`release.yml`)
- **Trigger**: Git tags matching `v*.*.*`
- **Purpose**: Create official releases with Windows executable
- **Artifacts**: Self-contained Windows executable
- **Distribution**: GitHub Releases page

### 3. Development Builds (`dev-builds.yml`)
- **Trigger**: Push to `main` branch
- **Purpose**: Latest development artifacts for testing
- **Retention**: 30 days
- **Access**: GitHub Actions artifacts

## 📦 Release Process

### Standard Release Flow

1. **Development & Testing**
   ```bash
   # Work on features/fixes in branches or directly on main
   git checkout main
   git pull origin main
   
   # Make changes and test locally
   dotnet build --configuration Release
   dotnet test  # If tests exist
   ```

2. **Commit Changes**
   ```bash
   git add .
   git commit -m "descriptive commit message"
   git push origin main
   ```

3. **Create Release Tag**
   ```bash
   # Determine version based on changes
   git tag v1.X.Y
   git push origin v1.X.Y
   ```

4. **Monitor Build**
   - Check GitHub Actions: `https://github.com/colinmxs/noita-savescummer/actions`
   - Verify release creation: `https://github.com/colinmxs/noita-savescummer/releases`

### Version Decision Matrix

| Change Type | Version Increment | Example |
|-------------|------------------|---------|
| Critical bug fix | PATCH | v1.0.1 → v1.0.2 |
| Multiple bug fixes | PATCH | v1.0.1 → v1.0.2 |
| New feature (compatible) | MINOR | v1.0.2 → v1.1.0 |
| Breaking change | MAJOR | v1.1.0 → v2.0.0 |
| Documentation only | No tag needed | Just push to main |

## 🔥 Hotfix Process

For critical production issues:

1. **Immediate Fix**
   ```bash
   # Fix the issue on main branch
   git checkout main
   git pull origin main
   # Make critical fix
   git add .
   git commit -m "hotfix: critical issue description"
   git push origin main
   ```

2. **Emergency Release**
   ```bash
   # Increment patch version
   git tag v1.X.Y
   git push origin v1.X.Y
   ```

3. **Update Documentation**
   - Update VERSION.md with hotfix details
   - Notify users in README if necessary

## 🛠️ Development Workflow

### Local Development
```bash
# Clone and setup
git clone https://github.com/colinmxs/noita-savescummer.git
cd noita-savescummer

# Build and test
dotnet restore
dotnet build
dotnet run  # Test locally

# Test Windows publishing
dotnet publish -c Release -r win-x64 --self-contained -o test-publish
./test-publish/noita-savescummer.exe  # Test executable
rm -rf test-publish  # Clean up
```

### Code Quality Standards
- Follow .NET coding conventions
- Use meaningful commit messages
- Test major changes before tagging
- Maintain clean architecture separation
- NO third-party NuGet packages (built-in .NET only)

## 🔧 Common Operations

### Creating a New Release
```bash
# Example: Adding a new feature
git add .
git commit -m "feat: add backup compression feature

- Implement compression for backup files
- Add compression settings to configuration
- Update UI to show compression status
- Maintain backward compatibility with existing backups"

git push origin main
git tag v1.1.0  # Minor version for new feature
git push origin v1.1.0
```

### Emergency Hotfix
```bash
# Example: Critical bug fix
git add .
git commit -m "hotfix: fix save corruption on Windows

- Fix file locking issue causing save corruption
- Add proper error handling for file operations
- Improve file system interaction reliability"

git push origin main
git tag v1.0.3  # Patch version for bug fix
git push origin v1.0.3
```

### Checking Build Status
```bash
# View recent tags
git tag --sort=-version:refname | head -10

# Check if release workflow completed
# Visit: https://github.com/colinmxs/noita-savescummer/actions

# Verify release artifacts
# Visit: https://github.com/colinmxs/noita-savescummer/releases
```

## 🐛 Troubleshooting

### Build Failures
1. **JSON Serialization Issues**: Ensure JsonContext is updated for new models
2. **Windows Build Issues**: Check CI logs for Windows-specific problems
3. **Dependency Issues**: Verify no third-party packages are added

### Release Issues
1. **Tag Not Triggering Release**: Check tag format matches `v*.*.*`
2. **Artifacts Missing**: Check release.yml workflow logs
3. **Download Issues**: Verify GitHub release was created successfully

### Common Fixes
```bash
# Remove bad tag
git tag -d v1.0.X
git push origin :refs/tags/v1.0.X

# Fix and retag
git tag v1.0.Y
git push origin v1.0.Y

# Force clean build
dotnet clean
rm -rf bin obj
dotnet restore
dotnet build --configuration Release
```

## 📚 Additional Resources

- **Current Build Status**: [`BUILD_STATUS.md`](./BUILD_STATUS.md) - Real-time status and technical configuration
- **GitHub Actions**: `.github/workflows/` directory - Workflow definitions
- **Version History**: [`VERSION.md`](./VERSION.md) - Detailed release notes and changelog
- **User Documentation**: [`README.md`](./README.md) - End-user instructions and features
- **GitHub Copilot Instructions**: [`.github/copilot-instructions.md`](./.github/copilot-instructions.md) - AI assistant guidelines

## 🎯 Key Principles

1. **Automation First**: Let GitHub Actions handle builds and releases
2. **Semantic Versioning**: Always use meaningful version numbers
3. **Windows Focus**: Optimized for Windows 10/11 (Noita's platform)
4. **Self-Contained**: Executables should not require .NET installation
5. **Documentation**: Keep all documentation up to date with changes
6. **Simplicity**: Maintain the "no third-party dependencies" rule

---

**Last Updated**: August 31, 2025  
**Current Version**: v1.2.0  
**Next Planned Version**: TBD based on feature requests and issues
