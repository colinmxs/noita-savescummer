using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Services;

public interface IBackupService
{
    Task<BackupResult> CreateBackupAsync(BackupRequest request, CancellationToken ct = default);
    Task<RestoreResult> RestoreBackupAsync(string backupName, bool keepCurrentProgress, CancellationToken ct = default);
    Task<RestoreResult> RestorePlayerOnlyAsync(string backupName, (double X, double Y)? resetPosition, CancellationToken ct = default);

    /// <summary>
    /// Edits the live player.xml safely: takes an undo backup, runs <paramref name="edit"/>(currentPath, tempPath),
    /// then atomically replaces player.xml with the temp file. Returns the undo backup.
    /// </summary>
    Task<BackupInfo?> EditPlayerXmlAsync(Action<string, string> edit, CancellationToken ct = default);
    IReadOnlyList<BackupInfo> GetAvailableBackups();
    CleanupResult CleanupOldBackups(int maxVersions, int maxUndoBackups);
    bool TogglePreservation(string backupName);
    IReadOnlyList<string> RecoverInterruptedOperations();
}

public sealed record BackupServiceOptions
{
    /// <summary>While Noita runs, wait until no file in save00 was written for this long before copying.</summary>
    public TimeSpan QuietPeriod { get; init; } = TimeSpan.FromSeconds(2);
    public TimeSpan MaxQuietWait { get; init; } = TimeSpan.FromSeconds(20);
    public int MaxCopyAttempts { get; init; } = 3;
    public TimeSpan RetryDelay { get; init; } = TimeSpan.FromMilliseconds(750);
}

public sealed partial class BackupService : IBackupService
{
    public const string PartialPrefix = ".partial-";
    public const string StagingSuffix = ".scummer-staging";
    public const string OldSuffix = ".scummer-old";
    private const string TimestampFormat = "yyyy-MM-dd_HH-mm-ss";

    private readonly string _savePath;
    private readonly string _backupsPath;
    private readonly BackupServiceOptions _options;
    private readonly PreservationService _preservation;

    public BackupService(string savePath, string backupsPath, BackupServiceOptions? options = null)
    {
        _savePath = Path.TrimEndingDirectorySeparator(Path.GetFullPath(savePath));
        _backupsPath = Path.GetFullPath(backupsPath);
        _options = options ?? new BackupServiceOptions();
        Directory.CreateDirectory(_backupsPath);
        _preservation = new PreservationService(_backupsPath);
    }

    public string? PreservationWarning => _preservation.LoadWarning;

    [GeneratedRegex(@"^(\d{4}-\d{2}-\d{2}_\d{2}-\d{2}-\d{2})(?:_(\d+))?$")]
    private static partial Regex BackupNamePattern();

    // ---------------------------------------------------------------- backup

