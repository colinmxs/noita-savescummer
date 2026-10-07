using System.Text.Json;
using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Services;

public interface IConfigurationService
{
    Task<Configuration> LoadAsync();
    Task SaveAsync(Configuration configuration);
}

public class ConfigurationService : IConfigurationService
{
    private readonly string _configPath;

    public ConfigurationService(string configPath)
    {
        _configPath = configPath;
    }

    /// <summary>Set when config.json existed but was unreadable (it is kept as config.json.corrupt).</summary>
    public string? LoadWarning { get; private set; }

    public async Task<Configuration> LoadAsync()
    {
        if (!File.Exists(_configPath))
            return new Configuration { BackupIntervalMinutes = 0 };

        try
        {
            var json = await File.ReadAllTextAsync(_configPath);
            return JsonSerializer.Deserialize(json, NoitaSaveScummerJsonContext.Default.Configuration)
                   ?? new Configuration { BackupIntervalMinutes = 0 };
        }
        catch (JsonException ex)
        {
            File.Move(_configPath, _configPath + ".corrupt", overwrite: true);
            LoadWarning = $"config.json was unreadable ({ex.Message}); saved as config.json.corrupt.";
            return new Configuration { BackupIntervalMinutes = 0 };
        }
    }

    public async Task SaveAsync(Configuration configuration)
    {
        var json = JsonSerializer.Serialize(configuration, NoitaSaveScummerJsonContext.Default.Configuration);
        await FileOps.WriteAllTextAtomicAsync(_configPath, json);
    }
}
