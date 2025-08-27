namespace NoitaSaveScummer.Models;

public class BackupInfo
{
    public required string Name { get; set; }
    public required DateTime Timestamp { get; set; }
    public required string FolderPath { get; set; }

    public string FormattedTimestamp => Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
}