    public async Task<BackupResult> CreateBackupAsync(BackupRequest request, CancellationToken ct = default)
    {
        if (request.RequireActiveRun && !SaveLayout.HasActiveRun(_savePath))
            return new(BackupOutcome.SkippedNoActiveRun, "No run in progress (no player.xml/world in save00); nothing to back up.");
        if (!Directory.Exists(_savePath))
            return new(BackupOutcome.SkippedNoActiveRun, "Save directory does not exist; nothing to back up.");

        Directory.CreateDirectory(_backupsPath);
        var quietDeadline = DateTime.UtcNow + _options.MaxQuietWait;
        string lastError = "unknown error";
        var attempts = 0;

        while (attempts < _options.MaxCopyAttempts)
        {
            ct.ThrowIfCancellationRequested();
            var before = SnapshotManifest.Capture(_savePath);

            // Noita streams chunks to disk while you play. Copying in the middle of that produces a snapshot
            // whose files come from different moments, which is what desyncs backgrounds from terrain.
            if (request.NoitaRunning &&
                DateTime.UtcNow - before.NewestWriteUtc < _options.QuietPeriod &&
                DateTime.UtcNow < quietDeadline)
            {
                await Task.Delay(_options.RetryDelay, ct);
                continue;
            }

            var hash = before.ComputeHash();
            if (request.SkipIfUnchanged &&
                GetAvailableBackups().FirstOrDefault(b => b.Kind != BackupKind.PreRestore)?.Metadata?.ManifestHash == hash)
            {
                return new(BackupOutcome.SkippedUnchanged, "Save unchanged since the last backup; skipped.");
            }

            attempts++;
            var name = UniqueName(DateTime.Now);
            var partial = Path.Combine(_backupsPath, PartialPrefix + name);
            try
            {
                await Task.Run(() => FileOps.CopyDirectory(_savePath, partial, null, ct), ct);

                var after = SnapshotManifest.Capture(_savePath);
                if (!before.SameAs(after))
                {
                    lastError = "the save changed while it was being copied";
                    FileOps.TryDeleteDirectory(partial);
                    await Task.Delay(_options.RetryDelay, ct);
                    continue;
                }

                var metadata = new BackupMetadata
                {
                    CreatedUtc = DateTime.UtcNow,
                    Kind = request.Kind,
                    NoitaWasRunning = request.NoitaRunning,
                    FileCount = before.FileCount,
                    TotalBytes = before.TotalBytes,
                    ManifestHash = hash,
                };
                await File.WriteAllTextAsync(
                    Path.Combine(partial, SaveLayout.MetadataFileName),
                    JsonSerializer.Serialize(metadata, NoitaSaveScummerJsonContext.Default.BackupMetadata), ct);

                var finalPath = Path.Combine(_backupsPath, name);
                Directory.Move(partial, finalPath);

                var info = ReadBackup(finalPath)
                    ?? throw new InvalidOperationException($"Backup {name} was written but could not be read back.");
                var quality = request.NoitaRunning ? "live, Noita running" : "clean";
                return new(BackupOutcome.Created, $"Backup {info.FormattedTimestamp} created ({quality}).", info);
            }
            catch (OperationCanceledException)
            {
                FileOps.TryDeleteDirectory(partial);
                throw;
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                lastError = ex.Message;
                FileOps.TryDeleteDirectory(partial);
                await Task.Delay(_options.RetryDelay, ct);
            }
        }

        return new(BackupOutcome.Failed,
            $"Backup failed after {attempts} attempt(s): {lastError}. Noita kept writing; try again, or Save & Quit first for a clean backup.");
    }

    // ---------------------------------------------------------------- restore

    public async Task<RestoreResult> RestoreBackupAsync(string backupName, bool keepCurrentProgress, CancellationToken ct = default)
    {
        var backup = FindBackup(backupName);
        if (backup.Kind != BackupKind.PreRestore && !SaveLayout.HasActiveRun(backup.FolderPath))
            throw new InvalidOperationException($"Backup {backup.FormattedTimestamp} has no player.xml/world folder; it cannot be restored.");

        RecoverInterruptedOperations();
        var undo = await CreateUndoBackupAsync(ct);
        await Task.Run(() => SwapInBackup(backup.FolderPath, keepCurrentProgress, ct), ct);
        return new RestoreResult(backup, undo);
    }

    public async Task<RestoreResult> RestorePlayerOnlyAsync(string backupName, (double X, double Y)? resetPosition, CancellationToken ct = default)
    {
        var backup = FindBackup(backupName);
        var source = Path.Combine(backup.FolderPath, SaveLayout.PlayerFile);
        if (!File.Exists(source))
            throw new InvalidOperationException($"Backup {backup.FormattedTimestamp} has no player.xml.");
        if (!Directory.Exists(_savePath))
            throw new InvalidOperationException("Save directory does not exist; start a run in Noita first.");

        var undo = await CreateUndoBackupAsync(ct);

        var destination = Path.Combine(_savePath, SaveLayout.PlayerFile);
        var temp = destination + ".scummer-tmp";
        await Task.Run(() =>
        {
            if (resetPosition is { } p)
                PlayerXml.WriteWithPosition(source, temp, p.X, p.Y);
            else
                File.Copy(source, temp, overwrite: true);
            File.Move(temp, destination, overwrite: true);
        }, ct);

        return new RestoreResult(backup, undo);
    }

