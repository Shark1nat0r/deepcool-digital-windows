using System.Buffers.Binary;
using DeepCool_Display.Monitor;
using HidSharp;

namespace DeepCool_Display.Devices;

/// <summary>
/// AK G2 series (AK400/AK500/AK620 G2 DIGITAL NYX, AK700 DIGITAL NYX).
/// The device receives power, temperature, usage and frequency in a single packet
/// and cycles through them on its own. Temperature alarm thresholds are hard-coded in the device.
/// Protocol taken from https://github.com/Nortank12/deepcool-digital-linux (lq_series.rs).
/// </summary>
public class AkG2SeriesDevice : IDisplayDevice
{
    private const byte ReportId = 16;
    private const byte TerminationByte = 22;
    private static readonly byte[] Header = [104, 1, 8, 12, 1, 2];

    private HidStream? _stream;
    private readonly byte[] _displayData = new byte[64];

    public string Name => "AK G2 Series";
    public bool SupportsDisplayModes => false;

    public AkG2SeriesDevice(HidDevice device)
    {
        if (!device.TryOpen(out _stream))
        {
            throw new Exception("Failed to open AK G2 Series device");
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
