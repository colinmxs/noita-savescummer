namespace NoitaSaveScummer.UI;

/// <summary>
/// Flicker-free full-screen renderer: only lines that changed since the last frame are rewritten,
/// and the screen is cleared only when the window is resized or the frame is invalidated.
/// </summary>
public sealed class ConsoleRenderer
{
    private string[] _previous = [];
    private int _width = -1;
    private int _height = -1;

    public void Invalidate()
    {
        _previous = [];
        _width = -1;
    }

    public void Render(IReadOnlyList<string> lines)
    {
        int width, height;
        try
        {
            width = Math.Max(20, Console.WindowWidth);
            height = Math.Max(5, Console.WindowHeight);
        }
        catch (IOException)
        {
            return; // no console attached
        }

        if (width != _width || height != _height)
        {
            Console.Clear();
            _previous = [];
            _width = width;
            _height = height;
        }

        TryHideCursor();
        var count = Math.Min(lines.Count, height - 1);
        var frame = new string[count];
        for (var i = 0; i < count; i++)
        {
            frame[i] = Fit(lines[i], width);
            if (i >= _previous.Length || _previous[i] != frame[i])
            {
                Console.SetCursorPosition(0, i);
                Console.Write(frame[i]);
            }
        }

        for (var i = count; i < _previous.Length; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.Write(new string(' ', width - 2));
        }

        _previous = frame;
        Console.SetCursorPosition(0, Math.Min(count, height - 1));
    }

    // Leave 2 columns of slack: emoji render 2 cells wide but may count as 1 char.
    private static string Fit(string line, int width)
    {
        var max = width - 2;
        return line.Length > max ? line[..max] : line.PadRight(max);
    }

    private static void TryHideCursor()
    {
        try
        {
            Console.CursorVisible = false;
        }
        catch (Exception ex) when (ex is IOException or PlatformNotSupportedException)
        {
        }
    }
}
