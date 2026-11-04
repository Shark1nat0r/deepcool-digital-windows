using System.Buffers.Binary;
using DeepCool_Display.Monitor;
using HidSharp;

namespace DeepCool_Display.Devices;

public class AkProSeriesDevice : IDisplayDevice
{
    private const byte ReportId = 16;
    private const byte TerminationByte = 22;
    private static readonly byte[] Header = [104, 1, 4, 13, 1, 2, 8];

    private HidStream? _stream;
    private readonly byte[] _displayData = new byte[64];

    public string Name => "AK DIGITAL PRO";
    public bool SupportsDisplayModes => false;

    public AkProSeriesDevice(HidDevice device)
    {
        if (!device.TryOpen(out _stream))
        {
            throw new Exception("Failed to open AK DIGITAL PRO device");
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

        BinaryPrimitives.WriteUInt16BigEndian(data[8..], ToUInt16(status.Power));

        data[10] = (byte)(isFahrenheit ? 1 : 0);
        BinaryPrimitives.WriteSingleBigEndian(data[11..], status.Temperature);

        data[15] = (byte)Math.Clamp(Math.Round(status.Usage), 0, 100);

        BinaryPrimitives.WriteUInt16BigEndian(data[16..], ToUInt16(status.Frequency));

        var checksum = 0;
        for (var i = 1; i <= 17; i++)
        {
            checksum += data[i];
        }
        data[18] = (byte)(checksum % 256);
        data[19] = TerminationByte;

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