    public async Task<BackupInfo?> EditPlayerXmlAsync(Action<string, string> edit, CancellationToken ct = default)
    {
        var current = Path.Combine(_savePath, SaveLayout.PlayerFile);
        if (!File.Exists(current))
            throw new InvalidOperationException("No player.xml in save00. Start a run, Save & Quit, then try again.");

        var undo = await CreateUndoBackupAsync(ct);
        var temp = current + ".scummer-tmp";
        await Task.Run(() =>
        {
            try
            {
                edit(current, temp);
                File.Move(temp, current, overwrite: true);
            }
            finally
            {
                if (File.Exists(temp)) File.Delete(temp);
            }
        }, ct);
        return undo;
    }

    private async Task<BackupInfo?> CreateUndoBackupAsync(CancellationToken ct)
    {
        if (!Directory.Exists(_savePath) || !Directory.EnumerateFileSystemEntries(_savePath).Any())
            return null;

        var result = await CreateBackupAsync(
            new BackupRequest(BackupKind.PreRestore, NoitaRunning: false, SkipIfUnchanged: false, RequireActiveRun: false), ct);
        if (result.Outcome != BackupOutcome.Created)
            throw new InvalidOperationException($"Could not create the undo backup ({result.Message}). Restore aborted; nothing was changed.");
        return result.Backup;
    }

    /// <summary>
    /// Builds the complete new save00 next to the real one, then swaps directories with two renames.
    /// The live save is never half-written: either the old or the new save00 exists at every moment.
    /// </summary>
    private void SwapInBackup(string backupFolder, bool keepCurrentProgress, CancellationToken ct)
    {
        var (staging, old) = SwapPaths();
        FileOps.DeleteDirectory(staging);

        var keptFromCurrent = keepCurrentProgress && Directory.Exists(_savePath)
            ? SaveLayout.PersistentEntries
                .Where(e => File.Exists(Path.Combine(_savePath, e)) || Directory.Exists(Path.Combine(_savePath, e)))
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        try
        {
            FileOps.CopyDirectory(backupFolder, staging, relative =>
                !relative.Equals(SaveLayout.MetadataFileName, StringComparison.OrdinalIgnoreCase) &&
                !keptFromCurrent.Contains(SaveLayout.FirstSegment(relative)), ct);

            foreach (var entry in keptFromCurrent)
            {
                var source = Path.Combine(_savePath, entry);
                var destination = Path.Combine(staging, entry);
                if (Directory.Exists(source)) FileOps.CopyDirectory(source, destination, null, ct);
                else File.Copy(source, destination, overwrite: true);
            }
        }
        catch
        {
            FileOps.TryDeleteDirectory(staging);
            throw;
        }

        try
        {
            if (Directory.Exists(_savePath))
                Directory.Move(_savePath, old);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            FileOps.TryDeleteDirectory(staging);
            throw new IOException(
                $"save00 is in use by another program (Noita, Steam Cloud sync, an Explorer window?). Nothing was changed. ({ex.Message})", ex);
        }

        try
        {
            Directory.Move(staging, _savePath);
        }
        catch
        {
            if (!Directory.Exists(_savePath) && Directory.Exists(old))
                Directory.Move(old, _savePath);
            throw;
        }

        FileOps.TryDeleteDirectory(old);
    }

    private (string Staging, string Old) SwapPaths() => (_savePath + StagingSuffix, _savePath + OldSuffix);

