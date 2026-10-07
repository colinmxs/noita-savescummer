using System.Text;

namespace NoitaSaveScummer.UI;

public static class IconProvider
{
    private static readonly bool _supportsUnicode;

    static IconProvider()
    {
        _supportsUnicode = Console.OutputEncoding.CodePage == 65001 ||
                           Console.OutputEncoding.EncodingName.Contains("UTF", StringComparison.OrdinalIgnoreCase);
        try
        {
            if (!_supportsUnicode && OperatingSystem.IsWindows())
            {
                Console.OutputEncoding = Encoding.UTF8;
                _supportsUnicode = true;
            }
        }
        catch (IOException)
        {
            // Can't switch encoding; fall back to ASCII icons.
        }
    }

    public static bool SupportsUnicode => _supportsUnicode;

    public static string Game => _supportsUnicode ? "🎮" : "[Game]";
    public static string Folder => _supportsUnicode ? "📁" : "[Dir]";
    public static string Save => _supportsUnicode ? "💾" : "[Save]";
    public static string Package => _supportsUnicode ? "📦" : "[Pack]";
    public static string Calendar => _supportsUnicode ? "📅" : "[Date]";
    public static string Document => _supportsUnicode ? "📝" : "[Log]";
    public static string Target => _supportsUnicode ? "🎯" : "[Ctrl]";
    public static string Wave => _supportsUnicode ? "👋" : "[Exit]";
    public static string Player => _supportsUnicode ? "👤" : "[User]";
    public static string Lock => _supportsUnicode ? "🔒" : "[LOCK]";
    public static string Unlock => _supportsUnicode ? "🔓" : "[UNLK]";
    public static string Pin => _supportsUnicode ? "📌" : "[PIN]";
    public static string Undo => _supportsUnicode ? "↩" : "[Undo]";
    public static string Bolt => _supportsUnicode ? "⚡" : "[Fast]";

    public static string Success => _supportsUnicode ? "✅" : "[OK]";
    public static string Error => _supportsUnicode ? "❌" : "[ERR]";
    public static string Warning => _supportsUnicode ? "⚠️" : "[WARN]";
    public static string Info => _supportsUnicode ? "ℹ️" : "[INFO]";

    public static string Timer => _supportsUnicode ? "⏱️" : "[Time]";
    public static string Hourglass => _supportsUnicode ? "⏳" : "[Wait]";
    public static string Pause => _supportsUnicode ? "⏸️" : "[Pause]";
    public static string Play => _supportsUnicode ? "▶️" : "[Play]";

    public static string Separator => _supportsUnicode ? "═" : "=";
    public static string Pointer => _supportsUnicode ? "▶" : ">";

    /// <summary>Forces the static constructor (console encoding setup) to run early.</summary>
    public static void Initialize()
    {
    }
}
