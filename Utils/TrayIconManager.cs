namespace DeepCool_Display.Utils;

public class TrayIconManager : IDisposable
{
    private readonly NotifyIcon _trayIcon;
    private const string TooltipText = "DeepCool Display";

    private readonly ToolStripMenuItem _autoStartMenuItem;
    private readonly ToolStripMenuItem _temperatureUnitMenuItem;
    private readonly ToolStripMenuItem _toggleDisplayModeMenuItem;

    public event EventHandler? ExitClicked;
    public event EventHandler<bool>? AutoStartToggled;
    public event EventHandler<bool>? TemperatureUnitToggled;
    public event EventHandler? DisplayModeToggled; // New event for toggling display mode

    public TrayIconManager()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
            Text = TooltipText,
            Visible = true
        };

        var contextMenu = new ContextMenuStrip();

        _toggleDisplayModeMenuItem = new ToolStripMenuItem("Show CPU Usage");
        _toggleDisplayModeMenuItem.Click += ToggleDisplayModeMenuItem_Click;
        contextMenu.Items.Add(_toggleDisplayModeMenuItem);

        _temperatureUnitMenuItem = new ToolStripMenuItem("Use Fahrenheit")
        {
            CheckOnClick = true
        };
        _temperatureUnitMenuItem.CheckedChanged += TemperatureUnitMenuItem_CheckedChanged;
        contextMenu.Items.Add(_temperatureUnitMenuItem);

        _autoStartMenuItem = new ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true
        };
        _autoStartMenuItem.Click += AutoStartMenuItem_Click;
        contextMenu.Items.Add(_autoStartMenuItem);

        contextMenu.Items.Add("Exit", null, (_, _) => ExitClicked?.Invoke(this, EventArgs.Empty));

        _trayIcon.ContextMenuStrip = contextMenu;
    }

    private void AutoStartMenuItem_Click(object? sender, EventArgs e)
    {
        AutoStartToggled?.Invoke(this, _autoStartMenuItem.Checked);
    }

    private void TemperatureUnitMenuItem_CheckedChanged(object? sender, EventArgs e)
    {
        var isFahrenheit = _temperatureUnitMenuItem.Checked;
        TemperatureUnitToggled?.Invoke(this, isFahrenheit);
    }

    private void ToggleDisplayModeMenuItem_Click(object? sender, EventArgs e)
    {
        DisplayModeToggled?.Invoke(this, EventArgs.Empty);
    }

    public void SetAutoStartMenuItemState(bool isChecked)
    {
        _autoStartMenuItem.Checked = isChecked;
    }

    public void SetTemperatureUnitState(bool isFahrenheit)
    {
        _temperatureUnitMenuItem.Checked = isFahrenheit;
    }

    public void SetDisplayModeText(string text)
    {
        _toggleDisplayModeMenuItem.Text = text;
    }

    public void SetDisplayModeVisible(bool visible)
    {
        _toggleDisplayModeMenuItem.Visible = visible;
    }

    public void SetTooltipText(string text)
    {
        _trayIcon.Text = text;
    }

    public void Dispose()
    {
        _trayIcon.Dispose();
        GC.SuppressFinalize(this);
    }
}
