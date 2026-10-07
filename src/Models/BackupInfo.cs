using System.Globalization;
using System.Text.Json.Serialization;

namespace NoitaSaveScummer.Models;

[JsonConverter(typeof(JsonStringEnumConverter<BackupKind>))]
public enum BackupKind
{
    /// <summary>Created by the interval timer.</summary>
    Timed,
    /// <summary>Created on request (B key or quick-save hotkey).</summary>
    Manual,
    /// <summary>Created right after Noita exited (Save &amp; Quit).</summary>
    OnExit,
    /// <summary>Automatic safety copy of save00 taken before a restore overwrote it.</summary>
    PreRestore,
    /// <summary>Created by v1.x (no metadata).</summary>
    Legacy,
}

/// <summary>Written as backup.json inside a backup folder. Its presence marks the backup as complete.</summary>
public class BackupMetadata
{
    public int FormatVersion { get; set; } = 1;
    public DateTime CreatedUtc { get; set; }
    public BackupKind Kind { get; set; }
    public bool NoitaWasRunning { get; set; }
    public int FileCount { get; set; }
    public long TotalBytes { get; set; }
    public string ManifestHash { get; set; } = string.Empty;
}

public class BackupInfo
{
    public required string Name { get; init; }
    public required DateTime Timestamp { get; init; }
    public int Sequence { get; init; }
    public required string FolderPath { get; init; }
    public bool IsPreserved { get; init; }
    public BackupMetadata? Metadata { get; init; }

    public BackupKind Kind => Metadata?.Kind ?? BackupKind.Legacy;

    /// <summary>True when the backup was taken while Noita was closed, so every file comes from one consistent save.</summary>
    public bool IsClean => Metadata is { NoitaWasRunning: false };

    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);

    public string Tags
    {
        get
        {
            var tags = new List<string>();
            switch (Kind)
            {
                case BackupKind.Manual: tags.Add("[MANUAL]"); break;
                case BackupKind.OnExit: tags.Add("[ON-EXIT]"); break;
                case BackupKind.PreRestore: tags.Add("[UNDO]"); break;
                case BackupKind.Legacy: tags.Add("[LEGACY]"); break;
            }
            if (Metadata is not null) tags.Add(IsClean ? "[CLEAN]" : "[LIVE]");
            if (IsPreserved) tags.Add("[PRESERVED]");
            return string.Join(' ', tags);
        }
    }

    public string DisplayName => $"{FormattedTimestamp}  {Tags}".TrimEnd();
}
