namespace NoitaSaveScummer.Models;

public sealed record BackupRequest(BackupKind Kind, bool NoitaRunning, bool SkipIfUnchanged, bool RequireActiveRun = true);

public enum BackupOutcome
{
    Created,
    SkippedUnchanged,
    SkippedNoActiveRun,
    Failed,
}

public sealed record BackupResult(BackupOutcome Outcome, string Message, BackupInfo? Backup = null);

public sealed record RestoreResult(BackupInfo Restored, BackupInfo? UndoBackup);

public sealed record CleanupResult(IReadOnlyList<string> Deleted, IReadOnlyList<string> Errors);
