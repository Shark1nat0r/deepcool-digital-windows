using DeepCool_Display.Devices;
using DeepCool_Display.Monitor;
using DeepCool_Display.Utils;

namespace DeepCool_Display;

public class TrayApplicationContext : ApplicationContext
{
    private readonly TrayIconManager _trayIconManager;
    private readonly CpuMonitor _cpuMonitor;
    private readonly IDisplayDevice _device;
    private readonly System.Windows.Forms.Timer _updateTimer;

    private const string CpuTemperatureOptionText = "Show CPU Temperature";
    private const string CpuUsageOptionText = "Show CPU Usage";

    private bool _isFahrenheit;
    private bool _showCpuUsage;
    private readonly Random _random = new Random();

    public TrayApplicationContext()
    {
        _isFahrenheit = Properties.Settings.Default.UseFahrenheit;
        _showCpuUsage = Properties.Settings.Default.ShowCpuUsage;

        // Detect the device first so a failure doesn't leave a stale tray icon behind
        _device = DeviceFactory.Create();
        _trayIconManager = new TrayIconManager();
        _cpuMonitor = new CpuMonitor();

        _updateTimer = new System.Windows.Forms.Timer { Interval = 1000 };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();

        _trayIconManager.TemperatureUnitToggled += TrayIconManager_TemperatureUnitToggled;
        _trayIconManager.DisplayModeToggled += TrayIconManager_DisplayModeToggled;
        _trayIconManager.ExitClicked += TrayIconManager_ExitClicked;
        _trayIconManager.AutoStartToggled += TrayIconManager_AutoStartToggled;

        InitializeAutoStartState();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        var status = _cpuMonitor.Read(_isFahrenheit);
        _device.SetDisplayMode(_showCpuUsage ? DisplayMode.CpuUsage : DisplayMode.CpuTemperature);
        _device.UpdateDisplay(status, _isFahrenheit);
    }

    private void TrayIconManager_TemperatureUnitToggled(object? sender, bool isFahrenheit)
    {
        _isFahrenheit = isFahrenheit;
    }

    private void TrayIconManager_DisplayModeToggled(object? sender, EventArgs e)
    {
        _showCpuUsage = !_showCpuUsage;
        _trayIconManager.SetDisplayModeText(_showCpuUsage ? CpuTemperatureOptionText : CpuUsageOptionText);
    }

    private static void TrayIconManager_ExitClicked(object? sender, EventArgs e)
    {
        Application.Exit();
    }

    private static void TrayIconManager_AutoStartToggled(object? sender, bool enabled)
    {
        try
        {
            AutoStartManager.SetAutoStart(enabled);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update Auto-Start setting: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void InitializeAutoStartState()
    {
        var isAutoStartEnabled = AutoStartManager.IsAutoStartEnabled();
        _trayIconManager.SetAutoStartMenuItemState(isAutoStartEnabled);

        _trayIconManager.SetTemperatureUnitState(_isFahrenheit);

        _trayIconManager.SetDisplayModeText(_showCpuUsage ? CpuTemperatureOptionText : CpuUsageOptionText);
        _trayIconManager.SetDisplayModeVisible(_device.SupportsDisplayModes);
        _trayIconManager.SetTooltipText($"DeepCool Display ({_device.Name})");
    }

    private void SaveSettings()
    {
        Properties.Settings.Default.ShowCpuUsage = _showCpuUsage;
        Properties.Settings.Default.UseFahrenheit = _isFahrenheit;

        Properties.Settings.Default.Save();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            SaveSettings();

            _updateTimer.Stop();
            _trayIconManager.Dispose();
            _cpuMonitor.Dispose();
            _device.Dispose();
        }

		base.Dispose(disposing);
	}
}
