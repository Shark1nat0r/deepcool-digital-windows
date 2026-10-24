using DeepCool_Display.Monitor;
using HidSharp;

namespace DeepCool_Display.Devices;

public class AgSeriesDevice : IDisplayDevice
{
    private const byte ReportId = 16;
    private const byte TemperatureModeCelsius = 19;
    private const byte UsageMode = 76;
    private const int TempLimitCelsius = 90;

    private HidDevice? _device;
    private HidStream? _stream;
    private readonly byte[] _displayData = new byte[64];
    private DisplayMode _currentMode;

    public string Name => "AG Series";
    public bool SupportsDisplayModes => true;

    public AgSeriesDevice(HidDevice device)
    {
        _device = device;
        if (!_device.TryOpen(out _stream))
        {
            throw new Exception("Failed to open AG Series device");
        }

        _displayData[0] = ReportId;
        _currentMode = DisplayMode.CpuTemperature;
    }

    public void SetDisplayMode(DisplayMode mode)
    {
        _currentMode = mode;
    }

    // The AG display has two digits and only supports Celsius, so isFahrenheit is ignored
    public void UpdateDisplay(CpuStatus status, bool isFahrenheit)
    {
        if (_stream == null)
        {
            throw new InvalidOperationException("Device stream is not initialized");
        }

        var tempValue = (int)Math.Round(status.Temperature);
        var value = _currentMode switch
        {
            DisplayMode.CpuTemperature => tempValue,
            DisplayMode.CpuUsage => (int)Math.Round(status.Usage),
            _ => throw new InvalidOperationException("Unsupported display mode")
        };
        value = Math.Clamp(value, 0, 99);

        _displayData[1] = _currentMode == DisplayMode.CpuUsage ? UsageMode : TemperatureModeCelsius;
        _displayData[3] = (byte)(value / 10);   // Tens digit
        _displayData[4] = (byte)(value % 10);   // Ones digit
        _displayData[5] = (byte)(tempValue >= TempLimitCelsius ? 1 : 0);

        _stream.Write(_displayData);
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _device = null;
        _stream = null;
        GC.SuppressFinalize(this);
    }
}
