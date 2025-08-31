using System.Text;

namespace NoitaSaveScummer.UI;

public static class IconProvider
{
    private static readonly bool _supportsUnicode;
    
    static IconProvider()
    {
        // Check if console supports Unicode by testing current encoding
        _supportsUnicode = Console.OutputEncoding.EncodingName.Contains("UTF") || 
                          Console.OutputEncoding.CodePage == 65001; // UTF-8 code page
        
        // Try to enable UTF-8 for better Unicode support on Windows
        try
        {
            if (!_supportsUnicode && Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Console.OutputEncoding = Encoding.UTF8;
                _supportsUnicode = true;
            }
        }
        catch
        {
            // If we can't set UTF-8, fall back to ASCII icons
        }
    }
    
    // Application icons
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
    
    // Status icons
    public static string Success => _supportsUnicode ? "✅" : "[OK]";
    public static string Error => _supportsUnicode ? "❌" : "[ERR]";
    public static string Warning => _supportsUnicode ? "⚠️" : "[WARN]";
    public static string Info => _supportsUnicode ? "ℹ️" : "[INFO]";
    
    // Timer icons
    public static string Timer => _supportsUnicode ? "⏱️" : "[Time]";
    public static string Hourglass => _supportsUnicode ? "⏳" : "[Wait]";
    public static string Pause => _supportsUnicode ? "⏸️" : "[Pause]";
    public static string Play => _supportsUnicode ? "▶️" : "[Play]";
    
    // Separators
    public static string Separator => _supportsUnicode ? "═" : "=";
    
    public static void Initialize()
    {
        // This method can be called to ensure static initialization
        // and apply console encoding settings early in the application
    }
}
