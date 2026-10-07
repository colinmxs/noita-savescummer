using System.Text.Json.Serialization;
using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Services;

// Every type that goes through System.Text.Json must be listed here. Release builds are trimmed,
// which disables reflection-based serialization (and so does the csproj, to catch mistakes in Debug).
[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(BackupMetadata))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class NoitaSaveScummerJsonContext : JsonSerializerContext
{
}
