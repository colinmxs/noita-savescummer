using NoitaSaveScummer.Models;
using NoitaSaveScummer.Services;
using NoitaSaveScummer.UI;

namespace NoitaSaveScummer.Services;

public class NoitaSaveScummerApp
{
    private readonly string _noitaSavePath;
    private readonly string _backupsPath;
    private readonly IConfigurationService _configService;
    private readonly IBackupService _backupService;
    private readonly ApplicationState _state;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public NoitaSaveScummerApp(string noitaSavePath, string backupsPath, 
        IConfigurationService configService, IBackupService backupService)
    {
        _noitaSavePath = noitaSavePath;
        _backupsPath = backupsPath;
        _configService = configService;
        _backupService = backupService;
        _state = new ApplicationState();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public async Task<bool> InitializeAsync()
    {
        ConsoleDisplay.ShowInitializationMessage();

        if (!Directory.Exists(_noitaSavePath))
        {
            Console.WriteLine($"{IconProvider.Error} Noita save directory not found!");
            Console.WriteLine($"Expected location: {_noitaSavePath}");
            Console.WriteLine("\nPlease ensure:");
            Console.WriteLine("• Noita has been run at least once");
            Console.WriteLine("• The game is installed in the standard location");
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
            return false;
        }

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_backupsPath)!);
            Directory.CreateDirectory(_backupsPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{IconProvider.Error} Failed to create backup directory: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return false;
        }

