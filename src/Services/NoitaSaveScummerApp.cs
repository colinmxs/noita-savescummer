using System.Threading.Channels;
using NoitaSaveScummer.Models;
using NoitaSaveScummer.UI;

namespace NoitaSaveScummer.Services;

public sealed class NoitaSaveScummerApp : IDisposable
{
    private static readonly TimeSpan StatusDuration = TimeSpan.FromSeconds(8);
    private static readonly TimeSpan ProcessPollInterval = TimeSpan.FromSeconds(1);

    private readonly AppPaths _paths;
    private readonly IConfigurationService _configService;
    private readonly IBackupService _backupService;
    private readonly INoitaProcess _noita;
    private readonly ApplicationState _state = new();
    private readonly ConsoleRenderer _renderer = new();
    private readonly SemaphoreSlim _ioLock = new(1, 1);
    private readonly Channel<HotkeyAction> _hotkeys = Channel.CreateUnbounded<HotkeyAction>();

    private Configuration _config = new();
    private IReadOnlyList<BackupInfo> _backups = [];
    private GlobalHotkeys? _globalHotkeys;
    private string? _hotkeyProblem;
    private Task _backgroundBackup = Task.CompletedTask;
    private DateTime _nextProcessPoll = DateTime.MinValue;
    private string? _lastUndoBackup;

    public NoitaSaveScummerApp(AppPaths paths, IConfigurationService configService, IBackupService backupService, INoitaProcess noita)
    {
        _paths = paths;
        _configService = configService;
        _backupService = backupService;
        _noita = noita;
    }

    public bool Initialize()
    {
        ConsoleDisplay.ShowInitializationMessage();

        if (!Directory.Exists(_paths.SavePath))
        {
            Console.WriteLine($"{IconProvider.Error} Noita save directory not found:");
            Console.WriteLine($"   {_paths.SavePath}");
            Console.WriteLine("\nRun Noita at least once, or pass --save-path <dir> / set NOITA_SAVE_PATH.");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey(intercept: true);
            return false;
        }

        Directory.CreateDirectory(_paths.BackupsPath);
        foreach (var message in _backupService.RecoverInterruptedOperations())
            Console.WriteLine($"{IconProvider.Info} {message}");
        return true;
    }

    public async Task RunAsync(CancellationToken ct)
    {
        _config = await _configService.LoadAsync();
        if (_configService is ConfigurationService { LoadWarning: { } warning })
            _state.SetStatus($"{IconProvider.Warning} {warning}", StatusDuration);
        if (!_config.IsValid())
        {
            _config = ConfigurationPrompts.GetInitialConfiguration(_config);
            await _configService.SaveAsync(_config);
        }

        ApplyHotkeySetting();
        _state.IsNoitaRunning = _noita.IsRunning();
        _state.ScheduleNext(_config.BackupIntervalMinutes, DateTime.Now);
        RefreshBackups();
        _renderer.Invalidate();
        Console.Clear();

        try
        {
            while (!ct.IsCancellationRequested)
            {
                var now = DateTime.Now;
                PollNoitaProcess(now);

                if (_state.IsBackupDue(now) && _backgroundBackup.IsCompleted)
                {
                    _state.ScheduleNext(_config.BackupIntervalMinutes, now); // schedule first: a failure never causes a retry storm
                    StartBackgroundBackup(BackupKind.Timed, _config.SkipUnchangedBackups);
                }

                while (_hotkeys.Reader.TryRead(out var action))
                    await HandleHotkeyAsync(action, ct);

                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true);
                    if (key.Key == ConsoleKey.Q) break;
                    await HandleKeyAsync(key, ct);
                }

                _renderer.Render(ConsoleDisplay.BuildMainScreen(
                    new MainScreenModel(_config, _state, _paths, _backups, _hotkeyProblem, DateTime.Now)));

                await Task.Delay(100, ct);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
        }

