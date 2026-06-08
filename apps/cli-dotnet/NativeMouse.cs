using System;
using System.Runtime.InteropServices;

namespace AutoClickerCli;

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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            macOS_Click(downFlags, upFlags);
            return;
        }

        throw new PlatformNotSupportedException("Mouse events are not implemented for this OS.");
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var btn = macOS_ButtonFromFlags(flags);
            macOS_SendButton(btn, true);
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

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var btn = macOS_ButtonFromFlags(flags);
            macOS_SendButton(btn, false);
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

    private static void macOS_Click(uint down, uint up)
    {
        var btn = macOS_ButtonFromFlags(down);
        macOS_SendButton(btn, true);
        macOS_SendButton(btn, false);
    }

    private static uint XButtonFromFlags(uint flags)
    {
        // map to X11 button numbers: left=1, middle=2, right=3
        if ((flags & 0x0008) != 0) return 3; // right down
        if ((flags & 0x0020) != 0) return 2; // middle down
        return 1; // default left
    }

    private static uint macOS_ButtonFromFlags(uint flags)
    {
        // macOS CGMouseButton: left=0, right=1, center=2
        if ((flags & 0x0008) != 0) return 1; // right down
        if ((flags & 0x0020) != 0) return 2; // middle down
        return 0; // default left
    }

    public static (uint down, uint up) GetButtonFlagsForCli(string button)
    {
        return button.ToLowerInvariant() switch
        {
            "right" => (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP),
            "middle" => (MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP),
            _ => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP),
        };
    }

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;

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

    #region macOS (CoreGraphics)
    [DllImport("CoreGraphics.framework/CoreGraphics", EntryPoint = "CGEventCreateMouseEvent")]
    private static extern IntPtr CGEventCreateMouseEvent(IntPtr proxy, uint type, CGPoint pos, uint button);

    [DllImport("CoreGraphics.framework/CoreGraphics", EntryPoint = "CGEventPost")]
    private static extern void CGEventPost(uint tap, IntPtr cgEvent);

    [DllImport("CoreGraphics.framework/CoreGraphics", EntryPoint = "CFRelease")]
    private static extern void CFRelease(IntPtr cf);

    [StructLayout(LayoutKind.Sequential)]
    private struct CGPoint
    {
        public double X;
        public double Y;

        public CGPoint(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    private const uint kCGEventMouseMoved = 1;
    private const uint kCGEventLeftMouseDown = 2;
    private const uint kCGEventLeftMouseUp = 3;
    private const uint kCGEventRightMouseDown = 6;
    private const uint kCGEventRightMouseUp = 7;
    private const uint kCGEventOtherMouseDown = 25;
    private const uint kCGEventOtherMouseUp = 26;
    private const uint kCGHIDEventTap = 0;

    private static void macOS_SendButton(uint button, bool isPress)
    {
        try
        {
            uint eventType = button switch
            {
                0 => isPress ? kCGEventLeftMouseDown : kCGEventLeftMouseUp,
                1 => isPress ? kCGEventRightMouseDown : kCGEventRightMouseUp,
                2 => isPress ? kCGEventOtherMouseDown : kCGEventOtherMouseUp,
                _ => kCGEventLeftMouseDown,
            };

            var pos = new CGPoint(0, 0);
            var evt = CGEventCreateMouseEvent(IntPtr.Zero, eventType, pos, button);
            if (evt != IntPtr.Zero)
            {
                CGEventPost(kCGHIDEventTap, evt);
                CFRelease(evt);
            }
        }
        catch
        {
            // macOS synthetic events may require entitlements; fail gracefully
        }
    }
    #endregion
}
