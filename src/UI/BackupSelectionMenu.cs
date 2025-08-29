using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.UI;

public static class BackupSelectionMenu
{
    public static string? SelectBackupToRestore(List<BackupInfo> backups)
    {
        return SelectBackup(backups, "Select Backup to Restore (Full Save)");
    }
    
    public static (string? backupName, bool resetLocation) SelectBackupToRestorePlayerOnly(List<BackupInfo> backups)
    {
        var backupName = SelectBackup(backups, "Select Backup to Restore (Player Only)");
        if (backupName == null)
            return (null, false);
            
        var resetLocation = AskForLocationReset();
        return (backupName, resetLocation);
    }
    
    private static bool AskForLocationReset()
    {
        Console.Clear();
        Console.WriteLine($"{IconProvider.Game} Reset Player Location?\n");
        Console.WriteLine("Do you want to reset the player to the starting location?");
        Console.WriteLine("This will move your character to the cave entrance.\n");
        Console.WriteLine("Y - Yes, reset to starting location");
        Console.WriteLine("N - No, keep current position");
        Console.WriteLine("ESC - Cancel restore");
        Console.Write("\nChoice: ");
        
        while (true)
        {
            var key = Console.ReadKey(true);
            
            switch (key.Key)
            {
                case ConsoleKey.Y:
                    Console.WriteLine("Y");
                    return true;
                case ConsoleKey.N:
                    Console.WriteLine("N");
                    return false;
                case ConsoleKey.Escape:
                    throw new OperationCanceledException("User cancelled restore");
                default:
                    // Invalid key, continue loop
                    break;
            }
        }
    }
    
    private static string? SelectBackup(List<BackupInfo> backups, string title)
    {
        if (backups.Count == 0)
        {
            return null;
        }

        Console.Clear();
        Console.WriteLine($"🎮 Noita Save Scummer - {title}\n");
        
        if (backups.Count == 1)
        {
            var backup = backups[0];
            Console.WriteLine($"Restoring backup from: {backup.FormattedTimestamp}");
            Console.WriteLine("\nPress any key to continue or ESC to cancel...");
            
            var key = Console.ReadKey(true);
            return key.Key == ConsoleKey.Escape ? null : backup.Name;
        }

        // Show numbered list of backups
        for (int i = 0; i < Math.Min(backups.Count, 9); i++)
        {
            Console.WriteLine($"{i + 1}. {backups[i].FormattedTimestamp}");
        }
        
        Console.WriteLine("\nEnter backup number (1-9) or ESC to cancel: ");
        
        while (true)
        {
            var keyInfo = Console.ReadKey(true);
            
            if (keyInfo.Key == ConsoleKey.Escape)
                return null;
                
            if (char.IsDigit(keyInfo.KeyChar))
            {
                var index = int.Parse(keyInfo.KeyChar.ToString()) - 1;
                if (index >= 0 && index < Math.Min(backups.Count, 9))
                {
                    return backups[index].Name;
                }
            }
        }
    }
}
