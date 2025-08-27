using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.UI;

public static class ConsoleDisplay
{
    public static void ShowMainInterface(Configuration config, ApplicationState state, string noitaPath, string backupPath)
    {
        Console.SetCursorPosition(0, 0);
        Console.Clear();
        
        Console.WriteLine("🎮 Noita Save Scummer - Running");
        Console.WriteLine("═".PadRight(50, '═'));
        Console.WriteLine();
        
        Console.WriteLine($"📁 Noita Save: {noitaPath}");
        Console.WriteLine($"💾 Backup Location: {backupPath}");
        Console.WriteLine();
        
        Console.WriteLine($"⏱️  Backup Interval: {config.BackupIntervalMinutes} minute(s)");
        Console.WriteLine($"📦 Max Backup Versions: {config.MaxBackupVersions}");
        Console.WriteLine($"📅 Last Backup: {(state.LastBackupTime == DateTime.MinValue ? "Never" : state.LastBackupTime.ToString("HH:mm:ss"))}");
        
        if (state.IsPaused)
        {
            Console.WriteLine($"⏸️  Status: PAUSED - {state.PausedTimeRemaining.Minutes:D2}:{state.PausedTimeRemaining.Seconds:D2} remaining when resumed");
        }
        else
        {
            var timeRemaining = state.TimeUntilNextBackup;
            if (timeRemaining.TotalSeconds > 0)
            {
                Console.WriteLine($"⏳ Next Backup: {timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}");
            }
            else
            {
                Console.WriteLine("⏳ Next Backup: Creating backup...");
            }
        }
        
        Console.WriteLine();
        Console.WriteLine("🎯 Controls:");
        Console.WriteLine("   F9  - Select and restore backup");
        Console.WriteLine("   P   - Pause/Resume timer");
        Console.WriteLine("   C   - Configure settings");
        Console.WriteLine("   Q   - Quit application");
        Console.WriteLine();
        Console.WriteLine($"📝 Status: {(state.IsPaused ? "Paused" : "Running")}... (console window must be focused for hotkeys)");
    }

    public static void ShowMessage(string message, int line = 15)
    {
        Console.SetCursorPosition(0, line);
        Console.WriteLine(message.PadRight(Console.WindowWidth - 1));
    }

    public static void ClearMessage(int line = 15)
    {
        Console.SetCursorPosition(0, line);
        Console.WriteLine("".PadRight(Console.WindowWidth - 1));
    }

    public static void ShowInitializationMessage()
    {
        Console.Clear();
        Console.WriteLine("🎮 Noita Save Scummer - Initializing...\n");
    }

    public static void ShowShutdownMessage()
    {
        Console.Clear();
        Console.WriteLine("👋 Shutting down gracefully...");
    }
}
