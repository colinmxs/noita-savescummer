namespace NoitaSaveScummer.Models;

public class Configuration
{
    public int BackupIntervalMinutes { get; set; }
    public int MaxBackupVersions { get; set; } = 5;
    public bool ResetPlayerLocationOnRestore { get; set; } = true;
    public double DefaultPlayerPositionX { get; set; } = 215.0;
    public double DefaultPlayerPositionY { get; set; } = -95.0;

    public bool IsValid() => 
        BackupIntervalMinutes > 0 && BackupIntervalMinutes <= 1440 &&
        MaxBackupVersions > 0 && MaxBackupVersions <= 100;
}
