using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Tests.Models;

[TestClass]
public class ApplicationStateTests
{
    [TestMethod]
    public void UpdateNextBackupTime_ValidInterval_SetsCorrectTime()
    {
        // Arrange
        var state = new ApplicationState();
        var beforeTime = DateTime.Now;

        // Act
        state.UpdateNextBackupTime(10);

        // Assert
        var expectedTime = beforeTime.AddMinutes(10);
        var tolerance = TimeSpan.FromSeconds(1);
        Assert.IsTrue(Math.Abs((state.NextBackupTime - expectedTime).TotalSeconds) < tolerance.TotalSeconds);
    }

    [TestMethod]
    public void Pause_WhenNotPaused_SetsPausedState()
    {
        // Arrange
        var state = new ApplicationState();
        state.UpdateNextBackupTime(10);

        // Act
        state.Pause();

        // Assert
        Assert.IsTrue(state.IsPaused);
        Assert.IsTrue(state.PausedTimeRemaining > TimeSpan.Zero);
    }

    [TestMethod]
    public void Resume_WhenPaused_ResetsState()
    {
        // Arrange
        var state = new ApplicationState();
        state.UpdateNextBackupTime(10);
        state.Pause();

        // Act
        state.Resume();

        // Assert
        Assert.IsFalse(state.IsPaused);
        Assert.AreEqual(TimeSpan.Zero, state.PausedTimeRemaining);
    }

    [TestMethod]
    public void Pause_WhenAlreadyPaused_DoesNotChange()
    {
        // Arrange
        var state = new ApplicationState();
        state.UpdateNextBackupTime(10);
        state.Pause();
        var originalTimeRemaining = state.PausedTimeRemaining;

        // Act
        state.Pause();

        // Assert
        Assert.IsTrue(state.IsPaused);
        Assert.AreEqual(originalTimeRemaining, state.PausedTimeRemaining);
    }

    [TestMethod]
    public void TimeUntilNextBackup_ReturnsCorrectTimeSpan()
    {
        // Arrange
        var state = new ApplicationState();
        state.NextBackupTime = DateTime.Now.AddMinutes(5);

        // Act
        var result = state.TimeUntilNextBackup;

        // Assert
        Assert.IsTrue(result.TotalMinutes > 4 && result.TotalMinutes < 6);
    }
}
