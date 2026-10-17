using DeepCool_Display.Monitor;
using HidSharp;

namespace DeepCool_Display.Devices;

public class AkSeriesDevice : IDisplayDevice
{
    private const byte ReportId = 16;
    private const byte InitCommand = 170;
    private const byte TemperatureModeCelsius = 19;
    private const byte TemperatureModeFahrenheit = 35;
    private const byte UsageMode = 76;
    private const int TempLimitCelsius = 90;
    private const int TempLimitFahrenheit = 194;

    private HidDevice? _device;
    private HidStream? _stream;
    private readonly byte[] _displayData = new byte[64];
    private DisplayMode _currentMode;

    public string Name => "AK Series";
    public bool SupportsDisplayModes => true;

    public AkSeriesDevice(HidDevice device)
    {
        _device = device;
        InitializeDevice();
        _displayData[0] = ReportId;
        _currentMode = DisplayMode.CpuTemperature;
    }

    private void InitializeDevice()
    {
        if (!_device!.TryOpen(out _stream))
        {
            throw new Exception("Failed to open AK Series device");
        }

        var initData = new byte[64];
        initData[0] = ReportId;
        initData[1] = InitCommand;
        _stream.Write(initData);
    }

    public void SetDisplayMode(DisplayMode mode)
    {
        _currentMode = mode;
    }

    public void UpdateDisplay(CpuStatus status, bool isFahrenheit)
    {
        UpdateDisplay(cpuTemperature: status.Temperature, cpuUsage: status.Usage, isFahrenheit: isFahrenheit);
    }

    public void UpdateDisplay(float cpuTemperature = 0, float cpuUsage = 0, bool isFahrenheit = false, bool alarmEnabled = true)
    {
        if (_stream == null)
        {
            throw new InvalidOperationException("Device stream is not initialized");
        }

        switch (_currentMode)
        {
            case DisplayMode.CpuTemperature:
                UpdateTemperatureDisplay(cpuTemperature, isFahrenheit, alarmEnabled);
                break;

            case DisplayMode.CpuUsage:
                UpdateUsageDisplay(cpuUsage);
                break;

            default:
                throw new InvalidOperationException("Unsupported display mode");
        }

        _stream.Write(_displayData);
    }

    private void UpdateTemperatureDisplay(float temperature, bool isFahrenheit, bool alarmEnabled)
    {
        _displayData[1] = (byte)(isFahrenheit ? TemperatureModeFahrenheit : TemperatureModeCelsius);

        var tempValue = (int)Math.Round(temperature);
        _displayData[3] = (byte)(tempValue / 100);         // Hundreds digit
        _displayData[4] = (byte)((tempValue / 10) % 10);   // Tens digit
        _displayData[5] = (byte)(tempValue % 10);          // Ones digit

        _displayData[2] = (byte)Math.Min(10, Math.Round(tempValue / (isFahrenheit ? 19.4 : 9.0)));

        var tempLimit = isFahrenheit ? TempLimitFahrenheit : TempLimitCelsius;
        _displayData[6] = (byte)(alarmEnabled && tempValue >= tempLimit ? 1 : 0);
    }

    private void UpdateUsageDisplay(float usage)
    {
        _displayData[1] = UsageMode;

        var usageValue = (int)Math.Round(usage);
        _displayData[3] = (byte)(usageValue / 100);         // Hundreds digit
        _displayData[4] = (byte)((usageValue / 10) % 10);   // Tens digit
        _displayData[5] = (byte)(usageValue % 10);          // Ones digit

        _displayData[2] = (byte)(usage < 15 ? 1 : Math.Round(usage / 10.0));

        // Alarm flag is not relevant for CPU usage and can be set to zero
        _displayData[6] = 0;
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _device = null;
        _stream = null;
        GC.SuppressFinalize(this);
    }
}