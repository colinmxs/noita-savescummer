namespace NoitaSaveScummer.Models;

public class BackupInfo
{
    public required string Name { get; set; }
    public required DateTime Timestamp { get; set; }
    public required string FolderPath { get; set; }
    public bool IsPreserved { get; set; } = false;

    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
    public string DisplayName => IsPreserved ? $"{FormattedTimestamp} [PRESERVED]" : FormattedTimestamp;
}
