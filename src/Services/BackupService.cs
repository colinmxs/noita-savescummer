using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Services;

public interface IBackupService
{
    Task CreateBackupAsync();
    Task RestoreBackupAsync(string backupName);
    List<BackupInfo> GetAvailableBackups();
    Task CleanupOldBackupsAsync(int maxVersions);
}

public class BackupService : IBackupService
{
    private readonly string _savePath;
    private readonly string _backupsPath;

    public BackupService(string savePath, string backupsPath)
    {
        _savePath = savePath;
        _backupsPath = backupsPath;
    }

    public async Task CreateBackupAsync()
    {
        try
        {
            Directory.CreateDirectory(_backupsPath);

            var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            var backupFolderPath = Path.Combine(_backupsPath, timestamp);
            
            Directory.CreateDirectory(backupFolderPath);
            await CopyDirectoryAsync(_savePath, backupFolderPath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create backup: {ex.Message}", ex);
        }
    }

    public async Task RestoreBackupAsync(string backupName)
    {
        try
        {
            var backupPath = Path.Combine(_backupsPath, backupName);
            
            if (!Directory.Exists(backupPath))
            {
                throw new InvalidOperationException($"Backup '{backupName}' not found");
            }

            if (Directory.Exists(_savePath))
            {
                Directory.Delete(_savePath, true);
            }

            Directory.CreateDirectory(_savePath);
            await CopyDirectoryAsync(backupPath, _savePath);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to restore backup: {ex.Message}", ex);
        }
    }

    public List<BackupInfo> GetAvailableBackups()
    {
        try
        {
            if (!Directory.Exists(_backupsPath))
                return new List<BackupInfo>();

            return Directory.GetDirectories(_backupsPath)
                .Select(dir => Path.GetFileName(dir))
                .Where(name => DateTime.TryParseExact(name, "yyyy-MM-dd_HH-mm-ss", null, 
                    System.Globalization.DateTimeStyles.None, out var timestamp))
                .Select(name => new BackupInfo
                {
                    Name = name,
                    Timestamp = DateTime.ParseExact(name, "yyyy-MM-dd_HH-mm-ss", null),
                    FolderPath = Path.Combine(_backupsPath, name)
                })
                .OrderByDescending(b => b.Timestamp)
                .ToList();
        }
        catch
        {
            return new List<BackupInfo>();
        }
    }

    public Task CleanupOldBackupsAsync(int maxVersions)
    {
        try
        {
            var backups = GetAvailableBackups();

            if (backups.Count > maxVersions)
            {
                var backupsToDelete = backups.Skip(maxVersions);
                
                foreach (var backup in backupsToDelete)
                {
                    if (Directory.Exists(backup.FolderPath))
                    {
                        Directory.Delete(backup.FolderPath, true);
                    }
                }
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
        
        return Task.CompletedTask;
    }

    private static async Task CopyDirectoryAsync(string sourceDir, string destDir)
    {
        var dir = new DirectoryInfo(sourceDir);
        
        if (!dir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");
        }

        Directory.CreateDirectory(destDir);

        foreach (var file in dir.GetFiles())
        {
            var destFile = Path.Combine(destDir, file.Name);
            await Task.Run(() => file.CopyTo(destFile, true));
        }

        foreach (var subDir in dir.GetDirectories())
        {
            var destSubDir = Path.Combine(destDir, subDir.Name);
            await CopyDirectoryAsync(subDir.FullName, destSubDir);
        }
    }
}
