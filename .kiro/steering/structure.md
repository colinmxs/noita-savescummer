# Project Structure

```
Program.cs                      Entry: single-instance mutex (sync Main), Ctrl+C -> cancellation, wiring
noita-savescummer.csproj        Project + version metadata (Version 2.x)
src/
  AppInfo.cs                    Version string
  Models/
    Configuration.cs            Persisted settings, IsValid(), Clone()
    ApplicationState.cs         Timer/pause/status state (thread-safe status)
    BackupInfo.cs               BackupInfo, BackupMetadata (backup.json), BackupKind
    BackupResults.cs            BackupRequest/Result, RestoreResult, CleanupResult
  Services/
    NoitaSaveScummerApp.cs      Main loop, key/hotkey dispatch, background backups, restores
    BackupService.cs            Consistent snapshot backup, atomic swap restore, undo, retention, recovery
    SaveLayout.cs               What is per-run vs cross-run in save00
    SnapshotManifest.cs         File size/mtime manifest to detect writes during copy
    PlayerXml.cs                player.xml load/save + position edit (invariant culture)
    WandXml.cs                  List/extract/inject wands in inventory_quick
    WandTemplateStore.cs        wand_templates/*.xml
    PreservationService.cs      preserved_backups.json
    ConfigurationService.cs     config.json load/save (atomic)
    NoitaProcess.cs             INoitaProcess: detect / force-close / launch Noita
    GlobalHotkeys.cs            RegisterHotKey message loop (Windows)
    AppPaths.cs                 Default paths + --save-path/--backup-path/env overrides
    FileOps.cs                  Copy/delete/atomic-write helpers (include hidden files)
    JsonContext.cs              Source-generated JSON context
  UI/
    ConsoleRenderer.cs          Diff-based flicker-free renderer
    ConsoleDisplay.cs           Main screen model -> lines
    ListMenu.cs                 Generic paged picker
    BackupSelectionMenu.cs      Backup picker (ListMenu)
    Prompts.cs                  Noita-running / reset-location / confirm prompts
    ConfigurationPrompts.cs     First-run and settings prompts
    IconProvider.cs             Unicode/ASCII icons
tests/NoitaSaveScummer.Tests/   xUnit tests against temp save dirs (TempSave fixture)
.github/workflows/              ci (build+test+trimmed publish), release, dev-builds
```

## Layering
- Models: plain data, no I/O, no Console.
- Services: file system logic. Must not call `Console` directly; return results or throw so UI decides presentation. Depend on interfaces so they can be unit tested against a temp directory.
- UI: all `Console` access lives here.
- Program.cs: composition only.

## Conventions
- Namespaces: `NoitaSaveScummer.{Models,Services,UI}`, file-scoped.
- One public type per file (interfaces may sit with their single implementation).
- New tests go in `tests/NoitaSaveScummer.Tests/`; use `TempSave` to fake a save00. Services take an options/interface seam (`BackupServiceOptions`, `INoitaProcess`) for testing.
