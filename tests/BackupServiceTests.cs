using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.Tests.Services;

[TestClass]
public class BackupServiceTests
{
    private string _testSourcePath = "";
    private string _testBackupsPath = "";

    [TestInitialize]
    public void Setup()
    {
        var tempBase = Path.Combine(Path.GetTempPath(), $"backup_test_{Guid.NewGuid()}");
        _testSourcePath = Path.Combine(tempBase, "source");
        _testBackupsPath = Path.Combine(tempBase, "backups");

        Directory.CreateDirectory(_testSourcePath);
        Directory.CreateDirectory(_testBackupsPath);

        // Create test files
        File.WriteAllText(Path.Combine(_testSourcePath, "test1.txt"), "test content 1");
        File.WriteAllText(Path.Combine(_testSourcePath, "test2.txt"), "test content 2");
        
        var subDir = Path.Combine(_testSourcePath, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "test3.txt"), "test content 3");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(Path.GetDirectoryName(_testSourcePath)!))
        {
            Directory.Delete(Path.GetDirectoryName(_testSourcePath)!, true);
        }
    }

    [TestMethod]
    public async Task CreateBackupAsync_ValidSource_CreatesTimestampedBackup()
    {
        // Arrange
        var service = new BackupService(_testSourcePath, _testBackupsPath);

        // Act
        await service.CreateBackupAsync();

        // Assert
        var backups = service.GetAvailableBackups();
        Assert.AreEqual(1, backups.Count);
        
        var backup = backups[0];
        Assert.IsTrue(DateTime.TryParseExact(backup.Name, "yyyy-MM-dd_HH-mm-ss", null, 
            System.Globalization.DateTimeStyles.None, out _));
        
        // Verify files were copied
        var backupFiles = Directory.GetFiles(backup.FolderPath, "*", SearchOption.AllDirectories);
        Assert.AreEqual(3, backupFiles.Length);
    }

    [TestMethod]
    public async Task RestoreBackupAsync_ValidBackup_RestoresFiles()
    {
        // Arrange
        var service = new BackupService(_testSourcePath, _testBackupsPath);
        await service.CreateBackupAsync();
        
        var backups = service.GetAvailableBackups();
        var backupName = backups[0].Name;
        
        // Modify source files
        File.WriteAllText(Path.Combine(_testSourcePath, "test1.txt"), "modified content");
        File.WriteAllText(Path.Combine(_testSourcePath, "new_file.txt"), "new content");

        // Act
        await service.RestoreBackupAsync(backupName);

        // Assert
        var restoredContent = File.ReadAllText(Path.Combine(_testSourcePath, "test1.txt"));
        Assert.AreEqual("test content 1", restoredContent);
        Assert.IsFalse(File.Exists(Path.Combine(_testSourcePath, "new_file.txt")));
    }

    [TestMethod]
    public async Task CleanupOldBackupsAsync_ExceedsLimit_RemovesOldBackups()
    {
        // Arrange
        var service = new BackupService(_testSourcePath, _testBackupsPath);
        
        // Create multiple backups
        for (int i = 0; i < 5; i++)
        {
            await service.CreateBackupAsync();
            await Task.Delay(1100); // Ensure different timestamps
        }

        // Act
        await service.CleanupOldBackupsAsync(3);

        // Assert
        var remainingBackups = service.GetAvailableBackups();
        Assert.AreEqual(3, remainingBackups.Count);
    }

    [TestMethod]
    public async Task RestoreBackupAsync_NonExistentBackup_ThrowsException()
    {
        // Arrange
        var service = new BackupService(_testSourcePath, _testBackupsPath);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.RestoreBackupAsync("nonexistent-backup"));
    }

    [TestMethod]
    public void GetAvailableBackups_NoBackups_ReturnsEmptyList()
    {
        // Arrange
        var service = new BackupService(_testSourcePath, _testBackupsPath);

        // Act
        var result = service.GetAvailableBackups();

        // Assert
        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAvailableBackups_ReturnsOrderedByTimestamp()
    {
        // Arrange
        var service = new BackupService(_testSourcePath, _testBackupsPath);
        
        // Manually create backup directories with specific timestamps
        var timestamps = new[]
        {
            "2024-01-01_10-00-00",
            "2024-01-01_12-00-00",
            "2024-01-01_11-00-00"
        };
        
        foreach (var timestamp in timestamps)
        {
            Directory.CreateDirectory(Path.Combine(_testBackupsPath, timestamp));
        }

        // Act
        var result = service.GetAvailableBackups();

        // Assert
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("2024-01-01_12-00-00", result[0].Name); // Most recent first
        Assert.AreEqual("2024-01-01_11-00-00", result[1].Name);
        Assert.AreEqual("2024-01-01_10-00-00", result[2].Name);
    }
}
