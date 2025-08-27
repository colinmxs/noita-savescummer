using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.UI;

public static class ConfigurationPrompts
{
    public static async Task<Configuration> GetInitialConfiguration()
    {
        Console.Clear();
        Console.WriteLine("🎮 Noita Save Scummer - First Time Setup\n");
        Console.WriteLine("Welcome! Let's configure your backup settings.\n");

        var backupInterval = GetBackupInterval();
        var maxVersions = GetMaxVersions();

        Console.WriteLine($"\n✅ Configuration complete!");
        Console.WriteLine($"   Backup interval: {backupInterval} minute(s)");
        Console.WriteLine($"   Max backup versions: {maxVersions}");
        await Task.Delay(2000);
        
        return new Configuration 
        { 
            BackupIntervalMinutes = backupInterval,
            MaxBackupVersions = maxVersions
        };
    }

    public static async Task<Configuration> UpdateConfiguration(Configuration currentConfig)
    {
        Console.Clear();
        Console.WriteLine("🎮 Noita Save Scummer - Configuration\n");
        Console.WriteLine($"Current backup interval: {currentConfig.BackupIntervalMinutes} minute(s)");
        Console.WriteLine($"Current max backup versions: {currentConfig.MaxBackupVersions}\n");
        
        var newInterval = GetBackupInterval("Enter new backup interval in minutes (or press Enter to keep current): ", currentConfig.BackupIntervalMinutes);
        var newMaxVersions = GetMaxVersions("Enter max backup versions to keep (or press Enter to keep current): ", currentConfig.MaxBackupVersions);
        
        Console.WriteLine($"\n✅ Configuration updated!");
        Console.WriteLine($"   Backup interval: {newInterval} minute(s)");
        Console.WriteLine($"   Max backup versions: {newMaxVersions}");
        await Task.Delay(2000);
        
        return new Configuration 
        { 
            BackupIntervalMinutes = newInterval,
            MaxBackupVersions = newMaxVersions
        };
    }

    private static int GetBackupInterval(string prompt = "Enter backup interval in minutes (recommended: 5-30): ", int? defaultValue = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (defaultValue.HasValue && string.IsNullOrWhiteSpace(input))
            {
                return defaultValue.Value;
            }

            if (int.TryParse(input, out int minutes) && minutes > 0 && minutes <= 1440)
            {
                return minutes;
            }

            Console.WriteLine("❌ Please enter a valid number between 1 and 1440 minutes.\n");
        }
    }

    private static int GetMaxVersions(string prompt = "How many backup versions to keep? (recommended: 5-20): ", int? defaultValue = null)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (defaultValue.HasValue && string.IsNullOrWhiteSpace(input))
            {
                return defaultValue.Value;
            }

            if (int.TryParse(input, out int versions) && versions > 0 && versions <= 100)
            {
                return versions;
            }

            Console.WriteLine("❌ Please enter a valid number between 1 and 100.\n");
        }
    }
}
