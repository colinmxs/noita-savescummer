using System.Text.Json;

namespace NoitaSaveScummer.Services;

/// <summary>Stores which backups are protected from retention cleanup (backups/preserved_backups.json).</summary>
public sealed class PreservationService
{
    public const string FileName = "preserved_backups.json";

    private readonly string _filePath;
    private readonly Dictionary<string, bool> _preserved;

    public PreservationService(string backupsDirectory)
    {
        _filePath = Path.Combine(backupsDirectory, FileName);
        _preserved = Load(out var warning);
        LoadWarning = warning;
    }

    /// <summary>Set when the file existed but could not be read; the bad file is kept as *.corrupt.</summary>
    public string? LoadWarning { get; }

    public bool IsPreserved(string backupName) =>
        _preserved.TryGetValue(backupName, out var preserved) && preserved;

    public void SetPreserved(string backupName, bool preserve)
    {
        if (preserve) _preserved[backupName] = true;
        else _preserved.Remove(backupName);
        Save();
    }

    public void RemoveMissing(IEnumerable<string> existingBackupNames)
    {
        var existing = existingBackupNames.ToHashSet(StringComparer.Ordinal);
        var stale = _preserved.Keys.Where(k => !existing.Contains(k)).ToList();
        if (stale.Count == 0) return;
        foreach (var key in stale) _preserved.Remove(key);
        Save();
    }

    private Dictionary<string, bool> Load(out string? warning)
    {
        warning = null;
        if (!File.Exists(_filePath)) return new Dictionary<string, bool>(StringComparer.Ordinal);
        try
        {
            var json = File.ReadAllText(_filePath);
            var data = JsonSerializer.Deserialize(json, NoitaSaveScummerJsonContext.Default.DictionaryStringBoolean);
            return new Dictionary<string, bool>(data ?? [], StringComparer.Ordinal);
        }
        catch (JsonException ex)
        {
            var corruptPath = _filePath + ".corrupt";
            File.Move(_filePath, corruptPath, overwrite: true);
            warning = $"Preservation list was unreadable ({ex.Message}); moved to {Path.GetFileName(corruptPath)}.";
            return new Dictionary<string, bool>(StringComparer.Ordinal);
        }
    }

    private void Save()
    {
        var json = JsonSerializer.Serialize(_preserved, NoitaSaveScummerJsonContext.Default.DictionaryStringBoolean);
        FileOps.WriteAllTextAtomic(_filePath, json);
    }
}
