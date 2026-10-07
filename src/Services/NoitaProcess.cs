using System.ComponentModel;
using System.Diagnostics;

namespace NoitaSaveScummer.Services;

public interface INoitaProcess
{
    bool IsRunning();

    /// <summary>Force-closes Noita. A force-close does not touch the save; it stays as it was on disk.</summary>
    Task<bool> CloseAsync(TimeSpan timeout, CancellationToken ct = default);

    void Launch(string command);
}

public sealed class NoitaProcess : INoitaProcess
{
    private static readonly string[] ProcessNames = ["noita", "noita_dev"];

    public bool IsRunning()
    {
        foreach (var name in ProcessNames)
        {
            var processes = Process.GetProcessesByName(name);
            var found = processes.Length > 0;
            foreach (var p in processes) p.Dispose();
            if (found) return true;
        }
        return false;
    }

    public async Task<bool> CloseAsync(TimeSpan timeout, CancellationToken ct = default)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeoutCts.CancelAfter(timeout);

        foreach (var name in ProcessNames)
        {
            foreach (var process in Process.GetProcessesByName(name))
            {
                using (process)
                {
                    try
                    {
                        process.Kill(entireProcessTree: true);
                        await process.WaitForExitAsync(timeoutCts.Token);
                    }
                    catch (Exception ex) when (ex is Win32Exception or InvalidOperationException)
                    {
                        // Already exited, or access denied (Noita running as administrator).
                    }
                    catch (OperationCanceledException) when (!ct.IsCancellationRequested)
                    {
                        // Timed out; the IsRunning check below reports failure.
                    }
                }
            }
        }

        return !IsRunning();
    }

    public void Launch(string command)
    {
        using var _ = Process.Start(new ProcessStartInfo(command) { UseShellExecute = true });
    }
}
