namespace NoitaSaveScummer.Services;

public sealed record AppPaths(string SavePath, string BackupBasePath)
{
    public string BackupsPath => Path.Combine(BackupBasePath, "backups");
    public string ConfigPath => Path.Combine(BackupBasePath, "config.json");

    /// <summary>
    /// Defaults to the standard Windows locations. Override with --save-path / --backup-path
    /// or the NOITA_SAVE_PATH / NOITA_SCUMMER_BACKUP_PATH environment variables.
    /// </summary>
    public static AppPaths Resolve(string[] args)
    {
        var save = ArgValue(args, "--save-path")
            ?? Environment.GetEnvironmentVariable("NOITA_SAVE_PATH")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "LocalLow", "Nolla_Games_Noita", "save00");

        var backups = ArgValue(args, "--backup-path")
            ?? Environment.GetEnvironmentVariable("NOITA_SCUMMER_BACKUP_PATH")
            ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "NoitaSaveBackups");

        return new AppPaths(Path.GetFullPath(save), Path.GetFullPath(backups));
    }

    private static string? ArgValue(string[] args, string name)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i].Equals(name, StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
                return args[i + 1];
            if (args[i].StartsWith(name + "=", StringComparison.OrdinalIgnoreCase))
                return args[i][(name.Length + 1)..];
        }
        return null;
    }
}
