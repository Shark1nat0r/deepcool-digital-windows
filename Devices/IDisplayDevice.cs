using DeepCool_Display.Monitor;

namespace DeepCool_Display.Devices;

public enum DisplayMode
{
    CpuTemperature,
    CpuUsage
}

public interface IDisplayDevice : IDisposable
{
    string Name { get; }

    /// <summary>
    /// False for devices that show all metrics at once and have no switchable display mode.
    /// </summary>
    bool SupportsDisplayModes { get; }

    void SetDisplayMode(DisplayMode mode);

    void UpdateDisplay(CpuStatus status, bool isFahrenheit);
}
