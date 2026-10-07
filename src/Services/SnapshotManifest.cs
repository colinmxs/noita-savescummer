using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace NoitaSaveScummer.Services;

/// <summary>Size + last-write stamp of every file under a directory. Used to detect writes during a copy.</summary>
public sealed class SnapshotManifest
{
    public readonly record struct FileStamp(long Length, long LastWriteTicks);

    private readonly SortedDictionary<string, FileStamp> _entries;

    private SnapshotManifest(SortedDictionary<string, FileStamp> entries, DateTime newestWriteUtc)
    {
        _entries = entries;
        NewestWriteUtc = newestWriteUtc;
    }

    public int FileCount => _entries.Count;
    public long TotalBytes => _entries.Values.Sum(e => e.Length);
    public DateTime NewestWriteUtc { get; }

    public static SnapshotManifest Capture(string root)
    {
        var entries = new SortedDictionary<string, FileStamp>(StringComparer.Ordinal);
        var newest = DateTime.MinValue;
        var dir = new DirectoryInfo(root);
        if (dir.Exists)
        {
            foreach (var file in dir.EnumerateFiles("*", FileOps.RecursiveAll))
            {
                var relative = Path.GetRelativePath(root, file.FullName).Replace('\\', '/');
                var written = file.LastWriteTimeUtc;
                entries[relative] = new FileStamp(file.Length, written.Ticks);
                if (written > newest) newest = written;
            }
        }
        return new SnapshotManifest(entries, newest);
    }

    public bool SameAs(SnapshotManifest other) =>
        _entries.Count == other._entries.Count &&
        _entries.All(kv => other._entries.TryGetValue(kv.Key, out var o) && o == kv.Value);

    public string ComputeHash()
    {
        var sb = new StringBuilder();
        foreach (var (path, stamp) in _entries)
        {
            sb.Append(path).Append('\t')
              .Append(stamp.Length.ToString(CultureInfo.InvariantCulture)).Append('\t')
              .Append(stamp.LastWriteTicks.ToString(CultureInfo.InvariantCulture)).Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }
}
