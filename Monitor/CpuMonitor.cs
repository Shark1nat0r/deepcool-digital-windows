using LibreHardwareMonitor.Hardware;

namespace DeepCool_Display.Monitor;

public readonly record struct CpuStatus(float Temperature, float Usage, float Power, float Frequency);

public class CpuMonitor : IDisposable
{
    // Preferred sensor names, in priority order (AMD first, then Intel)
    private static readonly string[] TemperatureSensorNames = ["Core (Tctl/Tdie)", "CPU Package", "Core (Tctl)", "Core (Tdie)"];
    private static readonly string[] PowerSensorNames = ["Package", "CPU Package"];
    private const string TotalLoadSensorName = "CPU Total";

    private readonly Computer _computer;
    private IHardware? _cpuHardware;
    private ISensor? _tempSensor;
    private ISensor? _usageSensor;
    private ISensor? _powerSensor;
    private ISensor[] _coreClockSensors = [];

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
        _usageSensor = FindSensor(SensorType.Load, [TotalLoadSensorName]);
        _powerSensor = FindSensor(SensorType.Power, PowerSensorNames);

        _coreClockSensors = _cpuHardware.Sensors
            .Where(s => s.SensorType == SensorType.Clock && s.Name.StartsWith("Core #"))
            .ToArray();
    }

    private ISensor? FindSensor(SensorType type, string[] preferredNames)
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

        return sensors.FirstOrDefault();
    }

    public CpuStatus Read(bool inFahrenheit = false)
    {
        _cpuHardware?.Update();

        var temperature = _tempSensor?.Value ?? 0;
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
