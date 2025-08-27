using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.UI;

public static class BackupSelectionMenu
{
    public static string? SelectBackupToRestore(List<BackupInfo> backups)
    {
        if (backups.Count == 0)
        {
            return null;
        }

        Console.Clear();
        Console.WriteLine("🎮 Noita Save Scummer - Select Backup to Restore\n");
        
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
