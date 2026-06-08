using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AutoClickerGui;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _cts;
    private readonly Button _toggleButton;
    private readonly Slider _cpsSlider;
    private readonly TextBlock _cpsLabel;
    private readonly ComboBox _buttonComboBox;
    private readonly CheckBox _holdModeBox;
    private readonly TextBlock _statusLabel;

    private bool _running;
    private int _cps = 10;

    public MainWindow()
    {
        InitializeComponent();

        _toggleButton = this.FindControl<Button>("ToggleButton")!;
        _cpsSlider = this.FindControl<Slider>("CpsSlider")!;
        _cpsLabel = this.FindControl<TextBlock>("CpsLabel")!;
        _buttonComboBox = this.FindControl<ComboBox>("ButtonComboBox")!;
        _holdModeBox = this.FindControl<CheckBox>("HoldModeBox")!;
        _statusLabel = this.FindControl<TextBlock>("StatusLabel")!;

        _buttonComboBox.SelectedIndex = 0;
        _cpsSlider.ValueChanged += (_, _) => UpdateCpsLabel();
        _toggleButton.Click += ToggleClicker;

        UpdateCpsLabel();
    }

    private void UpdateCpsLabel()
    {
        _cps = (int)Math.Round(_cpsSlider.Value);
        _cpsLabel.Text = $"{_cps} CPS";
    }

    private void ToggleClicker(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_running)
        {
            StopClicker();
            return;
        }

        StartClicker();
    }

    private void StartClicker()
    {
        _running = true;
        _toggleButton.Content = "Stop Auto Clicker";
        _statusLabel.Text = _holdModeBox.IsChecked == true
            ? $"Holding {_buttonComboBox.SelectedItem?.ToString()?.ToLowerInvariant()} clicks at {_cps} CPS"
            : $"Clicking {_buttonComboBox.SelectedItem?.ToString()?.ToLowerInvariant()} at {_cps} CPS";

        if (_holdModeBox.IsChecked == true)
        {
            var (down, _) = GetButtonFlags(_buttonComboBox.SelectedItem?.ToString());
            SendMouseDown(down);
            return;
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // Start background click loop for more accurate timing
        Task.Run(async () =>
        {
            var intervalMs = Math.Max(1, 1000.0 / Math.Max(1, _cps));
            var interval = TimeSpan.FromMilliseconds(intervalMs);
            var sw = new Stopwatch();

            try
            {
                while (!token.IsCancellationRequested)
                {
                    sw.Restart();
                    ClickOnce();
                    sw.Stop();

                    var remaining = interval - sw.Elapsed;
                    if (remaining > TimeSpan.Zero)
                        await Task.Delay(remaining, token).ConfigureAwait(false);
                    else
                        await Task.Yield();
                }
            }
            catch (TaskCanceledException) { }
        }, token);
    }

    private void StopClicker()
    {
        _running = false;
        _toggleButton.Content = "Start Auto Clicker";
        _statusLabel.Text = "Ready to click.";

        if (_holdModeBox.IsChecked == true)
        {
            var (_, up) = GetButtonFlags(_buttonComboBox.SelectedItem?.ToString());
            SendMouseUp(up);
        }

        if (_cts != null)
        {
            _cts.Cancel();
            _cts.Dispose();
            _cts = null;
        }
    }

    private void ClickOnce()
    {
        var (down, up) = GetButtonFlags(_buttonComboBox.SelectedItem?.ToString());
        SendClick(down, up);
    }

    private static void SendClick(uint downFlags, uint upFlags)
    {
        NativeMouse.SendClick(downFlags, upFlags);
    }

    private static void SendMouseDown(uint flags)
    {
        NativeMouse.SendMouseDown(flags);
    }

    private static void SendMouseUp(uint flags)
    {
        NativeMouse.SendMouseUp(flags);
    }

    private static (uint down, uint up) GetButtonFlags(string? selection)
    {
        return selection switch
        {
            "Right" => (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP),
            "Middle" => (MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP),
            _ => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP),
        };
    }

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
}