namespace NoitaSaveScummer.Models;

public class Configuration
{
    public const int MinIntervalMinutes = 1;
    public const int MaxIntervalMinutes = 1440;
    public const int MinBackupVersions = 1;
    public const int MaxBackupVersionsLimit = 100;

    public int BackupIntervalMinutes { get; set; }
    public int MaxBackupVersions { get; set; } = 20;

    /// <summary>Default answer for the "reset player location?" question on player-only restore.</summary>
    public bool ResetPlayerLocationOnRestore { get; set; } = true;
    public double DefaultPlayerPositionX { get; set; } = 215.0;
    public double DefaultPlayerPositionY { get; set; } = -95.0;

    /// <summary>
    /// When true, a full restore keeps the current cross-run progress (persistent/, stats/, mod settings)
    /// and only rolls back the per-run state. When false, save00 becomes an exact copy of the backup.
    /// </summary>
    public bool KeepCurrentProgressOnRestore { get; set; } = true;

    /// <summary>Take a backup right after Noita exits (i.e. after Save &amp; Quit), when the save is fully consistent.</summary>
    public bool BackupOnNoitaExit { get; set; } = true;

    /// <summary>Skip timed backups when nothing in save00 changed since the previous backup.</summary>
    public bool SkipUnchangedBackups { get; set; } = true;

    public bool EnableGlobalHotkeys { get; set; } = true;
    public bool RelaunchNoitaAfterQuickLoad { get; set; } = true;
    public string NoitaLaunchCommand { get; set; } = "steam://rungameid/881100";

    /// <summary>How many automatic pre-restore (undo) backups to keep.</summary>
    public int MaxUndoBackups { get; set; } = 3;

    public bool IsValid() =>
        BackupIntervalMinutes is >= MinIntervalMinutes and <= MaxIntervalMinutes &&
        MaxBackupVersions is >= MinBackupVersions and <= MaxBackupVersionsLimit &&
        MaxUndoBackups is >= 1 and <= 20;

    public Configuration Clone() => (Configuration)MemberwiseClone();
}
