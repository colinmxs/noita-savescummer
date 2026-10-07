using NoitaSaveScummer.Models;

namespace NoitaSaveScummer.UI;

public static class BackupSelectionMenu
{
    public const int PageSize = 9;

    /// <summary>Paged picker: arrows/PgUp/PgDn to move, 1-9 to pick on the current page, Enter to pick, Esc to cancel.</summary>
    public static BackupInfo? Select(ConsoleRenderer renderer, IReadOnlyList<BackupInfo> backups, string title, string? hint = null)
    {
        if (backups.Count == 0) return null;
        var cursor = 0;

        while (true)
        {
            var page = cursor / PageSize;
            var pages = (backups.Count + PageSize - 1) / PageSize;
            var lines = new List<string> { $"{IconProvider.Game} {title}", string.Empty };
            if (hint is not null)
            {
                lines.Add(hint);
                lines.Add(string.Empty);
            }

            for (var i = page * PageSize; i < Math.Min(backups.Count, (page + 1) * PageSize); i++)
            {
                var marker = i == cursor ? IconProvider.Pointer : " ";
                lines.Add($" {marker} {i - page * PageSize + 1}. {backups[i].DisplayName}");
            }

            lines.Add(string.Empty);
            lines.Add($"Page {page + 1}/{pages}   Up/Down move   Left/Right page   1-9 pick   Enter select   Esc cancel");
            renderer.Render(lines);

            var key = Console.ReadKey(intercept: true);
            switch (key.Key)
            {
                case ConsoleKey.Escape: return null;
                case ConsoleKey.Enter: return backups[cursor];
                case ConsoleKey.UpArrow: cursor = Math.Max(0, cursor - 1); break;
                case ConsoleKey.DownArrow: cursor = Math.Min(backups.Count - 1, cursor + 1); break;
                case ConsoleKey.LeftArrow or ConsoleKey.PageUp: cursor = Math.Max(0, (page - 1) * PageSize); break;
                case ConsoleKey.RightArrow or ConsoleKey.PageDown: cursor = Math.Min(backups.Count - 1, (page + 1) * PageSize); break;
                case ConsoleKey.Home: cursor = 0; break;
                case ConsoleKey.End: cursor = backups.Count - 1; break;
                default:
                    if (key.KeyChar is >= '1' and <= '9')
                    {
                        var index = page * PageSize + (key.KeyChar - '1');
                        if (index < backups.Count) return backups[index];
                    }
                    break;
            }
        }
    }
}
