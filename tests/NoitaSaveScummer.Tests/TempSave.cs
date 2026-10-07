using NoitaSaveScummer.Services;

namespace NoitaSaveScummer.Tests;

/// <summary>Builds a fake save00 + backups directory under a temp folder.</summary>
public sealed class TempSave : IDisposable
{
    public string Root { get; } = Path.Combine(Path.GetTempPath(), "nss-tests-" + Guid.NewGuid().ToString("N"));
    public string SavePath => Path.Combine(Root, "save00");
    public string BackupsPath => Path.Combine(Root, "backups");

    public static readonly BackupServiceOptions FastOptions = new()
    {
        QuietPeriod = TimeSpan.Zero,
        MaxQuietWait = TimeSpan.Zero,
        RetryDelay = TimeSpan.FromMilliseconds(1),
        MaxCopyAttempts = 3,
    };

    public TempSave()
    {
        Directory.CreateDirectory(SavePath);
    }

    public BackupService CreateService(BackupServiceOptions? options = null) =>
        new(SavePath, BackupsPath, options ?? FastOptions);

    /// <summary>Writes a run that looks like Noita's: per-run files, world/ with chunk data and .stream_info, plus progress dirs.</summary>
    public void WriteRun(string tag, double x = 100, double y = 200)
    {
        Write("player.xml", PlayerXmlContent(x, y, tag));
        Write("world_state.xml", $"<Entity name=\"world_state\" tag=\"{tag}\" />");
        Write("session_numbers.salakieli", "session-" + tag);
        Write("world/.stream_info", "stream-" + tag);
        Write("world/world_pixel_scenes.bin", "scenes-" + tag);
        Write("world/world_0_0.png_petri", "chunk-" + tag);
        Write("world/entities_0.bin", "entities-" + tag);
        Write("persistent/flags/card_unlocked_" + tag, string.Empty);
        Write("stats/_stats.salakieli", "stats-" + tag);
        Write("mod_config.xml", "<Mods tag=\"" + tag + "\" />");
    }

    public static string PlayerXmlContent(double x, double y, string tag) =>
        "<Entity name=\"DEBUG_NAME:player\" tags=\"player_unit\">\n" +
        $"  <_Transform position.x=\"{x.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" " +
        $"position.y=\"{y.ToString(System.Globalization.CultureInfo.InvariantCulture)}\" rotation=\"0\" scale.x=\"1\" scale.y=\"1\" />\n" +
        $"  <Entity name=\"inventory_quick\" tag=\"{tag}\"><_Transform position.x=\"1\" position.y=\"2\" /></Entity>\n" +
        "</Entity>\n";

    public void Write(string relative, string content)
    {
        var path = Path.Combine(SavePath, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    public string Read(string relative) => File.ReadAllText(Path.Combine(SavePath, relative));

    public bool Exists(string relative) =>
        File.Exists(Path.Combine(SavePath, relative)) || Directory.Exists(Path.Combine(SavePath, relative));

    public void Dispose()
    {
        try
        {
            FileOps.DeleteDirectory(Root);
        }
        catch (IOException)
        {
        }
    }
}
