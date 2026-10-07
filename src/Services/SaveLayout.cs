namespace NoitaSaveScummer.Services;

/// <summary>
/// Knowledge about Noita's save00 layout.
///
/// Per-run state (must always come from ONE moment in time, or the world desyncs):
///   player.xml, world_state.xml, session_numbers.salakieli, and everything in world/
///   (chunk .png_petri files, entities_*.bin, world_pixel_scenes.bin, world_sim.bin,
///   world_tree.bin and .stream_info, which stores the pixel-scene background list,
///   camera values, seed and the loaded-chunk table).
///
/// Cross-run progress (safe to keep from the current save when restoring):
///   persistent/ (unlocks, orbs, bones), stats/, mod_config.xml, mod_settings.bin.
/// </summary>
public static class SaveLayout
{
    public const string PlayerFile = "player.xml";
    public const string WorldDirectory = "world";
    public const string MetadataFileName = "backup.json";

    public static readonly IReadOnlyList<string> PersistentEntries =
        ["persistent", "stats", "mod_config.xml", "mod_settings.bin"];

    /// <summary>True when the directory holds a run that can be continued (Noita removes these on death / new game).</summary>
    public static bool HasActiveRun(string saveDirectory) =>
        File.Exists(Path.Combine(saveDirectory, PlayerFile)) &&
        Directory.Exists(Path.Combine(saveDirectory, WorldDirectory));

    /// <summary>True when a save-relative path (forward slashes) belongs to cross-run progress.</summary>
    public static bool IsPersistent(string relativePath) =>
        PersistentEntries.Contains(FirstSegment(relativePath), StringComparer.OrdinalIgnoreCase);

    public static string FirstSegment(string relativePath)
    {
        var slash = relativePath.IndexOf('/');
        return slash < 0 ? relativePath : relativePath[..slash];
    }
}
