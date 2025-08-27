using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Tests;

[TestClass]
public sealed class ConfigurationTests
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
}
