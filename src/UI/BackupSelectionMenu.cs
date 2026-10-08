using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.UI;

public static class BackupSelectionMenu
{
    public static BackupInfo? Select(ConsoleRenderer renderer, IReadOnlyList<BackupInfo> backups, string title, string? hint = null) =>
        ListMenu.Select(renderer, backups, b => b.DisplayName, title, hint);
}
