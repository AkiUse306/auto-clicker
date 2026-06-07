using System;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Threading;

namespace AutoClickerGui;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _timer;
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

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
        _timer.Tick += (_, _) => ClickOnce();

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

        var intervalMs = Math.Max(1, 1000 / _cps);
        _timer.Interval = TimeSpan.FromMilliseconds(intervalMs);
        _timer.Start();
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

        _timer.Stop();
    }

    private void ClickOnce()
    {
        var (down, up) = GetButtonFlags(_buttonComboBox.SelectedItem?.ToString());
        SendClick(down, up);
    }

    private static void SendClick(uint downFlags, uint upFlags)
    {
        mouse_event(downFlags, 0, 0, 0, UIntPtr.Zero);
        mouse_event(upFlags, 0, 0, 0, UIntPtr.Zero);
    }

    private static void SendMouseDown(uint flags)
    {
        mouse_event(flags, 0, 0, 0, UIntPtr.Zero);
    }

    private static void SendMouseUp(uint flags)
    {
        mouse_event(flags, 0, 0, 0, UIntPtr.Zero);
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

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
}