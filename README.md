# Deepcool-Digital-Windows

**Deepcool-Digital-Windows** is a lightweight alternative to the official software from Deepcool, designed to display CPU usage and temperature on Deepcool coolers. It currently supports **non-pro AK series devices only**. This project is intended for users who prefer a streamlined solution for monitoring their CPU metrics without the overhead of the official software.

---

## Features

- Displays **CPU usage** and **CPU temperature** directly on supported Deepcool AK series coolers.
- Toggle between **Celsius** and **Fahrenheit** temperature units.
- Switch between displaying **CPU usage** or **CPU temperature**.
- Lightweight and efficient implementation using the **.NET 9 Runtime**.

---

## Supported Devices

- **Non-Pro AK Series Coolers**
    - This app does not currently support Pro models or other Deepcool devices.

---

## Disclaimer

- This project and its author are **not associated with Deepcool in any way**.
- The app is provided **as-is**, and you use it at your own risk. Ensure you understand the implications of running software that requires administrator privileges.
- The app requires **administrator rights** to read CPU temperature data.

---

## Inspiration

This project was heavily inspired by Nortank12's work (https://github.com/Nortank12/deepcool-digital-linux) for the Linux implementation of Deepcool cooler support. Special thanks to Nortank12 for their contributions to the community!

---

## Requirements

1. **.NET 9 Runtime**
    - You can find two versions of the app on the [Releases page](https://github.com/your-repository/releases):
        - A version that includes the runtime (self-contained).
        - A version that does not include the runtime (framework-dependent).
    - If using the framework-dependent version, ensure the .NET 9 Runtime is installed on your system. Download it from [Microsoft's official site](https://dotnet.microsoft.com/).
2. **Administrator Rights**
    - The app requires elevated privileges to access CPU temperature data.

---

## Installation

1. Download the latest release from the [Releases page](https://github.com/your-repository/releases).
2. Choose either:
    - The **self-contained version**, which includes all necessary runtime files.
    - The **framework-dependent version**, which requires .NET 9 Runtime to be installed separately.
3. Extract the downloaded files to a folder of your choice.
4. Install the [PawnIO Driver](https://pawnio.eu/) for hardware access to display the current CPU Temperature.
5. Run `Deepcool-Digital-Windows.exe` with administrator privileges.

---

## Usage

1. Launch the application by double-clicking `Deepcool-Digital-Windows.exe`.
2. Use the tray icon to toggle between Celsius/Fahrenheit or CPU usage/temperature display modes.
3. Monitor your CPU metrics directly on your supported Deepcool AK series cooler.

---

## Development

### Built With:

- **LibreHardwareMonitor**: For reading CPU metrics (licensed under MPL-2.0).
- **HidSharp**: For USB HID communication with Deepcool devices (licensed under Apache 2.0).


### License:

This project is licensed under the [MIT License](LICENSE). See the `LICENSE` file for details.

---

## Contributions

Contributions are welcome! Please feel free to open issues or submit pull requests if you have ideas for improvements or additional features.

---

## Support

If you encounter any issues, please open an issue on this repository or reach out through GitHub discussions.


