using System.Buffers.Binary;
using DeepCool_Display.Monitor;
using HidSharp;

namespace DeepCool_Display.Devices;

/// <summary>
/// LQ series liquid coolers (LQ240, LQ360).
/// The device receives power, temperature, usage and frequency in a single packet
/// and cycles through them on its own. Temperature alarm thresholds are hard-coded in the device.
/// Uses the same packet layout as the AK G2 series, see the mapping table at
/// https://github.com/Nortank12/deepcool-digital-linux/blob/main/device-list/tables/lq-series.md
/// </summary>
public class LqSeriesDevice : IDisplayDevice
{
    private const byte ReportId = 16;
    private const byte TerminationByte = 22;
    private static readonly byte[] Header = [104, 1, 8, 12, 1, 2];

    private HidStream? _stream;
    private readonly byte[] _displayData = new byte[64];

    public string Name => "LQ Series";
    public bool SupportsFahrenheit => true;
    public bool SupportsDisplayModes => false;

    public LqSeriesDevice(HidDevice device)
    {
        if (!device.TryOpen(out _stream))
        {
            throw new Exception("Failed to open LQ Series device");
        }

        _displayData[0] = ReportId;
        Header.CopyTo(_displayData, 1);
    }

    public void SetDisplayMode(DisplayMode mode)
    {
        // The device shows all metrics by itself
    }

    public void UpdateDisplay(CpuStatus status, bool isFahrenheit)
    {
        if (_stream == null)
        {
            throw new InvalidOperationException("Device stream is not initialized");
        }

        var data = _displayData.AsSpan();

        BinaryPrimitives.WriteUInt16BigEndian(data[7..], ToUInt16(status.Power));

        data[9] = (byte)(isFahrenheit ? 1 : 0);
        BinaryPrimitives.WriteSingleBigEndian(data[10..], status.Temperature);

        data[14] = (byte)Math.Clamp(Math.Round(status.Usage), 0, 100);

        BinaryPrimitives.WriteUInt16BigEndian(data[15..], ToUInt16(status.Frequency));

        var checksum = 0;
        for (var i = 1; i <= 16; i++)
        {
            checksum += data[i];
        }
        data[17] = (byte)(checksum % 256);
        data[18] = TerminationByte;

        _stream.Write(_displayData);
    }

    private static ushort ToUInt16(float value)
    {
        return (ushort)Math.Clamp(Math.Round(value), 0, ushort.MaxValue);
    }

    public void Dispose()
    {
        _stream?.Dispose();
        _stream = null;
        GC.SuppressFinalize(this);
    }
}
