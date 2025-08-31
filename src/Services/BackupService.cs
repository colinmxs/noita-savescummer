using NoitaSaveScummer.Models;
using System.Xml;

namespace NoitaSaveScummer.Services;

public interface IBackupService
{
    Task CreateBackupAsync();
    Task RestoreBackupAsync(string backupName);
    Task RestorePlayerOnlyAsync(string backupName, bool resetLocation = false, double posX = 215.0, double posY = -95.0);
    List<BackupInfo> GetAvailableBackups();
    Task CleanupOldBackupsAsync(int maxVersions);
    void ToggleBackupPreservation(string backupName);
    bool IsBackupPreserved(string backupName);
}

public class BackupService : IBackupService
{
    private readonly string _savePath;
    private readonly string _backupsPath;
    private readonly PreservationService _preservationService;

    public BackupService(string savePath, string backupsPath)
    {
        _savePath = savePath;
        _backupsPath = backupsPath;
        _preservationService = new PreservationService(_backupsPath);
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

    public async Task RestorePlayerOnlyAsync(string backupName, bool resetLocation = false, double posX = 215.0, double posY = -95.0)
    {
        try
        {
            var backupPath = Path.Combine(_backupsPath, backupName);
            
            if (!Directory.Exists(backupPath))
            {
                throw new InvalidOperationException($"Backup '{backupName}' not found");
            }

            var playerXmlBackupPath = Path.Combine(backupPath, "player.xml");
            if (!File.Exists(playerXmlBackupPath))
            {
                throw new InvalidOperationException($"player.xml not found in backup '{backupName}'");
            }

            var playerXmlDestPath = Path.Combine(_savePath, "player.xml");
            
            // Ensure the save directory exists
            Directory.CreateDirectory(_savePath);
            
            if (resetLocation)
            {
                // Read the XML, modify the position, then save
                var xmlDoc = new XmlDocument();
                await Task.Run(() => xmlDoc.Load(playerXmlBackupPath));
                
                var transformNode = xmlDoc.SelectSingleNode("//Entity/_Transform");
                if (transformNode != null)
                {
                    var posXAttr = transformNode.Attributes?["position.x"];
                    var posYAttr = transformNode.Attributes?["position.y"];
                    
                    if (posXAttr != null) posXAttr.Value = posX.ToString("F6");
                    if (posYAttr != null) posYAttr.Value = posY.ToString("F6");
                }
                
                await Task.Run(() => xmlDoc.Save(playerXmlDestPath));
            }
            else
            {
                // Copy the file as-is
                await Task.Run(() => File.Copy(playerXmlBackupPath, playerXmlDestPath, true));
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to restore player.xml: {ex.Message}", ex);
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
                    FolderPath = Path.Combine(_backupsPath, name),
                    IsPreserved = _preservationService.IsPreserved(Path.Combine(_backupsPath, name))
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
                var backupsToDelete = backups
                    .Where(b => !b.IsPreserved)  // Don't delete preserved backups
                    .Skip(maxVersions);
                
                foreach (var backup in backupsToDelete)
                {
                    if (Directory.Exists(backup.FolderPath))
                    {
                        Directory.Delete(backup.FolderPath, true);
                    }
                }
                
                // Clean up preservation metadata for deleted backups
                var remainingBackups = backups.Where(b => Directory.Exists(b.FolderPath));
                _preservationService.CleanupDeletedBackups(remainingBackups.Select(b => b.FolderPath));
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

    public void ToggleBackupPreservation(string backupName)
    {
        var backupPath = Path.Combine(_backupsPath, backupName);
        _preservationService.TogglePreservation(backupPath);
    }

    public bool IsBackupPreserved(string backupName)
    {
        var backupPath = Path.Combine(_backupsPath, backupName);
        return _preservationService.IsPreserved(backupPath);
    }
}
