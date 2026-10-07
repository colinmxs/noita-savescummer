# Tech Stack and Conventions

## Stack
- C# / .NET 9.0 console app (`noita-savescummer.csproj`), `Nullable` and `ImplicitUsings` enabled
- Windows-only target (`win-x64`), shipped as a self-contained single-file exe
- No third-party NuGet packages in the app. Test projects may use standard test packages (xUnit) with pinned versions.

## Commands
```bash
dotnet build --configuration Release
dotnet run
dotnet test                       # xUnit, tests/NoitaSaveScummer.Tests
dotnet publish noita-savescummer.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:PublishTrimmed=true -p:TrimMode=partial -o test-build
```
The release pipeline publishes with `PublishTrimmed=true`. Verify behavior against a trimmed publish, not just `dotnet run`. Debug builds hide trimming bugs. No local SDK? Use Docker: `docker run --rm -v "$PWD":/src -w /src mcr.microsoft.com/dotnet/sdk:9.0 dotnet test` (DOCKER_HOST is preset; bind mounts work under /mnt/wsl/code). Delete bin/obj afterwards; they are root-owned.

## Hard rules
- JSON: always serialize through `NoitaSaveScummerJsonContext` (src/Services/JsonContext.cs). Never use `new JsonSerializerOptions()` or generic reflection overloads; trimmed builds disable reflection-based serialization and throw at runtime. Register new types with `[JsonSerializable]`.
- Culture: use `CultureInfo.InvariantCulture` for every number/date format or parse that touches files or folder names (player.xml coordinates, backup timestamps).
- Console icons: use `IconProvider`, never hardcode emoji.
- Paths: `Path.Combine` only; no hardcoded separators.
- File writes to the save directory: write to a temp location and move/rename into place; never delete before the replacement copy has succeeded.
- `TreatWarningsAsErrors` is on; `JsonSerializerIsReflectionEnabledByDefault=false` in app and tests.
- Do not swallow exceptions silently in data paths (backup, restore, preservation, cleanup). Surface them to the UI.
- Long-running I/O must not block the UI loop.

## CI / Release
- `.github/workflows/ci.yml`: build + `dotnet test` (if `tests/` exists) + publish smoke test on windows-latest
- `release.yml`: on `vX.Y.Z` tag, publish and attach `noita-savescummer-windows-x64.zip`
- `dev-builds.yml`: main-branch prerelease `dev-latest`
- Semantic versioning. Bump `AssemblyVersion`/`FileVersion`/`InformationalVersion` in the csproj and add a VERSION.md entry for every release.
