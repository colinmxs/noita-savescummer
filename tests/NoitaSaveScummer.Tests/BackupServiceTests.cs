using NoitaSaveScummer.Models;
using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.Tests;

public class BackupServiceTests : IDisposable
{
    private readonly TempSave _save = new();

    public void Dispose() => _save.Dispose();

    private static BackupRequest Manual(bool running = false, bool skip = false) => new(BackupKind.Manual, running, skip);

    [Fact]
    public async Task Backup_copies_everything_including_hidden_stream_info_and_writes_metadata()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();

        var result = await service.CreateBackupAsync(Manual());

        Assert.Equal(BackupOutcome.Created, result.Outcome);
        var folder = result.Backup!.FolderPath;
        Assert.True(File.Exists(Path.Combine(folder, "world", ".stream_info")));
        Assert.True(File.Exists(Path.Combine(folder, "session_numbers.salakieli")));
        Assert.True(File.Exists(Path.Combine(folder, SaveLayout.MetadataFileName)));
        Assert.True(result.Backup.IsClean);
        Assert.Equal(BackupKind.Manual, result.Backup.Kind);
    }

    [Fact]
    public async Task Backup_while_running_is_marked_live()
    {
        _save.WriteRun("A");
        var result = await _save.CreateService().CreateBackupAsync(Manual(running: true));
        Assert.False(result.Backup!.IsClean);
    }

    [Fact]
    public async Task Backup_is_skipped_when_save_unchanged()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        await service.CreateBackupAsync(Manual());

        var second = await service.CreateBackupAsync(Manual(skip: true));

        Assert.Equal(BackupOutcome.SkippedUnchanged, second.Outcome);
        Assert.Single(service.GetAvailableBackups());
    }

    [Fact]
    public async Task Backup_is_skipped_when_no_run_is_active()
    {
        _save.Write("persistent/flags/x", string.Empty); // progress only, no player.xml / world
        var result = await _save.CreateService().CreateBackupAsync(Manual());
        Assert.Equal(BackupOutcome.SkippedNoActiveRun, result.Outcome);
    }

    [Fact]
    public async Task Two_backups_in_the_same_second_get_unique_names()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        var first = await service.CreateBackupAsync(Manual());
        var second = await service.CreateBackupAsync(Manual());

        Assert.NotEqual(first.Backup!.Name, second.Backup!.Name);
        Assert.Equal(2, service.GetAvailableBackups().Count);
    }

    [Fact]
    public async Task Full_restore_replaces_world_entirely_so_no_files_from_the_newer_run_remain()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        var backup = (await service.CreateBackupAsync(Manual())).Backup!;

        // Play on: world changes, new chunks appear (this is what used to survive a restore).
        _save.WriteRun("B");
        _save.Write("world/world_512_0.png_petri", "new-chunk-B");
        _save.Write("world/entities_1.bin", "new-entities-B");

        await service.RestoreBackupAsync(backup.Name, keepCurrentProgress: false);

        Assert.Equal("stream-A", _save.Read("world/.stream_info"));
        Assert.Equal("scenes-A", _save.Read("world/world_pixel_scenes.bin"));
        Assert.Equal("session-A", _save.Read("session_numbers.salakieli"));
        Assert.False(_save.Exists("world/world_512_0.png_petri"));
        Assert.False(_save.Exists("world/entities_1.bin"));
        Assert.False(_save.Exists(SaveLayout.MetadataFileName));
    }

    [Fact]
    public async Task Full_restore_keeps_current_progress_when_requested()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        var backup = (await service.CreateBackupAsync(Manual())).Backup!;
        _save.WriteRun("B");

        await service.RestoreBackupAsync(backup.Name, keepCurrentProgress: true);

        Assert.Contains("tag=\"A\"", _save.Read("world_state.xml"));
        Assert.Equal("stats-B", _save.Read("stats/_stats.salakieli"));
        Assert.True(_save.Exists("persistent/flags/card_unlocked_B"));
        Assert.True(_save.Exists("persistent/flags/card_unlocked_A")); // kept current dir, which still has A's unlock too
        Assert.Contains("tag=\"B\"", _save.Read("mod_config.xml"));
    }

    [Fact]
    public async Task Restore_takes_an_undo_backup_that_can_restore_the_previous_state()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        var backup = (await service.CreateBackupAsync(Manual())).Backup!;
        _save.WriteRun("B");

        var result = await service.RestoreBackupAsync(backup.Name, keepCurrentProgress: false);

        Assert.NotNull(result.UndoBackup);
        Assert.Equal(BackupKind.PreRestore, result.UndoBackup!.Kind);
        await service.RestoreBackupAsync(result.UndoBackup.Name, keepCurrentProgress: false);
        Assert.Equal("stream-B", _save.Read("world/.stream_info"));
    }

    [Fact]
    public async Task Restore_rejects_backup_without_a_run()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        var backup = (await service.CreateBackupAsync(Manual())).Backup!;
        File.Delete(Path.Combine(backup.FolderPath, "player.xml"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.RestoreBackupAsync(backup.Name, false));
        Assert.Equal("stream-A", _save.Read("world/.stream_info"));
    }

    [Fact]
    public async Task Player_only_restore_resets_position_with_invariant_culture_and_keeps_world()
    {
        var previous = Thread.CurrentThread.CurrentCulture;
        Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("de-DE");
        try
        {
            _save.WriteRun("A", x: 5000, y: 9000);
            var service = _save.CreateService();
            var backup = (await service.CreateBackupAsync(Manual())).Backup!;
            _save.WriteRun("B");

            await service.RestorePlayerOnlyAsync(backup.Name, (215.0, -95.0));

            var xml = _save.Read("player.xml");
            Assert.Contains("position.x=\"215.000000\"", xml);
            Assert.Contains("position.y=\"-95.000000\"", xml);
            Assert.Contains("<Entity name=\"inventory_quick\" tag=\"A\"><_Transform position.x=\"1\"", xml); // child untouched
            Assert.Equal("stream-B", _save.Read("world/.stream_info"));
            Assert.Equal((215.0, -95.0), PlayerXml.ReadPosition(Path.Combine(_save.SavePath, "player.xml")));
        }
        finally
        {
            Thread.CurrentThread.CurrentCulture = previous;
        }
    }

    [Fact]
    public async Task Cleanup_keeps_preserved_and_limits_undo_backups_separately()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        var names = new List<string>();
        for (var i = 0; i < 4; i++)
        {
            _save.Write("world/tick", i.ToString(System.Globalization.CultureInfo.InvariantCulture));
            names.Add((await service.CreateBackupAsync(Manual())).Backup!.Name);
        }
        service.TogglePreservation(names[0]); // oldest

        var result = service.CleanupOldBackups(maxVersions: 2, maxUndoBackups: 1);

        var remaining = service.GetAvailableBackups().Select(b => b.Name).ToList();
        Assert.Contains(names[0], remaining);
        Assert.Contains(names[3], remaining);
        Assert.Contains(names[2], remaining);
        Assert.DoesNotContain(names[1], remaining);
        Assert.Single(result.Deleted);
    }

    [Fact]
    public async Task Preservation_survives_a_new_service_instance()
    {
        _save.WriteRun("A");
        var name = (await _save.CreateService().CreateBackupAsync(Manual())).Backup!.Name;
        _save.CreateService().TogglePreservation(name);

        Assert.True(_save.CreateService().GetAvailableBackups().Single().IsPreserved);
    }

    [Fact]
    public async Task Interrupted_restore_is_recovered_and_partial_backups_are_hidden_and_removed()
    {
        _save.WriteRun("A");
        var service = _save.CreateService();
        await service.CreateBackupAsync(Manual());

        // Simulate a crash after save00 was moved aside but before the new one was moved in.
        Directory.Move(_save.SavePath, _save.SavePath + BackupService.OldSuffix);
        Directory.CreateDirectory(Path.Combine(_save.BackupsPath, BackupService.PartialPrefix + "2020-01-01_00-00-00"));
        Assert.Single(service.GetAvailableBackups());

        var messages = service.RecoverInterruptedOperations();

        Assert.Equal("stream-A", _save.Read("world/.stream_info"));
        Assert.False(Directory.Exists(Path.Combine(_save.BackupsPath, BackupService.PartialPrefix + "2020-01-01_00-00-00")));
        Assert.NotEmpty(messages);
    }

    [Fact]
    public void Legacy_backup_folders_are_listed()
    {
        Directory.CreateDirectory(Path.Combine(_save.BackupsPath, "2024-05-01_12-00-00"));
        Directory.CreateDirectory(Path.Combine(_save.BackupsPath, "not-a-backup"));

        var backups = _save.CreateService().GetAvailableBackups();

        var legacy = Assert.Single(backups);
        Assert.Equal(BackupKind.Legacy, legacy.Kind);
        Assert.False(legacy.IsClean);
    }
}
