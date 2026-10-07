using NoitaSaveScummer.Services;
using NoitaSaveScummer.UI;

namespace NoitaSaveScummer;

internal static class Program
{
    // Synchronous Main: the mutex must be acquired and released on the same thread,
    // which an async Main does not guarantee after an await.
    private static int Main(string[] args)
    {
        using var mutex = new Mutex(initiallyOwned: false, @"Local\NoitaSaveScummer_SingleInstance");
        bool ownsMutex;
        try
        {
            ownsMutex = mutex.WaitOne(TimeSpan.Zero);
        }
        catch (AbandonedMutexException)
        {
            ownsMutex = true; // previous instance crashed; ownership passes to us
        }

        if (!ownsMutex)
        {
            Console.WriteLine("Noita Save Scummer is already running.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(intercept: true);
            return 1;
        }

        try
        {
            return RunAsync(args).GetAwaiter().GetResult();
        }
        finally
        {
            mutex.ReleaseMutex();
        }
    }

    private static async Task<int> RunAsync(string[] args)
    {
        using var shutdown = new CancellationTokenSource();
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true; // exit through the main loop so a running backup can finish
            shutdown.Cancel();
        };

        try
        {
            IconProvider.Initialize();
            Console.Title = "Noita Save Scummer";

            var paths = AppPaths.Resolve(args);
            var configService = new ConfigurationService(paths.ConfigPath);
            var backupService = new BackupService(paths.SavePath, paths.BackupsPath);
            using var app = new NoitaSaveScummerApp(paths, configService, backupService, new NoitaProcess());

            if (!app.Initialize()) return 1;
            await app.RunAsync(shutdown.Token);
            ConsoleDisplay.ShowShutdownMessage();
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"Fatal error: {ex}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey(intercept: true);
            return 1;
        }
    }
}
