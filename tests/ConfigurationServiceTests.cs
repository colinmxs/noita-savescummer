using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoitaSaveScummer.Models;
using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.Tests.Services;

[TestClass]
public class ConfigurationServiceTests
{
    private string _testConfigPath = "";

    [TestInitialize]
    public void Setup()
    {
        _testConfigPath = Path.Combine(Path.GetTempPath(), $"test_config_{Guid.NewGuid()}.json");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_testConfigPath))
        {
            File.Delete(_testConfigPath);
        }
    }

    [TestMethod]
    public async Task LoadAsync_NonExistentFile_ReturnsDefaultConfig()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);

        // Act
        var result = await service.LoadAsync();

        // Assert
        Assert.AreEqual(0, result.BackupIntervalMinutes);
        Assert.AreEqual(5, result.MaxBackupVersions);
    }

    [TestMethod]
    public async Task SaveAsync_ValidConfig_SavesSuccessfully()
    {
        // Arrange
        var service = new ConfigurationService(_testConfigPath);
        var config = new Configuration
        {
            BackupIntervalMinutes = 15,
            MaxBackupVersions = 10
        };

        // Act
        await service.SaveAsync(config);

        // Assert
        Assert.IsTrue(File.Exists(_testConfigPath));
        var loadedConfig = await service.LoadAsync();
        Assert.AreEqual(15, loadedConfig.BackupIntervalMinutes);
        Assert.AreEqual(10, loadedConfig.MaxBackupVersions);
    }

    [TestMethod]
    public async Task LoadAsync_CorruptFile_ReturnsDefaultConfig()
    {
        // Arrange
        await File.WriteAllTextAsync(_testConfigPath, "invalid json content");
        var service = new ConfigurationService(_testConfigPath);

        // Act
        var result = await service.LoadAsync();

        // Assert
        Assert.AreEqual(0, result.BackupIntervalMinutes);
        Assert.AreEqual(5, result.MaxBackupVersions);
    }

    [TestMethod]
    public async Task SaveAsync_InvalidPath_ThrowsException()
    {
        // Arrange
        var invalidPath = Path.Combine("Z:\\nonexistent\\path", "config.json");
        var service = new ConfigurationService(invalidPath);
        var config = new Configuration { BackupIntervalMinutes = 10, MaxBackupVersions = 5 };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(
            () => service.SaveAsync(config));
    }
}
