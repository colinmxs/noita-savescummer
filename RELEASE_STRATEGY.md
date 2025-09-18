# Release Strategy

This document outlines the procedures for versioning, building, and releasing the Noita Save Scummer application.

## Versioning

This project uses Semantic Versioning (`MAJOR.MINOR.PATCH`).

- **MAJOR**: For breaking changes.
- **MINOR**: For new, backward-compatible features.
- **PATCH**: For backward-compatible bug fixes.

Release tags must be formatted as `vX.Y.Z`.

## GitHub Actions

The following GitHub Actions workflows are used for automation:

- **`ci.yml`**: Runs on every push and pull request to validate the build.
- **`release.yml`**: Creates official Windows releases when a version tag is pushed.
- **`dev-builds.yml`**: Generates development artifacts from the `main` branch for testing.

## Release Process

1.  Merge all changes into the `main` branch.
2.  Ensure the project builds locally: `dotnet build --configuration Release`
3.  Commit and push final changes: `git commit -m "Your commit message" && git push origin main`
4.  Create a new version tag: `git tag vX.Y.Z`
5.  Push the tag to the remote repository: `git push origin vX.Y.Z`

The `release.yml` workflow will automatically create a new GitHub release.

## Development Workflow

### Local Setup
```bash
git clone https://github.com/colinmxs/noita-savescummer.git
cd noita-savescummer
dotnet build
dotnet run
```

### Testing a Release Build
To create and test a self-contained Windows executable:
```bash
dotnet publish -c Release -r win-x64 --self-contained -o test-build
./test-build/noita-savescummer.exe
rm -rf test-build
```

### Project Principles
- **No Third-Party Dependencies**: The project uses only built-in .NET libraries.
- **Windows-Focused**: Development and testing are prioritized for Windows, Noita's primary platform.
- **Simplicity**: The application is designed to be simple and reliable.
