using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Tests.Models;

[TestClass]
public class ConfigurationTests
{
    [TestMethod]
    public void Configuration_ValidValues_ReturnsTrue()
    {
        // Arrange
        var config = new Configuration
        {
            BackupIntervalMinutes = 10,
            MaxBackupVersions = 5
        };

        // Act
        var result = config.IsValid();

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Configuration_InvalidBackupInterval_ReturnsFalse()
    {
        // Arrange
        var config = new Configuration
        {
            BackupIntervalMinutes = 0,
            MaxBackupVersions = 5
        };

        // Act
        var result = config.IsValid();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Configuration_InvalidMaxVersions_ReturnsFalse()
    {
        // Arrange
        var config = new Configuration
        {
            BackupIntervalMinutes = 10,
            MaxBackupVersions = 0
        };

        // Act
        var result = config.IsValid();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Configuration_BackupIntervalTooHigh_ReturnsFalse()
    {
        // Arrange
        var config = new Configuration
        {
            BackupIntervalMinutes = 1500,
            MaxBackupVersions = 5
        };

        // Act
        var result = config.IsValid();

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Configuration_MaxVersionsTooHigh_ReturnsFalse()
    {
        // Arrange
        var config = new Configuration
        {
            BackupIntervalMinutes = 10,
            MaxBackupVersions = 150
        };

        // Act
        var result = config.IsValid();

        // Assert
        Assert.IsFalse(result);
    }
}
