using System;
using System.Runtime.InteropServices;

namespace AutoClickerGui;

public static class NativeMouse
{
    public static void SendClick(uint downFlags, uint upFlags)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Windows_Click(downFlags, upFlags);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Linux_Click(downFlags, upFlags);
            return;
        }

        throw new PlatformNotSupportedException("Mouse events are not implemented for this OS yet. macOS support requires additional permissions and implementation.");
    }

    public static void SendMouseDown(uint flags)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            mouse_event_windows(flags, 0, 0, 0, UIntPtr.Zero);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var btn = XButtonFromFlags(flags);
            Linux_SendButton(btn, true);
            return;
        }

        throw new PlatformNotSupportedException("Mouse down not implemented for this OS.");
    }

    public static void SendMouseUp(uint flags)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            mouse_event_windows(flags, 0, 0, 0, UIntPtr.Zero);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var btn = XButtonFromFlags(flags);
            Linux_SendButton(btn, false);
            return;
        }

        throw new PlatformNotSupportedException("Mouse up not implemented for this OS.");
    }

    private static void Windows_Click(uint down, uint up)
    {
        mouse_event_windows(down, 0, 0, 0, UIntPtr.Zero);
        mouse_event_windows(up, 0, 0, 0, UIntPtr.Zero);
    }

    private static void Linux_Click(uint down, uint up)
    {
        var btn = XButtonFromFlags(down);
        Linux_SendButton(btn, true);
        Linux_SendButton(btn, false);
    }

    private static uint XButtonFromFlags(uint flags)
    {
        // map to X11 button numbers: left=1, middle=2, right=3
        if ((flags & 0x0008) != 0) return 3; // right down
        if ((flags & 0x0020) != 0) return 2; // middle down
        return 1; // default left
    }

    #region Windows pinvoke
    [DllImport("user32.dll", EntryPoint = "mouse_event", SetLastError = true)]
    private static extern void mouse_event_windows(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);
    #endregion

    #region Linux (X11 + XTest)
    [DllImport("libX11.so.6", EntryPoint = "XOpenDisplay")]
    private static extern IntPtr XOpenDisplay(IntPtr display);

    [DllImport("libX11.so.6", EntryPoint = "XCloseDisplay")]
    private static extern int XCloseDisplay(IntPtr display);

    [DllImport("libX11.so.6", EntryPoint = "XFlush")]
    private static extern int XFlush(IntPtr display);

    [DllImport("libXtst.so.6", EntryPoint = "XTestFakeButtonEvent")]
    private static extern int XTestFakeButtonEvent(IntPtr display, uint button, bool is_press, ulong delay);

    private static void Linux_SendButton(uint button, bool isPress)
    {
        IntPtr dsp = XOpenDisplay(IntPtr.Zero);
        if (dsp == IntPtr.Zero)
            throw new InvalidOperationException("Cannot open X display for sending mouse events.");

        XTestFakeButtonEvent(dsp, button, isPress, 0);
        XFlush(dsp);
        XCloseDisplay(dsp);
    }
    #endregion
}