        _state.SetStatus("Waiting for running backup to finish...");
        await _backgroundBackup;
    }

    // ---------------------------------------------------------------- Noita process / backups

    private void PollNoitaProcess(DateTime now)
    {
        if (now < _nextProcessPoll) return;
        _nextProcessPoll = now + ProcessPollInterval;

        var running = _noita.IsRunning();
        var exited = _state.IsNoitaRunning && !running;
        _state.IsNoitaRunning = running;

        if (exited && _config.BackupOnNoitaExit && _backgroundBackup.IsCompleted)
            StartBackgroundBackup(BackupKind.OnExit, skipIfUnchanged: true);
    }

    private void StartBackgroundBackup(BackupKind kind, bool skipIfUnchanged)
    {
        _backgroundBackup = Task.Run(async () =>
        {
            try
            {
                await RunBackupAsync(kind, skipIfUnchanged, CancellationToken.None);
            }
            catch (Exception ex)
            {
                _state.SetStatus($"{IconProvider.Error} Backup failed: {ex.Message}", StatusDuration);
            }
        });
    }

    private async Task<BackupResult> RunBackupAsync(BackupKind kind, bool skipIfUnchanged, CancellationToken ct)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            _state.Busy = $"{IconProvider.Hourglass} Backing up...";
            var result = await _backupService.CreateBackupAsync(
                new BackupRequest(kind, _noita.IsRunning(), skipIfUnchanged), ct);

            if (result.Outcome == BackupOutcome.Created)
            {
                _state.LastBackupTime = DateTime.Now;
                var cleanup = _backupService.CleanupOldBackups(_config.MaxBackupVersions, _config.MaxUndoBackups);
                var suffix = cleanup.Errors.Count > 0 ? $" Cleanup problem: {cleanup.Errors[0]}" : string.Empty;
                _state.SetStatus($"{IconProvider.Success} {result.Message}{suffix}", StatusDuration);
            }
            else
            {
                var icon = result.Outcome == BackupOutcome.Failed ? IconProvider.Error : IconProvider.Info;
                _state.SetStatus($"{icon} {result.Message}", StatusDuration);
            }

            RefreshBackups();
            return result;
        }
        finally
        {
            _state.Busy = null;
            _ioLock.Release();
        }
    }

    private void RefreshBackups() => _backups = _backupService.GetAvailableBackups();

    // ---------------------------------------------------------------- input

    private async Task HandleKeyAsync(ConsoleKeyInfo key, CancellationToken ct)
    {
        try
        {
            switch (key.Key)
            {
                case ConsoleKey.F9: await FullRestoreInteractiveAsync(ct); break;
                case ConsoleKey.F8: await PlayerRestoreInteractiveAsync(ct); break;
                case ConsoleKey.F7: TogglePreservationInteractive(); break;
                case ConsoleKey.U: await UndoLastRestoreAsync(ct); break;
                case ConsoleKey.B: await RunBackupAsync(BackupKind.Manual, skipIfUnchanged: false, ct); break;
                case ConsoleKey.P: TogglePause(); break;
                case ConsoleKey.C: await ChangeSettingsAsync(); break;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _state.SetStatus($"{IconProvider.Error} {ex.Message}", StatusDuration);
        }
        finally
        {
            _renderer.Invalidate();
        }
    }

    private async Task HandleHotkeyAsync(HotkeyAction action, CancellationToken ct)
    {
        try
        {
            switch (action)
            {
                case HotkeyAction.QuickSave:
                    await RunBackupAsync(BackupKind.Manual, skipIfUnchanged: false, ct);
                    break;
                case HotkeyAction.QuickLoad:
                    await QuickLoadAsync(ct);
                    break;
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _state.SetStatus($"{IconProvider.Error} {ex.Message}", StatusDuration);
        }
    }

    // ---------------------------------------------------------------- restores

    private async Task FullRestoreInteractiveAsync(CancellationToken ct)
    {
        var backup = SelectBackup("Full restore (world + player)", RestoreHint());
        if (backup is null || !await EnsureNoitaClosedAsync(ct)) return;
        await RestoreAsync(backup, ct);
    }

    private async Task QuickLoadAsync(CancellationToken ct)
    {
        var backup = _backups.FirstOrDefault(b => b.Kind != BackupKind.PreRestore);
        if (backup is null)
        {
            _state.SetStatus($"{IconProvider.Error} Quick-load: no backups yet.", StatusDuration);
            return;
        }

        var wasRunning = _noita.IsRunning();
        if (wasRunning && !await _noita.CloseAsync(TimeSpan.FromSeconds(10), ct))
        {
            _state.SetStatus($"{IconProvider.Error} Quick-load: could not close Noita (running as administrator?).", StatusDuration);
            return;
        }

        await RestoreAsync(backup, ct);
        if (wasRunning && _config.RelaunchNoitaAfterQuickLoad)
        {
            _noita.Launch(_config.NoitaLaunchCommand);
            _state.SetStatus($"{IconProvider.Success} Quick-loaded {backup.FormattedTimestamp}; relaunching Noita. Choose 'Continue'.", StatusDuration);
        }
    }

    private async Task RestoreAsync(BackupInfo backup, CancellationToken ct)
    {
        await _ioLock.WaitAsync(ct);
        try
        {
            _state.Busy = $"{IconProvider.Hourglass} Restoring {backup.FormattedTimestamp}...";
            var result = await _backupService.RestoreBackupAsync(backup.Name, _config.KeepCurrentProgressOnRestore, ct);
            _lastUndoBackup = result.UndoBackup?.Name;
            _backupService.CleanupOldBackups(_config.MaxBackupVersions, _config.MaxUndoBackups);
            RefreshBackups();
            _state.SetStatus($"{IconProvider.Success} Restored {backup.FormattedTimestamp}. Start Noita and choose 'Continue'. (U = undo)", StatusDuration);
        }
        finally
        {
            _state.Busy = null;
            _ioLock.Release();
        }
    }

    private async Task PlayerRestoreInteractiveAsync(CancellationToken ct)
    {
        var backup = SelectBackup("Player-only restore (keeps the current world)", null);
        if (backup is null) return;

        var reset = Prompts.AskResetLocation(_renderer, _config.ResetPlayerLocationOnRestore,
            _config.DefaultPlayerPositionX, _config.DefaultPlayerPositionY);
        if (reset is null || !await EnsureNoitaClosedAsync(ct)) return;

        await _ioLock.WaitAsync(ct);
        try
        {
            _state.Busy = $"{IconProvider.Hourglass} Restoring player...";
            var position = reset.Value ? (_config.DefaultPlayerPositionX, _config.DefaultPlayerPositionY) : ((double, double)?)null;
            var result = await _backupService.RestorePlayerOnlyAsync(backup.Name, position, ct);
            _lastUndoBackup = result.UndoBackup?.Name;
            RefreshBackups();
            _state.SetStatus($"{IconProvider.Success} Player restored from {backup.FormattedTimestamp}{(reset.Value ? " at spawn" : string.Empty)}. (U = undo)", StatusDuration);
        }
        finally
        {
            _state.Busy = null;
            _ioLock.Release();
        }
    }

    private async Task UndoLastRestoreAsync(CancellationToken ct)
    {
        var undo = (_lastUndoBackup is { } name ? _backups.FirstOrDefault(b => b.Name == name) : null)
                   ?? _backups.FirstOrDefault(b => b.Kind == BackupKind.PreRestore);
        if (undo is null)
        {
            _state.SetStatus($"{IconProvider.Info} Nothing to undo.", StatusDuration);
            return;
        }

        if (!Prompts.Confirm(_renderer, $"{IconProvider.Undo} Undo restore",
                $"Put save00 back to how it was at {undo.FormattedTimestamp}, before the last restore?"))
            return;
        if (!await EnsureNoitaClosedAsync(ct)) return;

        await _ioLock.WaitAsync(ct);
        try
        {
            _state.Busy = $"{IconProvider.Hourglass} Undoing...";
            // Exact copy: the undo snapshot already contains the progress files as they were.
            var result = await _backupService.RestoreBackupAsync(undo.Name, keepCurrentProgress: false, ct);
            _lastUndoBackup = result.UndoBackup?.Name;
            RefreshBackups();
            _state.SetStatus($"{IconProvider.Success} Undone. save00 is back to {undo.FormattedTimestamp}.", StatusDuration);
        }
        finally
        {
            _state.Busy = null;
            _ioLock.Release();
        }
    }

    private async Task<bool> EnsureNoitaClosedAsync(CancellationToken ct)
    {
        if (!_noita.IsRunning()) return true;
        if (!Prompts.ConfirmCloseNoita(_renderer)) return false;
        if (await _noita.CloseAsync(TimeSpan.FromSeconds(10), ct))
        {
            _state.IsNoitaRunning = false;
            return true;
        }
        _state.SetStatus($"{IconProvider.Error} Could not close Noita. Close it yourself and try again.", StatusDuration);
        return false;
    }

    private static string RestoreHint() =>
        "[CLEAN] = taken while Noita was closed (safest).  [LIVE] = taken mid-game, verified unchanged during copy.";

    private BackupInfo? SelectBackup(string title, string? hint)
    {
        RefreshBackups();
        var candidates = _backups.Where(b => b.Kind != BackupKind.PreRestore).ToList();
        if (candidates.Count == 0)
        {
            _state.SetStatus($"{IconProvider.Error} No backups available yet. Press B to make one.", StatusDuration);
            return null;
        }
        return BackupSelectionMenu.Select(_renderer, candidates, title, hint);
    }

    // ---------------------------------------------------------------- misc commands

    private void TogglePreservationInteractive()
    {
        RefreshBackups();
        var backup = BackupSelectionMenu.Select(_renderer, _backups, "Preserve / unpreserve backup",
            "Preserved backups are never deleted by retention cleanup.");
        if (backup is null) return;
        var preserved = _backupService.TogglePreservation(backup.Name);
        RefreshBackups();
        _state.SetStatus($"{IconProvider.Success} {backup.FormattedTimestamp} {(preserved ? "preserved" : "no longer preserved")}.", StatusDuration);
    }

    private void TogglePause()
    {
        if (_state.IsPaused) _state.Resume(DateTime.Now);
        else _state.Pause(DateTime.Now);
    }

    private async Task ChangeSettingsAsync()
    {
        TryShowCursor();
        var updated = ConfigurationPrompts.UpdateConfiguration(_config);
        await _configService.SaveAsync(updated);
        var intervalChanged = updated.BackupIntervalMinutes != _config.BackupIntervalMinutes;
        _config = updated;
        if (intervalChanged) _state.ScheduleNext(_config.BackupIntervalMinutes, DateTime.Now);
        ApplyHotkeySetting();
        Console.Clear();
    }

    private void ApplyHotkeySetting()
    {
        _globalHotkeys?.Dispose();
        _globalHotkeys = null;
        _hotkeyProblem = null;

        if (!_config.EnableGlobalHotkeys)
        {
            _hotkeyProblem = "disabled (enable in settings with C)";
            return;
        }
        if (!OperatingSystem.IsWindows())
        {
            _hotkeyProblem = "only available on Windows";
            return;
        }

        _globalHotkeys = new GlobalHotkeys(_hotkeys.Writer);
        var failed = _globalHotkeys.Start();
        if (failed.Count > 0)
            _hotkeyProblem = $"could not register {string.Join(", ", failed)} (used by another program)";
    }

    private static void TryShowCursor()
    {
        try
        {
            Console.CursorVisible = true;
        }
        catch (Exception ex) when (ex is IOException or PlatformNotSupportedException)
        {
        }
    }

    public void Dispose()
    {
        _globalHotkeys?.Dispose();
        _ioLock.Dispose();
    }
}
