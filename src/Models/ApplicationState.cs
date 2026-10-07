namespace NoitaSaveScummer.Models;

/// <summary>In-memory runtime state. Status/busy members are thread-safe because background backups update them.</summary>
public class ApplicationState
{
    private readonly object _gate = new();
    private string _status = string.Empty;
    private DateTime _statusExpires = DateTime.MaxValue;
    private string? _busy;

    public DateTime LastBackupTime { get; set; } = DateTime.MinValue;
    public DateTime NextBackupTime { get; private set; }
    public bool IsPaused { get; private set; }
    public TimeSpan PausedTimeRemaining { get; private set; }
    public bool IsNoitaRunning { get; set; }

    public TimeSpan TimeUntilNextBackup(DateTime now) => NextBackupTime - now;

    public bool IsBackupDue(DateTime now) => !IsPaused && now >= NextBackupTime;

    public void ScheduleNext(int intervalMinutes, DateTime now)
    {
        if (IsPaused)
            PausedTimeRemaining = TimeSpan.FromMinutes(intervalMinutes);
        else
            NextBackupTime = now.AddMinutes(intervalMinutes);
    }

    public void Pause(DateTime now)
    {
        if (IsPaused) return;
        var remaining = TimeUntilNextBackup(now);
        PausedTimeRemaining = remaining < TimeSpan.Zero ? TimeSpan.Zero : remaining;
        IsPaused = true;
    }

    public void Resume(DateTime now)
    {
        if (!IsPaused) return;
        NextBackupTime = now.Add(PausedTimeRemaining);
        PausedTimeRemaining = TimeSpan.Zero;
        IsPaused = false;
    }

    public void SetStatus(string message, TimeSpan? duration = null)
    {
        lock (_gate)
        {
            _status = message;
            _statusExpires = duration is { } d ? DateTime.Now + d : DateTime.MaxValue;
        }
    }

    public string GetStatus(DateTime now)
    {
        lock (_gate)
        {
            if (_busy is not null) return _busy;
            return now <= _statusExpires ? _status : string.Empty;
        }
    }

    public string? Busy
    {
        get { lock (_gate) return _busy; }
        set { lock (_gate) _busy = value; }
    }

    /// <summary>Formats a countdown as mm:ss, or hh:mm:ss when an hour or more remains.</summary>
    public static string FormatCountdown(TimeSpan value)
    {
        if (value < TimeSpan.Zero) value = TimeSpan.Zero;
        return value.TotalHours >= 1
            ? $"{(int)value.TotalHours:D2}:{value.Minutes:D2}:{value.Seconds:D2}"
            : $"{value.Minutes:D2}:{value.Seconds:D2}";
    }
}
