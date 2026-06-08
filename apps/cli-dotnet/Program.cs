using System;
using System.Threading.Tasks;

namespace AutoClickerCli;

class Program
{
    static async Task Main(string[] args)
    {
        // Advanced stdin command loop supporting:
        // click left [jitter_min jitter_max] - single click with optional jitter
        // burst left count delay [jitter_min jitter_max] - burst clicks
        // down left - mouse down
        // up left - mouse up
        // hold left duration - hold mouse button

        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            try
            {
                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                var cmd = parts[0].ToLowerInvariant();
                var btn = parts.Length > 1 ? parts[1].ToLowerInvariant() : "left";
                var (down, up) = NativeMouse.GetButtonFlagsForCli(btn);

                switch (cmd)
                {
                    case "click":
                        {
                            NativeMouse.SendClick(down, up);
                            // Optional jitter sleep
                            if (parts.Length > 3 && int.TryParse(parts[2], out var jitterMin) && int.TryParse(parts[3], out var jitterMax))
                            {
                                var jitterMs = new Random().Next(jitterMin, jitterMax + 1);
                                if (jitterMs > 0) await Task.Delay(jitterMs);
                            }
                        }
                        break;

                    case "burst":
                        {
                            if (parts.Length < 4 || !int.TryParse(parts[2], out var count) || !int.TryParse(parts[3], out var delayMs))
                                continue;

                            for (int i = 0; i < count; i++)
                            {
                                NativeMouse.SendClick(down, up);
                                if (i < count - 1 && delayMs > 0) await Task.Delay(delayMs);
                            }
                        }
                        break;

                    case "down":
                        NativeMouse.SendMouseDown(down);
                        break;

                    case "up":
                        NativeMouse.SendMouseUp(up);
                        break;

                    case "hold":
                        {
                            if (parts.Length < 3 || !int.TryParse(parts[2], out var holdMs))
                                continue;

                            NativeMouse.SendMouseDown(down);
                            await Task.Delay(holdMs);
                            NativeMouse.SendMouseUp(up);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
            }
        }

        await Task.CompletedTask;
    }
}

