using System.Globalization;
using NoitaSaveScummer.Models;
using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.UI;

public sealed record MainScreenModel(
    Configuration Config,
    ApplicationState State,
    AppPaths Paths,
    IReadOnlyList<BackupInfo> Backups,
    string? HotkeyProblem,
    DateTime Now);

public static class ConsoleDisplay
{
    public const int RecentBackupsShown = 5;

    public static IReadOnlyList<string> BuildMainScreen(MainScreenModel m)
    {
        var lines = new List<string>();
        var state = m.State;
        var config = m.Config;

        lines.Add($"{IconProvider.Game} Noita Save Scummer v{AppInfo.Version}");
        lines.Add(new string(IconProvider.Separator[0], 60));
        lines.Add($"{IconProvider.Folder} Save:    {m.Paths.SavePath}");
        var preserved = m.Backups.Count(b => b.IsPreserved);
        lines.Add($"{IconProvider.Save} Backups: {m.Paths.BackupsPath}  ({m.Backups.Count} stored, {preserved} preserved)");
        lines.Add($"{IconProvider.Target} Noita:   {(state.IsNoitaRunning ? "RUNNING" : "not running")}");

        string next;
        if (state.IsPaused)
            next = $"PAUSED ({ApplicationState.FormatCountdown(state.PausedTimeRemaining)} left when resumed)";
        else
            next = ApplicationState.FormatCountdown(state.TimeUntilNextBackup(m.Now));
        lines.Add($"{IconProvider.Timer} Every {config.BackupIntervalMinutes} min, keep {config.MaxBackupVersions}  |  Next backup: {next}");

        var last = state.LastBackupTime == DateTime.MinValue
            ? "none this session"
            : state.LastBackupTime.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
        lines.Add($"{IconProvider.Calendar} Last backup: {last}");
        lines.Add(string.Empty);

        lines.Add("Recent backups:");
        if (m.Backups.Count == 0)
            lines.Add("   (none yet)");
        foreach (var backup in m.Backups.Take(RecentBackupsShown))
            lines.Add($"   {backup.DisplayName}");
        lines.Add(string.Empty);

        lines.Add("Controls (this window):");
        lines.Add("   F9 Full restore        F8 Player-only restore   F7 Preserve/unpreserve");
        lines.Add("   U  Undo last restore   B  Back up now           P  Pause/resume timer");
        lines.Add("   F6 Wand tools          C  Settings              Q  Quit");
        if (m.HotkeyProblem is null)
            lines.Add($"Global (in-game): {HotkeyBindings.LabelFor(HotkeyAction.QuickSave)} quick-save, " +
                      $"{HotkeyBindings.LabelFor(HotkeyAction.QuickLoad)} quick-load (closes Noita, restores newest, relaunches)");
        else
            lines.Add($"Global hotkeys: {m.HotkeyProblem}");
        lines.Add(string.Empty);

        var status = state.GetStatus(m.Now);
        lines.Add($"{IconProvider.Document} {(string.IsNullOrEmpty(status) ? (state.IsPaused ? "Paused" : "Running") : status)}");
        return lines;
    }

    public static void ShowInitializationMessage()
    {
        Console.Clear();
        Console.WriteLine($"{IconProvider.Game} Noita Save Scummer - Initializing...\n");
    }

    public static void ShowShutdownMessage()
    {
        try
        {
            Console.CursorVisible = true;
        }
        catch (Exception ex) when (ex is IOException or PlatformNotSupportedException)
        {
        }
        Console.Clear();
        Console.WriteLine($"{IconProvider.Wave} Shut down. Your backups are in the backups folder.");
    }
}
