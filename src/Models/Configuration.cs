namespace NoitaSaveScummer.Models;

public class Configuration
{
    public int BackupIntervalMinutes { get; set; }
    public int MaxBackupVersions { get; set; } = 5;

    public bool IsValid() => 
        BackupIntervalMinutes > 0 && BackupIntervalMinutes <= 1440 &&
        MaxBackupVersions > 0 && MaxBackupVersions <= 100;
}
