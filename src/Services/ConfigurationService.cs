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

    public async Task<Configuration> LoadAsync()
    {
        try
        {
            if (!File.Exists(_configPath))
            {
                return new Configuration { BackupIntervalMinutes = 0, MaxBackupVersions = 5 };
            }

            var json = await File.ReadAllTextAsync(_configPath);
            var config = JsonSerializer.Deserialize<Configuration>(json);
            return config ?? new Configuration { BackupIntervalMinutes = 0, MaxBackupVersions = 5 };
        }
        catch
        {
            return new Configuration { BackupIntervalMinutes = 0, MaxBackupVersions = 5 };
        }
    }

    public async Task SaveAsync(Configuration configuration)
    {
        try
        {
            var json = JsonSerializer.Serialize(configuration, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_configPath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to save configuration: {ex.Message}", ex);
        }
    }
}
