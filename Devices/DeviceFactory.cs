using HidSharp;

namespace DeepCool_Display.Devices;

public static class DeviceFactory
{
    public const int VendorId = 13875;

    // AK400 DIGITAL, AK620 DIGITAL, AK500 DIGITAL, AK500S DIGITAL
    private static readonly int[] AkSeriesProductIds = [1, 2, 3, 4];

    // AK620 G2 DIGITAL NYX, AK700 DIGITAL NYX, AK400 G2 DIGITAL NYX, AK500 G2 DIGITAL NYX
    private static readonly int[] AkG2SeriesProductIds = [41, 42, 43, 44];

    public static IDisplayDevice Create()
    {
        var devices = DeviceList.Local.GetHidDevices(VendorId).ToList();

        var g2Device = devices.FirstOrDefault(d => AkG2SeriesProductIds.Contains(d.ProductID));
        if (g2Device != null)
        {
            return new AkG2SeriesDevice(g2Device);
        }

        var akDevice = devices.FirstOrDefault(d => AkSeriesProductIds.Contains(d.ProductID));
        if (akDevice != null)
        {
            return new AkSeriesDevice(akDevice);
        }

        var found = devices.Count == 0
            ? "none"
            : string.Join(", ", devices.Select(d => $"PID {d.ProductID}").Distinct());
        throw new Exception($"No supported DeepCool device found (DeepCool devices detected: {found})");
    }
}