    /// <summary>Cleans up after a crash/power loss in the middle of a backup or restore.</summary>
    public IReadOnlyList<string> RecoverInterruptedOperations()
    {
        var messages = new List<string>();
        var (staging, old) = SwapPaths();

        if (!Directory.Exists(_savePath) && Directory.Exists(old))
        {
            Directory.Move(old, _savePath);
            messages.Add("Recovered save00 from an interrupted restore.");
        }
        if (Directory.Exists(old) && FileOps.TryDeleteDirectory(old))
            messages.Add("Removed leftover data from an interrupted restore.");
        FileOps.TryDeleteDirectory(staging);

        if (Directory.Exists(_backupsPath))
        {
            foreach (var partial in Directory.EnumerateDirectories(_backupsPath, PartialPrefix + "*", FileOps.TopLevelAll))
            {
                if (FileOps.TryDeleteDirectory(partial))
                    messages.Add($"Removed incomplete backup {Path.GetFileName(partial)[PartialPrefix.Length..]}.");
            }
        }
        if (_preservation.LoadWarning is { } warning) messages.Add(warning);
        return messages;
    }

    // ---------------------------------------------------------------- listing / retention

    public IReadOnlyList<BackupInfo> GetAvailableBackups()
    {
        if (!Directory.Exists(_backupsPath)) return [];
        return Directory.EnumerateDirectories(_backupsPath, "*", FileOps.TopLevelAll)
            .Select(ReadBackup)
            .OfType<BackupInfo>()
            .OrderByDescending(b => b.Timestamp)
            .ThenByDescending(b => b.Sequence)
            .ToList();
    }

    private BackupInfo? ReadBackup(string folder)
    {
        var name = Path.GetFileName(folder);
        var match = BackupNamePattern().Match(name);
        if (!match.Success ||
            !DateTime.TryParseExact(match.Groups[1].Value, TimestampFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var timestamp))
            return null;

        BackupMetadata? metadata = null;
        var metadataPath = Path.Combine(folder, SaveLayout.MetadataFileName);
        if (File.Exists(metadataPath))
        {
            try
            {
                metadata = JsonSerializer.Deserialize(File.ReadAllText(metadataPath), NoitaSaveScummerJsonContext.Default.BackupMetadata);
            }
            catch (JsonException)
            {
                metadata = null; // shown as [LEGACY]; contents are still a full copy
            }
        }

        return new BackupInfo
        {
            Name = name,
            Timestamp = timestamp,
            Sequence = match.Groups[2].Success ? int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture) : 1,
            FolderPath = folder,
            IsPreserved = _preservation.IsPreserved(name),
            Metadata = metadata,
        };
    }

    private BackupInfo FindBackup(string backupName) =>
        GetAvailableBackups().FirstOrDefault(b => b.Name == backupName)
        ?? throw new InvalidOperationException($"Backup '{backupName}' not found.");

    private string UniqueName(DateTime now)
    {
        var baseName = now.ToString(TimestampFormat, CultureInfo.InvariantCulture);
        var candidate = baseName;
        for (var i = 2; Exists(candidate); i++)
            candidate = $"{baseName}_{i.ToString(CultureInfo.InvariantCulture)}";
        return candidate;

        bool Exists(string n) =>
            Directory.Exists(Path.Combine(_backupsPath, n)) || Directory.Exists(Path.Combine(_backupsPath, PartialPrefix + n));
    }

    public CleanupResult CleanupOldBackups(int maxVersions, int maxUndoBackups)
    {
        var all = GetAvailableBackups();
        var toDelete = all.Where(b => !b.IsPreserved && b.Kind != BackupKind.PreRestore).Skip(maxVersions)
            .Concat(all.Where(b => !b.IsPreserved && b.Kind == BackupKind.PreRestore).Skip(maxUndoBackups))
            .ToList();

        var deleted = new List<string>();
        var errors = new List<string>();
        foreach (var backup in toDelete)
        {
            try
            {
                FileOps.DeleteDirectory(backup.FolderPath);
                deleted.Add(backup.Name);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                errors.Add($"{backup.Name}: {ex.Message}");
            }
        }

        _preservation.RemoveMissing(GetAvailableBackups().Select(b => b.Name));
        return new CleanupResult(deleted, errors);
    }

    public bool TogglePreservation(string backupName)
    {
        var backup = FindBackup(backupName);
        var preserve = !backup.IsPreserved;
        _preservation.SetPreserved(backup.Name, preserve);
        return preserve;
    }
}
