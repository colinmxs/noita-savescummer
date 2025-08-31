using System.Text.Json.Serialization;
using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.Services;

[JsonSerializable(typeof(Configuration))]
[JsonSerializable(typeof(BackupInfo))]
[JsonSerializable(typeof(ApplicationState))]
[JsonSerializable(typeof(Dictionary<string, bool>))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class NoitaSaveScummerJsonContext : JsonSerializerContext
{
}
