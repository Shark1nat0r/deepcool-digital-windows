# Deepcool-Digital-Windows

**Deepcool-Digital-Windows** is a lightweight alternative to the official software from Deepcool, designed to display CPU usage and temperature on Deepcool coolers. It currently supports **non-pro AK series**, **AK G2 series (DIGITAL NYX)**, **AK DIGITAL PRO (AK500/AK620)** and **AG series** devices. This project is intended for users who prefer a streamlined solution for monitoring their CPU metrics without the overhead of the official software.

> [!IMPORTANT]
> **CPU temperature, power consumption and frequency require the [PawnIO driver](https://pawnio.eu/).** Without it, the app still works and shows CPU usage, but the other values are shown as 0, and the tray menu shows "PawnIO driver missing".

---

## Features

- Shows live CPU metrics on supported Deepcool coolers. Which metrics depend on the cooler; see [Supported Devices](#supported-devices).
- Switch between displaying **CPU usage** or **CPU temperature** (on coolers with a single value display).
- Toggle between **Celsius** and **Fahrenheit** (on coolers that support it).
- Optionally show the **Core Average** temperature instead of CPU Package (see [Core Average](#core-average)).
- Adjustable update interval (1–10 seconds).
- Optional **Start with Windows**.
- All settings are remembered between runs.
- Lightweight and efficient implementation using the **.NET 9 Runtime**.

### Core Average

- **Intel:** the average of all core temperatures.
- **AMD Ryzen:** Ryzen doesn't report per-core temperatures, so the CCD (chiplet) temperature is used instead: the average across CCDs on multi-CCD CPUs (e.g. 5950X, 7950X), or the single CCD1 temperature on single-CCD CPUs (e.g. 5800X, 7800X3D), which is not an average.
- The option is hidden if the CPU reports neither (e.g. Ryzen 1000/2000 series and APUs such as the 5600G or 8700G) or PawnIO is not installed.

---

## Supported Devices

| Series | Models | What the display shows | Usage/temperature toggle | Fahrenheit |
|---|---|---|:---:|:---:|
| **AK DIGITAL** | AK400, AK500, AK500S, AK620 DIGITAL | CPU usage *or* temperature | ✅ | ✅ |
| **AK DIGITAL NYX (G2)** | AK400 G2, AK500 G2, AK620 G2, AK700 DIGITAL NYX | Power, temperature, usage and frequency, cycled by the cooler itself | — | ✅ |
| **AK DIGITAL PRO** | AK500, AK620 DIGITAL PRO | Power, temperature, usage and frequency, cycled by the cooler itself | — | ✅ |
| **AG DIGITAL** | AG300, AG400, AG500, AG620 DIGITAL | CPU usage *or* temperature (two digits, max. 99) | ✅ | ❌ Celsius only |

- The cooler is detected automatically by its USB product ID.
- Other Pro models (e.g. AK400 DIGITAL PRO) and other Deepcool devices are not supported yet.

---

## Disclaimer

- This project and its author are **not associated with Deepcool in any way**.
- The app is provided **as-is**, and you use it at your own risk. It runs with **administrator rights**; make sure you understand the implications of that.

---

## Inspiration

This project was heavily inspired by Nortank12's work (https://github.com/Nortank12/deepcool-digital-linux) for the Linux implementation of Deepcool cooler support. Special thanks to Nortank12 for their contributions to the community!

---

## Requirements

1. **.NET 9 Runtime**
    - You can find two versions of the app on the [Releases page](https://github.com/Shark1nat0r/deepcool-digital-windows/releases):
        - A version that includes the runtime (self-contained).
        - A version that does not include the runtime (framework-dependent).
    - If using the framework-dependent version, ensure the .NET 9 Runtime is installed on your system. Download it from [Microsoft's official site](https://dotnet.microsoft.com/).
2. **Administrator Rights**
    - Required to read CPU temperature, power and frequency through the PawnIO driver, and to register the "Start with Windows" task.
3. **[PawnIO Driver](https://pawnio.eu/)** (optional, for temperature, power and frequency)
    - Required to read CPU temperature, power consumption and frequency. Without it, CPU usage still works, but the other values are shown as 0 and the tray menu shows "PawnIO driver missing".

---

## Installation

1. Download the latest release from the [Releases page](https://github.com/Shark1nat0r/deepcool-digital-windows/releases).
2. Choose either:
    - The **self-contained version**, which includes all necessary runtime files.
    - The **framework-dependent version**, which requires .NET 9 Runtime to be installed separately.
3. Save the downloaded `.exe` to a folder of your choice.
4. **Install the [PawnIO Driver](https://pawnio.eu/)** if you want CPU temperature, power and frequency readings. CPU usage works without it.
5. Run `deepcool-display-self-contained.exe` (or `deepcool-display-framework-dependent.exe`) with administrator privileges.

### Verifying a Release

Releases are built by GitHub Actions from the tagged commit and signed with a build provenance attestation. To check that a downloaded file was built by this repository's workflow, run:

```
gh attestation verify deepcool-display-self-contained.exe --repo Shark1nat0r/deepcool-digital-windows
```

The `SHA256SUMS.txt` file attached to each release lists the checksums of the binaries.

---

## Usage

1. Right-click the downloaded `.exe` and choose **Run as administrator**.
2. Use the tray icon menu to:
    - switch between CPU usage and temperature (AK DIGITAL and AG coolers),
    - toggle Celsius/Fahrenheit and Core Average temperature,
    - set the update interval (1–10 seconds),
    - enable **Start with Windows**, which starts the app with administrator rights at logon without a prompt.
3. Watch your CPU metrics on your Deepcool cooler.

---

## Development

### Built With:

- **LibreHardwareMonitor**: For reading CPU metrics (licensed under MPL-2.0).
- **HidSharp**: For USB HID communication with Deepcool devices (licensed under Apache 2.0).
- **TaskScheduler**: For the "Start with Windows" scheduled task (licensed under MIT).


### License:

This project is licensed under the [MIT License](LICENSE). See the `LICENSE` file for details.

---

## Contributions

Contributions are welcome! Please feel free to open issues or submit pull requests if you have ideas for improvements or additional features.

---

## Support

If you encounter any issues, please open an issue on this repository or reach out through GitHub discussions.


