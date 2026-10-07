namespace NoitaSaveScummer.UI;

public static class Prompts
{
    /// <summary>Shows a full-screen question and returns the pressed key among <paramref name="allowed"/> (Esc always cancels → null).</summary>
    public static ConsoleKey? Ask(ConsoleRenderer renderer, IReadOnlyList<string> lines, params ConsoleKey[] allowed)
    {
        renderer.Render(lines);
        while (true)
        {
            var key = Console.ReadKey(intercept: true).Key;
            if (key == ConsoleKey.Escape) return null;
            if (allowed.Contains(key)) return key;
        }
    }

    public static bool ConfirmCloseNoita(ConsoleRenderer renderer) =>
        Ask(renderer,
        [
            $"{IconProvider.Warning} Noita is running",
            string.Empty,
            "Restoring while Noita runs does not work: the game keeps the world in memory and",
            "overwrites save00 when it exits, mixing old and new files (misaligned backgrounds, missing structures).",
            string.Empty,
            "K   - Force-close Noita now and continue (progress since the last autosave is lost;",
            "      an undo backup of the current save is taken first)",
            "Esc - Cancel",
        ], ConsoleKey.K) == ConsoleKey.K;

    /// <summary>Returns true = reset to spawn, false = keep backup position, null = cancel.</summary>
    public static bool? AskResetLocation(ConsoleRenderer renderer, bool defaultReset, double x, double y)
    {
        var key = Ask(renderer,
        [
            $"{IconProvider.Player} Player-only restore: where should the player appear?",
            string.Empty,
            "Player-only restore combines the backup's player with the CURRENT world.",
            "Appearing where the backup was saved can drop you into terrain that has since changed.",
            string.Empty,
            $"Y     - Reset to the spawn point ({x:0.##}, {y:0.##})",
            "N     - Keep the position from the backup",
            $"Enter - Default ({(defaultReset ? "reset" : "keep")})",
            "Esc   - Cancel",
        ], ConsoleKey.Y, ConsoleKey.N, ConsoleKey.Enter);

        return key switch
        {
            ConsoleKey.Y => true,
            ConsoleKey.N => false,
            ConsoleKey.Enter => defaultReset,
            _ => null,
        };
    }

    public static bool Confirm(ConsoleRenderer renderer, string title, params string[] body)
    {
        var lines = new List<string> { title, string.Empty };
        lines.AddRange(body);
        lines.Add(string.Empty);
        lines.Add("Y - Yes    Esc - Cancel");
        return Ask(renderer, lines, ConsoleKey.Y) == ConsoleKey.Y;
    }
}
