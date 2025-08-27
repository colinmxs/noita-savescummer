using System.Threading;
using NoitaSaveScummer.Services;
using NoitaSaveScummer.UI;

namespace NoitaSaveScummer;

class Program
{
    private static readonly string NoitaSavePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        @"AppData\LocalLow\Nolla_Games_Noita\save00");
    
    private static readonly string BackupBasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
        "NoitaSaveBackups");
    
    private static readonly string BackupsPath = Path.Combine(BackupBasePath, "backups");
    private static readonly string ConfigPath = Path.Combine(BackupBasePath, "config.json");
    
    private static readonly Mutex AppMutex = new(true, "NoitaSaveScummer_SingleInstance");
    private static NoitaSaveScummerApp? _app;

    static async Task<int> Main(string[] args)
    {
        try
        {
            if (!AppMutex.WaitOne(TimeSpan.Zero, true))
            {
                Console.WriteLine("Noita Save Scummer is already running!");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return 1;
            }

            // Initialize console encoding for better icon support
            UI.IconProvider.Initialize();

            Console.Title = "Noita Save Scummer";
            Console.CancelKeyPress += OnCancelKeyPress;

            var configService = new ConfigurationService(ConfigPath);
            var backupService = new BackupService(NoitaSavePath, BackupsPath);
            
            _app = new NoitaSaveScummerApp(NoitaSavePath, BackupsPath, configService, backupService);

            if (!await _app.InitializeAsync())
            {
                return 1;
            }

            await _app.RunAsync();
            return 0;
        }
        catch (Exception ex)
        {
            Console.Clear();
            Console.WriteLine($"Fatal error: {ex.Message}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return 1;
        }
        finally
        {
            AppMutex?.ReleaseMutex();
            AppMutex?.Dispose();
        }
    }

    private static void OnCancelKeyPress(object? sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        
        try
        {
            _app?.Stop();
            ConsoleDisplay.ShowShutdownMessage();
            Task.Delay(500).Wait();
        }
        catch
        {
            // Ignore exceptions during shutdown
        }
        finally
        {
            try
            {
                AppMutex?.ReleaseMutex();
            }
            catch
            {
                // Ignore mutex release exceptions during shutdown
            }
            
            Environment.Exit(0);
        }
    }
}
