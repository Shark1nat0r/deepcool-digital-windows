using LibreHardwareMonitor.Hardware;

namespace DeepCool_Display.Monitor;

public readonly record struct CpuStatus(float Temperature, float Usage, float Power, float Frequency);

public class CpuMonitor : IDisposable
{
    // Preferred sensor names, in priority order (AMD first, then Intel)
    private static readonly string[] TemperatureSensorNames = ["Core (Tctl/Tdie)", "CPU Package", "Core (Tctl)", "Core (Tdie)"];
    // Averaged sensors computed by LibreHardwareMonitor. Intel reports per-core temperatures and averages them;
    // AMD Zen only reports per-CCD temperatures: multi-CCD CPUs get the CCD average, single-CCD CPUs
    // only have CCD1, which is a single die temperature rather than an average.
    private static readonly string[] AverageTemperatureSensorNames = ["Core Average", "CCDs Average (Tdie)", "CCD1 (Tdie)"];
    private static readonly string[] PowerSensorNames = ["Package", "CPU Package"];
    private const string TotalLoadSensorName = "CPU Total";

    private readonly Computer _computer;
    private IHardware? _cpuHardware;
    private ISensor? _tempSensor;
    private ISensor? _averageTempSensor;
    private ISensor? _usageSensor;
    private ISensor? _powerSensor;
    private ISensor[] _coreClockSensors = [];

    public bool SupportsCoreAverage => _averageTempSensor != null;

    public CpuMonitor()
    {
        _computer = new Computer { IsCpuEnabled = true };
        _computer.Open();
        InitializeSensors();
    }

    private void InitializeSensors()
    {
        _cpuHardware = _computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);
        if (_cpuHardware == null) return;

        _cpuHardware.Update();

        foreach (var sensor in _cpuHardware.Sensors)
        {
            sensor.ValuesTimeWindow = TimeSpan.Zero; // Disable historical data storage
        }

        _tempSensor = FindSensor(SensorType.Temperature, TemperatureSensorNames);
        _averageTempSensor = FindSensor(SensorType.Temperature, AverageTemperatureSensorNames, fallbackToFirst: false);
        _usageSensor = FindSensor(SensorType.Load, [TotalLoadSensorName]);
        _powerSensor = FindSensor(SensorType.Power, PowerSensorNames);

        _coreClockSensors = _cpuHardware.Sensors
            .Where(s => s.SensorType == SensorType.Clock && (s.Name.StartsWith("Core #") || s.Name.StartsWith("CPU Core #")))
            .ToArray();
    }

    private ISensor? FindSensor(SensorType type, string[] preferredNames, bool fallbackToFirst = true)
    {
        var sensors = _cpuHardware!.Sensors
            .Where(s => s.SensorType == type)
            .OrderBy(s => s.Index)
            .ToList();

        foreach (var name in preferredNames)
        {
            var match = sensors.FirstOrDefault(s => s.Name == name);
            if (match != null) return match;
        }

        return fallbackToFirst ? sensors.FirstOrDefault() : null;
    }

    public CpuStatus Read(bool inFahrenheit = false, bool useCoreAverage = false)
    {
        _cpuHardware?.Update();

        var temperature = (useCoreAverage ? _averageTempSensor?.Value : null) ?? _tempSensor?.Value ?? 0;
        if (inFahrenheit)
        {
            temperature = ConvertCelsiusToFahrenheit(temperature);
        }

        // Highest core clock, same as the Linux implementation
        var frequency = _coreClockSensors.Length > 0
            ? _coreClockSensors.Max(s => s.Value ?? 0)
            : 0;

        return new CpuStatus(
            Temperature: temperature,
            Usage: _usageSensor?.Value ?? 0,
            Power: _powerSensor?.Value ?? 0,
            Frequency: frequency);
    }

    private static float ConvertCelsiusToFahrenheit(float temperature)
    {
        return (temperature * 9 / 5) + 32;
    }

    public void Dispose()
    {
        _computer.Close();
        GC.SuppressFinalize(this);
    }
}
