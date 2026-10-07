using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.UI;

public static class ConfigurationPrompts
{
    public static Configuration GetInitialConfiguration(Configuration defaults)
    {
        Console.Clear();
        Console.WriteLine($"{IconProvider.Game} Noita Save Scummer - First Time Setup\n");
        Console.WriteLine("Welcome! Press Enter to accept a default.\n");
        return Prompt(defaults.Clone(), firstRun: true);
    }

    public static Configuration UpdateConfiguration(Configuration current)
    {
        Console.Clear();
        Console.WriteLine($"{IconProvider.Game} Noita Save Scummer - Settings\n");
        Console.WriteLine("Press Enter to keep the current value.\n");
        return Prompt(current.Clone(), firstRun: false);
    }

    private static Configuration Prompt(Configuration config, bool firstRun)
    {
        int? intervalDefault = firstRun && config.BackupIntervalMinutes <= 0 ? 5 : config.BackupIntervalMinutes;
        config.BackupIntervalMinutes = ReadInt("Backup interval in minutes", intervalDefault,
            Configuration.MinIntervalMinutes, Configuration.MaxIntervalMinutes);
        config.MaxBackupVersions = ReadInt("Backups to keep (preserved ones don't count)", config.MaxBackupVersions,
            Configuration.MinBackupVersions, Configuration.MaxBackupVersionsLimit);
        config.BackupOnNoitaExit = ReadBool("Back up automatically after Noita exits (Save & Quit = cleanest backup)", config.BackupOnNoitaExit);
        config.SkipUnchangedBackups = ReadBool("Skip timed backups when the save hasn't changed", config.SkipUnchangedBackups);
        config.KeepCurrentProgressOnRestore = ReadBool("Keep unlocks/stats/progress made since the backup when restoring", config.KeepCurrentProgressOnRestore);
        config.EnableGlobalHotkeys = ReadBool("Enable in-game hotkeys Ctrl+Alt+F5 / Ctrl+Alt+F9", config.EnableGlobalHotkeys);
        if (config.EnableGlobalHotkeys)
            config.RelaunchNoitaAfterQuickLoad = ReadBool("Relaunch Noita (via Steam) after quick-load", config.RelaunchNoitaAfterQuickLoad);

        Console.WriteLine($"\n{IconProvider.Success} Settings saved. Press any key...");
        Console.ReadKey(intercept: true);
        return config;
    }

    private static int ReadInt(string label, int? defaultValue, int min, int max)
    {
        while (true)
        {
            Console.Write(defaultValue is { } d ? $"{label} [{d}]: " : $"{label}: ");
            var input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input) && defaultValue is { } value && value >= min && value <= max)
                return value;
            if (int.TryParse(input, out var parsed) && parsed >= min && parsed <= max)
                return parsed;
            Console.WriteLine($"{IconProvider.Error} Enter a whole number from {min} to {max}.");
        }
    }

    private static bool ReadBool(string label, bool defaultValue)
    {
        while (true)
        {
            Console.Write($"{label} [{(defaultValue ? "Y/n" : "y/N")}]: ");
            var input = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(input)) return defaultValue;
            if (input.Equals("y", StringComparison.OrdinalIgnoreCase) || input.Equals("yes", StringComparison.OrdinalIgnoreCase)) return true;
            if (input.Equals("n", StringComparison.OrdinalIgnoreCase) || input.Equals("no", StringComparison.OrdinalIgnoreCase)) return false;
            Console.WriteLine($"{IconProvider.Error} Answer y or n.");
        }
    }
}
