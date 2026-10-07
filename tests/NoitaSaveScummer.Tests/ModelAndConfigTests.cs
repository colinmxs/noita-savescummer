using NoitaSaveScummer.Models;
using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.Tests;

public class ModelAndConfigTests : IDisposable
{
    private readonly TempSave _temp = new();

    public void Dispose() => _temp.Dispose();

    [Theory]
    [InlineData(90 * 60, "01:30:00")]
    [InlineData(5 * 60 + 7, "05:07")]
    [InlineData(-3, "00:00")]
    public void Countdown_formats_hours(int seconds, string expected) =>
        Assert.Equal(expected, ApplicationState.FormatCountdown(TimeSpan.FromSeconds(seconds)));

    [Fact]
    public void Pause_and_resume_keep_remaining_time()
    {
        var state = new ApplicationState();
        var t0 = new DateTime(2025, 1, 1, 12, 0, 0);
        state.ScheduleNext(10, t0);
        state.Pause(t0.AddMinutes(4));
        Assert.Equal(TimeSpan.FromMinutes(6), state.PausedTimeRemaining);
        Assert.False(state.IsBackupDue(t0.AddHours(1)));

        state.Resume(t0.AddMinutes(30));
        Assert.Equal(t0.AddMinutes(36), state.NextBackupTime);
    }

    [Fact]
    public async Task Configuration_round_trips_all_fields()
    {
        var service = new ConfigurationService(Path.Combine(_temp.Root, "config.json"));
        var config = new Configuration
        {
            BackupIntervalMinutes = 7,
            MaxBackupVersions = 33,
            DefaultPlayerPositionX = 1.5,
            KeepCurrentProgressOnRestore = false,
            EnableGlobalHotkeys = false,
        };

        await service.SaveAsync(config);
        var loaded = await service.LoadAsync();

        Assert.Equal(7, loaded.BackupIntervalMinutes);
        Assert.Equal(33, loaded.MaxBackupVersions);
        Assert.Equal(1.5, loaded.DefaultPlayerPositionX);
        Assert.False(loaded.KeepCurrentProgressOnRestore);
        Assert.False(loaded.EnableGlobalHotkeys);
    }

    [Fact]
    public async Task Version_1_config_loads_with_new_defaults()
    {
        var path = Path.Combine(_temp.Root, "config.json");
        await File.WriteAllTextAsync(path, """{ "BackupIntervalMinutes": 5, "MaxBackupVersions": 10 }""");

        var loaded = await new ConfigurationService(path).LoadAsync();

        Assert.True(loaded.IsValid());
        Assert.True(loaded.BackupOnNoitaExit);
        Assert.Equal(3, loaded.MaxUndoBackups);
    }

    [Fact]
    public async Task Corrupt_config_is_set_aside_with_a_warning()
    {
        var path = Path.Combine(_temp.Root, "config.json");
        await File.WriteAllTextAsync(path, "{ not json");
        var service = new ConfigurationService(path);

        var loaded = await service.LoadAsync();

        Assert.False(loaded.IsValid());
        Assert.NotNull(service.LoadWarning);
        Assert.True(File.Exists(path + ".corrupt"));
    }

    [Fact]
    public void Clone_is_independent()
    {
        var original = new Configuration { BackupIntervalMinutes = 5 };
        var copy = original.Clone();
        copy.BackupIntervalMinutes = 9;
        Assert.Equal(5, original.BackupIntervalMinutes);
    }

    [Fact]
    public void Corrupt_preservation_file_is_set_aside_with_a_warning()
    {
        Directory.CreateDirectory(_temp.BackupsPath);
        File.WriteAllText(Path.Combine(_temp.BackupsPath, PreservationService.FileName), "[[[");

        var service = new PreservationService(_temp.BackupsPath);

        Assert.NotNull(service.LoadWarning);
        Assert.False(service.IsPreserved("anything"));
    }

    [Fact]
    public void App_paths_honor_arguments()
    {
        var paths = AppPaths.Resolve(["--save-path", _temp.SavePath, $"--backup-path={_temp.Root}"]);
        Assert.Equal(Path.GetFullPath(_temp.SavePath), paths.SavePath);
        Assert.Equal(Path.Combine(Path.GetFullPath(_temp.Root), "backups"), paths.BackupsPath);
    }

    [Theory]
    [InlineData("persistent/flags/x", true)]
    [InlineData("stats", true)]
    [InlineData("mod_config.xml", true)]
    [InlineData("world/.stream_info", false)]
    [InlineData("player.xml", false)]
    [InlineData("session_numbers.salakieli", false)]
    public void Save_layout_classifies_paths(string path, bool persistent) =>
        Assert.Equal(persistent, SaveLayout.IsPersistent(path));
}
