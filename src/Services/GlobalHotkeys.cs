using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading.Channels;

namespace NoitaSaveScummer.Services;

public enum HotkeyAction
{
    QuickSave = 1,
    QuickLoad = 2,
}

public sealed record HotkeyBinding(HotkeyAction Action, uint Modifiers, uint VirtualKey, string Label);

public static class HotkeyBindings
{
    public const uint ModAlt = 0x0001;
    public const uint ModControl = 0x0002;

    public static readonly IReadOnlyList<HotkeyBinding> All =
    [
        new(HotkeyAction.QuickSave, ModControl | ModAlt, 0x74 /* VK_F5 */, "Ctrl+Alt+F5"),
        new(HotkeyAction.QuickLoad, ModControl | ModAlt, 0x78 /* VK_F9 */, "Ctrl+Alt+F9"),
    ];

    public static string LabelFor(HotkeyAction action) => All.First(b => b.Action == action).Label;
}

/// <summary>System-wide hotkeys (RegisterHotKey) so quick-save/quick-load work while Noita has focus.</summary>
public sealed class GlobalHotkeys : IDisposable
{
    private const uint ModNoRepeat = 0x4000;
    private const uint WmHotkey = 0x0312;
    private const uint WmQuit = 0x0012;

    private readonly ChannelWriter<HotkeyAction> _writer;
    private Thread? _thread;
    private uint _threadId;

    public GlobalHotkeys(ChannelWriter<HotkeyAction> writer)
    {
        _writer = writer;
    }

    /// <summary>Registers all bindings on a dedicated message-loop thread. Returns labels that failed to register.</summary>
    [SupportedOSPlatform("windows")]
    public IReadOnlyList<string> Start()
    {
        var failures = new List<string>();
        using var ready = new ManualResetEventSlim();

        _thread = new Thread(() =>
        {
            _threadId = GetCurrentThreadId();
            var registered = new List<int>();
            foreach (var binding in HotkeyBindings.All)
            {
                if (RegisterHotKey(IntPtr.Zero, (int)binding.Action, binding.Modifiers | ModNoRepeat, binding.VirtualKey))
                    registered.Add((int)binding.Action);
                else
                    failures.Add(binding.Label);
            }
            ready.Set();

            try
            {
                while (GetMessage(out var msg, IntPtr.Zero, 0, 0) > 0)
                {
                    if (msg.Message == WmHotkey)
                        _writer.TryWrite((HotkeyAction)(int)msg.WParam);
                }
            }
            finally
            {
                foreach (var id in registered) UnregisterHotKey(IntPtr.Zero, id);
            }
        })
        {
            IsBackground = true,
            Name = "GlobalHotkeys",
        };

        _thread.Start();
        ready.Wait();
        return failures;
    }

    public void Dispose()
    {
        if (_thread is { IsAlive: true })
        {
            PostThreadMessage(_threadId, WmQuit, UIntPtr.Zero, IntPtr.Zero);
            _thread.Join(TimeSpan.FromSeconds(1));
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Msg
    {
        public IntPtr Hwnd;
        public uint Message;
        public UIntPtr WParam;
        public IntPtr LParam;
        public uint Time;
        public int PointX;
        public int PointY;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("user32.dll")]
    private static extern int GetMessage(out Msg lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool PostThreadMessage(uint idThread, uint msg, UIntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();
}
