using System;
using System.Threading.Tasks;

namespace AutoClickerCli;

class Program
{
    static async Task Main(string[] args)
    {
        // Simple stdin command loop: commands are lines like
        // click left
        // down right
        // up left
        // The CLI runs until stdin closes.

        string? line;
        while ((line = Console.ReadLine()) != null)
        {
            try
            {
                var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                var cmd = parts[0].ToLowerInvariant();
                var btn = parts.Length > 1 ? parts[1].ToLowerInvariant() : "left";

                uint down, up;
                (down, up) = parts.Length > 1 ? NativeMouse.GetButtonFlagsForCli(btn) : NativeMouse.GetButtonFlagsForCli("left");

                if (cmd == "click")
                {
                    NativeMouse.SendClick(down, up);
                }
                else if (cmd == "down")
                {
                    NativeMouse.SendMouseDown(down);
                }
                else if (cmd == "up")
                {
                    NativeMouse.SendMouseUp(up);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
            }
        }

        await Task.CompletedTask;
    }
}
