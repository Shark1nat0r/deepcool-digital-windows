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

    // Without the PawnIO driver there are no temperature readings, so only CPU usage can be shown
    private readonly bool _isPawnIoInstalled = LibreHardwareMonitor.PawnIo.PawnIo.IsInstalled;

    private bool _isFahrenheit;
    private bool _showCpuUsage;
    private bool _useCoreAverage;
    private int _updateIntervalSeconds;

    public TrayApplicationContext()
    {
        _isFahrenheit = Properties.Settings.Default.UseFahrenheit;
        _showCpuUsage = Properties.Settings.Default.ShowCpuUsage;
        _useCoreAverage = Properties.Settings.Default.UseCoreAverage;
        _updateIntervalSeconds = Math.Clamp(Properties.Settings.Default.UpdateIntervalSeconds,
            TrayIconManager.MinUpdateIntervalSeconds, TrayIconManager.MaxUpdateIntervalSeconds);

        // Detect the device first so a failure doesn't leave a stale tray icon behind
        _device = DeviceFactory.Create();
        _trayIconManager = new TrayIconManager();
        _cpuMonitor = new CpuMonitor();

        _updateTimer = new System.Windows.Forms.Timer { Interval = _updateIntervalSeconds * 1000 };
        _updateTimer.Tick += UpdateTimer_Tick;
        _updateTimer.Start();

        _trayIconManager.TemperatureUnitToggled += TrayIconManager_TemperatureUnitToggled;
        _trayIconManager.DisplayModeToggled += TrayIconManager_DisplayModeToggled;
        _trayIconManager.ExitClicked += TrayIconManager_ExitClicked;
        _trayIconManager.AutoStartToggled += TrayIconManager_AutoStartToggled;
        _trayIconManager.UpdateIntervalChanged += TrayIconManager_UpdateIntervalChanged;
        _trayIconManager.CoreAverageToggled += (_, useCoreAverage) => _useCoreAverage = useCoreAverage;

        InitializeAutoStartState();
    }

    private void UpdateTimer_Tick(object? sender, EventArgs e)
    {
        // Celsius-only devices always get Celsius, regardless of the saved preference
        var useFahrenheit = _isFahrenheit && _device.SupportsFahrenheit;
        var status = _cpuMonitor.Read(useFahrenheit, _useCoreAverage);
        _device.SetDisplayMode(_showCpuUsage || !_isPawnIoInstalled ? DisplayMode.CpuUsage : DisplayMode.CpuTemperature);
        _device.UpdateDisplay(status, useFahrenheit);
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

    private void TrayIconManager_UpdateIntervalChanged(object? sender, int seconds)
    {
        _updateIntervalSeconds = seconds;
        _updateTimer.Interval = seconds * 1000;
        _trayIconManager.SetUpdateInterval(seconds);
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
        _trayIconManager.SetUpdateInterval(_updateIntervalSeconds);
        _trayIconManager.SetCoreAverageState(_useCoreAverage);
        _trayIconManager.SetCoreAverageVisible(_cpuMonitor.SupportsCoreAverage && _isPawnIoInstalled);

        _trayIconManager.SetDisplayModeText(_showCpuUsage ? CpuTemperatureOptionText : CpuUsageOptionText);
        _trayIconManager.SetDisplayModeVisible(_device.SupportsDisplayModes && _isPawnIoInstalled);
        _trayIconManager.SetTemperatureUnitVisible(_device.SupportsFahrenheit && _isPawnIoInstalled);
        _trayIconManager.SetPawnIoMissingVisible(!_isPawnIoInstalled);
        _trayIconManager.SetTooltipText($"DeepCool Display ({_device.Name})");
    }

    private void SaveSettings()
    {
        Properties.Settings.Default.ShowCpuUsage = _showCpuUsage;
        Properties.Settings.Default.UseFahrenheit = _isFahrenheit;
        Properties.Settings.Default.UpdateIntervalSeconds = _updateIntervalSeconds;
        Properties.Settings.Default.UseCoreAverage = _useCoreAverage;

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
