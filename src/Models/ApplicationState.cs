namespace NoitaSaveScummer.Models;

public class ApplicationState
{
    public DateTime LastBackupTime { get; set; } = DateTime.MinValue;
    public DateTime NextBackupTime { get; set; }
    public bool IsPaused { get; set; }
    public TimeSpan PausedTimeRemaining { get; set; }

    public TimeSpan TimeUntilNextBackup => NextBackupTime - DateTime.Now;

    public void UpdateNextBackupTime(int intervalMinutes)
    {
        NextBackupTime = DateTime.Now.AddMinutes(intervalMinutes);
    }

    public void Pause()
    {
        if (!IsPaused)
        {
            PausedTimeRemaining = TimeUntilNextBackup;
            if (PausedTimeRemaining < TimeSpan.Zero)
                PausedTimeRemaining = TimeSpan.Zero;
            IsPaused = true;
        }
    }

    public void Resume()
    {
        if (IsPaused)
        {
            NextBackupTime = DateTime.Now.Add(PausedTimeRemaining);
            IsPaused = false;
            PausedTimeRemaining = TimeSpan.Zero;
        }
    }
}
