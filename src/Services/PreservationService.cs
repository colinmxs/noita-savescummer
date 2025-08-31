using System.Text.Json;
using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.Services;

public class PreservationService
{
    private readonly string _preservationFilePath;
    private Dictionary<string, bool> _preservedBackups;

    public PreservationService(string saveDirectory)
    {
        _preservationFilePath = Path.Combine(saveDirectory, "preserved_backups.json");
        _preservedBackups = new Dictionary<string, bool>();
        LoadPreservationData();
    }

    public bool IsPreserved(string backupPath)
    {
        var fileName = Path.GetFileName(backupPath);
        return _preservedBackups.TryGetValue(fileName, out var preserved) && preserved;
    }

    public void SetPreservation(string backupPath, bool preserve)
    {
        var fileName = Path.GetFileName(backupPath);
        if (preserve)
        {
            _preservedBackups[fileName] = true;
        }
        else
        {
            _preservedBackups.Remove(fileName);
        }
        SavePreservationData();
    }

    public void TogglePreservation(string backupPath)
    {
        var fileName = Path.GetFileName(backupPath);
        var currentState = IsPreserved(backupPath);
        SetPreservation(backupPath, !currentState);
    }

    public void CleanupDeletedBackups(IEnumerable<string> existingBackupPaths)
    {
        var existingFileNames = existingBackupPaths.Select(Path.GetFileName).ToHashSet();
        var keysToRemove = _preservedBackups.Keys.Where(k => !existingFileNames.Contains(k)).ToList();
        
        foreach (var key in keysToRemove)
        {
            _preservedBackups.Remove(key);
        }
        
        if (keysToRemove.Count > 0)
        {
            SavePreservationData();
        }
    }

    private void LoadPreservationData()
    {
        try
        {
            if (File.Exists(_preservationFilePath))
            {
                var json = File.ReadAllText(_preservationFilePath);
                var options = new JsonSerializerOptions();
                _preservedBackups = JsonSerializer.Deserialize<Dictionary<string, bool>>(json, options) 
                                   ?? new Dictionary<string, bool>();
            }
        }
        catch
        {
            // If there's any error loading, start fresh
            _preservedBackups = new Dictionary<string, bool>();
        }
    }

    private void SavePreservationData()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_preservedBackups, options);
            File.WriteAllText(_preservationFilePath, json);
        }
        catch
        {
            // Silently fail to avoid disrupting application flow
        }
    }
}