        Console.WriteLine($"{IconProvider.Success} Initialization complete!");
        await Task.Delay(1000);
        return true;
    }

    public async Task RunAsync()
    {
        var config = await _configService.LoadAsync();
        if (!config.IsValid())
        {
            config = await ConfigurationPrompts.GetInitialConfiguration();
            await _configService.SaveAsync(config);
        }

        _state.UpdateNextBackupTime(config.BackupIntervalMinutes);

        while (!_cancellationTokenSource.Token.IsCancellationRequested)
        {
            try
            {
                ConsoleDisplay.ShowMainInterface(config, _state, _noitaSavePath, _backupsPath);

                if (!_state.IsPaused && DateTime.Now >= _state.NextBackupTime)
                {
                    await CreateBackupAsync(config);
                }

                if (Console.KeyAvailable)
                {
                    var keyInfo = Console.ReadKey(true);
                    config = await HandleUserInputAsync(keyInfo, config);
                }

                await Task.Delay(100, _cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                ConsoleDisplay.ShowMessage($"{IconProvider.Error} Error: {ex.Message}");
                await Task.Delay(3000);
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    private async Task CreateBackupAsync(Configuration config)
    {
        await _backupService.CreateBackupAsync();
        await _backupService.CleanupOldBackupsAsync(config.MaxBackupVersions);
        _state.LastBackupTime = DateTime.Now;
        _state.UpdateNextBackupTime(config.BackupIntervalMinutes);
    }

    private async Task<Configuration> HandleUserInputAsync(ConsoleKeyInfo keyInfo, Configuration config)
    {
        switch (keyInfo.Key)
        {
            case ConsoleKey.F9:
                await HandleRestoreAsync();
                break;
                
            case ConsoleKey.F8:
                await HandleRestorePlayerOnlyAsync();
                break;
                
            case ConsoleKey.F7:
                await HandlePreservationAsync();
                break;
                
            case ConsoleKey.P:
                HandlePauseResume();
                break;
                
            case ConsoleKey.C:
                config = await HandleConfigurationChangeAsync(config);
                break;
                
            case ConsoleKey.Q:
                ConsoleDisplay.ShowShutdownMessage();
                Environment.Exit(0);
                break;
        }
        
        return config;
    }

    private async Task HandleRestoreAsync()
    {
        try
        {
            var availableBackups = _backupService.GetAvailableBackups();
            
            if (availableBackups.Count == 0)
            {
                ConsoleDisplay.ShowMessage($"{IconProvider.Error} No backups available to restore!");
                await Task.Delay(2000);
                ConsoleDisplay.ClearMessage();
                return;
            }

            var selectedBackup = BackupSelectionMenu.SelectBackupToRestore(availableBackups);
            if (selectedBackup == null)
                return;

            ConsoleDisplay.ShowMessage($"{IconProvider.Hourglass} Restoring backup...");
            await _backupService.RestoreBackupAsync(selectedBackup);
            ConsoleDisplay.ShowMessage($"{IconProvider.Success} Backup restored successfully!");
        }
        catch (Exception ex)
        {
            ConsoleDisplay.ShowMessage($"{IconProvider.Error} Restore failed: {ex.Message}");
        }
        
        await Task.Delay(2000);
        ConsoleDisplay.ClearMessage();
    }

    private async Task HandleRestorePlayerOnlyAsync()
    {
        try
        {
            var availableBackups = _backupService.GetAvailableBackups();
            
            if (availableBackups.Count == 0)
            {
                ConsoleDisplay.ShowMessage($"{IconProvider.Error} No backups available to restore!");
                await Task.Delay(2000);
                ConsoleDisplay.ClearMessage();
                return;
            }

            var (selectedBackup, resetLocation) = BackupSelectionMenu.SelectBackupToRestorePlayerOnly(availableBackups);
            if (selectedBackup == null)
                return;

            var config = await _configService.LoadAsync();
            
            ConsoleDisplay.ShowMessage($"{IconProvider.Hourglass} Restoring player.xml...");
            await _backupService.RestorePlayerOnlyAsync(
                selectedBackup, 
                resetLocation, 
                config.DefaultPlayerPositionX, 
                config.DefaultPlayerPositionY);
            
            var locationMsg = resetLocation ? " (location reset)" : "";
            ConsoleDisplay.ShowMessage($"{IconProvider.Success} Player.xml restored successfully{locationMsg}!");
        }
        catch (OperationCanceledException)
        {
            // User cancelled, just return
            return;
        }
        catch (Exception ex)
        {
            ConsoleDisplay.ShowMessage($"{IconProvider.Error} Player restore failed: {ex.Message}");
        }
        
        await Task.Delay(2000);
        ConsoleDisplay.ClearMessage();
    }

    private async Task HandlePreservationAsync()
    {
        try
        {
            var availableBackups = _backupService.GetAvailableBackups();
            
            if (availableBackups.Count == 0)
            {
                ConsoleDisplay.ShowMessage($"{IconProvider.Error} No backups available to manage!");
                await Task.Delay(2000);
                ConsoleDisplay.ClearMessage();
                return;
            }

            var selectedBackup = BackupSelectionMenu.SelectBackupToPreserve(availableBackups);
            if (selectedBackup == null)
                return;

            _backupService.ToggleBackupPreservation(selectedBackup);
            
            var isNowPreserved = _backupService.IsBackupPreserved(selectedBackup);
            var statusMessage = isNowPreserved ? "preserved" : "unpreserved";
            
            ConsoleDisplay.ShowMessage($"{IconProvider.Success} Backup {statusMessage} successfully!");
        }
        catch (Exception ex)
        {
            ConsoleDisplay.ShowMessage($"{IconProvider.Error} Preservation operation failed: {ex.Message}");
        }
        
        await Task.Delay(2000);
        ConsoleDisplay.ClearMessage();
    }

    private void HandlePauseResume()
    {
        if (_state.IsPaused)
        {
            _state.Resume();
        }
        else
        {
            _state.Pause();
        }
    }

    private async Task<Configuration> HandleConfigurationChangeAsync(Configuration currentConfig)
    {
        var newConfig = await ConfigurationPrompts.UpdateConfiguration(currentConfig);
        await _configService.SaveAsync(newConfig);
        
        if (!_state.IsPaused)
        {
            _state.UpdateNextBackupTime(newConfig.BackupIntervalMinutes);
        }
        else
        {
            _state.PausedTimeRemaining = TimeSpan.FromMinutes(newConfig.BackupIntervalMinutes);
        }
        
        return newConfig;
    }
}
