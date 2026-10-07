namespace NoitaSaveScummer.Services;

internal static class FileOps
{
    // AttributesToSkip = 0 so hidden files (e.g. world/.stream_info on Unix-like hosts) are never skipped.
    public static readonly EnumerationOptions TopLevelAll = new()
    {
        RecurseSubdirectories = false,
        AttributesToSkip = 0,
        IgnoreInaccessible = false,
    };

    public static readonly EnumerationOptions RecursiveAll = new()
    {
        RecurseSubdirectories = true,
        AttributesToSkip = 0,
        IgnoreInaccessible = false,
    };

    /// <summary>
    /// Recursively copies <paramref name="source"/> into <paramref name="destination"/>.
    /// <paramref name="include"/> receives save-relative paths with forward slashes (directories without a trailing slash).
    /// </summary>
    public static void CopyDirectory(string source, string destination, Func<string, bool>? include, CancellationToken ct, string relativePrefix = "")
    {
        Directory.CreateDirectory(destination);

        foreach (var file in Directory.EnumerateFiles(source, "*", TopLevelAll))
        {
            ct.ThrowIfCancellationRequested();
            var name = Path.GetFileName(file);
            if (include is not null && !include(relativePrefix + name)) continue;
            File.Copy(file, Path.Combine(destination, name), overwrite: false);
        }

        foreach (var directory in Directory.EnumerateDirectories(source, "*", TopLevelAll))
        {
            ct.ThrowIfCancellationRequested();
            var name = Path.GetFileName(directory);
            var relative = relativePrefix + name;
            if (include is not null && !include(relative)) continue;
            CopyDirectory(directory, Path.Combine(destination, name), include, ct, relative + "/");
        }
    }

    /// <summary>Deletes a directory tree, clearing read-only flags first (players often mark bones_new read-only).</summary>
    public static void DeleteDirectory(string path)
    {
        if (!Directory.Exists(path)) return;
        foreach (var file in Directory.EnumerateFiles(path, "*", RecursiveAll))
        {
            var attributes = File.GetAttributes(file);
            if (attributes.HasFlag(FileAttributes.ReadOnly))
                File.SetAttributes(file, attributes & ~FileAttributes.ReadOnly);
        }
        Directory.Delete(path, recursive: true);
    }

    public static bool TryDeleteDirectory(string path)
    {
        try
        {
            DeleteDirectory(path);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    /// <summary>Writes text to a temp file next to the target and atomically replaces it.</summary>
    public static async Task WriteAllTextAtomicAsync(string path, string contents, CancellationToken ct = default)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var temp = path + ".tmp";
        await File.WriteAllTextAsync(temp, contents, ct);
        File.Move(temp, path, overwrite: true);
    }

    public static void WriteAllTextAtomic(string path, string contents)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var temp = path + ".tmp";
        File.WriteAllText(temp, contents);
        File.Move(temp, path, overwrite: true);
    }
}
