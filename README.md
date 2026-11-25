# Mettler Toledo UC retail scale tooling

A tool to read the weight from the load cell and print labels on Mettler Toledo UC retail scales.

## UC Scale Tool (v2)

`MettlerToledoUcTestTools/UcScaleTool` is the rebuilt tool. It works on both board types found in UC scales:

| Board support library | Used on | Load cell I2C |
|---|---|---|
| `jida.dll` (Kontron JIDA) | UC-HTT-M (Win 7), UC-SPCT-M (XP), UC-CTTM (Win 7) | bus 0, address `C0` |
| `UspEpc.dll` (MSC UspEpc) | UC-GTT-S (XP) | bus 1, address `C0` |

Features:
- **Hardware detection**: checks whether JIDA or UspEpc works and shows the JIDA board name(s) (e.g. `UUP6`, `MNP1`).
  With *Board library = Automatisch* the tool picks the working one by itself.
- **Load cell**: unlocks the load cell over I2C (repeated every 30 seconds), shows net/tare/gross weight, zero and tare.
- **Label printing**: prints a label with product, weight, price per kg, total price and an in-store EAN-13 barcode.
- **Printer settings**: read and change print speed, darkness, label/receipt mode, offset, the takeaway sensor and the rewind/takeup motors, and calibrate the printer.
- **I2C scan** (read-only) to find the load cell on unknown scales. Addresses `A0`-`AF` (EEPROMs) and `D2` (clock generator) are never written to.
- Settings are stored in `UcScaleTool.ini` next to the exe, so a working configuration stays with the tool.

The tool targets .NET Framework 2.0 (some XP scales have no .NET 3.5) and is built as 32-bit (x86), because `jida.dll` and `UspEpc.dll` are 32-bit.
It asks for administrator rights on start (UAC on Windows 7).

### Install on a scale
1. .NET Framework 2.0 or newer is needed. Windows 7 has it built in; the tested UC-GTT-S (XP) only had .NET 2.0.
2. Copy the contents of `UcScaleTool/bin/Release` to the scale (`UcScaleTool.exe`, `Jida.dll`, `UspEpc.dll`, `BarcodeLib.dll`, `LibUsbDotNet.dll`).
3. Start `UcScaleTool.exe`, open *Hardware* > *Detecteer hardware*, then *Weegschaal openen*.

On UspEpc scales the `UspEpc.sys` driver must already be installed (it is on the factory image).

### Printer protocol
Two printer command sets are available under *Hardware* > *Printerprotocol*:
- *Standaard (XP-versie)*: the command set of the working XP version, with printer status check (remove label / end of roll).
- *Oud (eerste versie)*: the command set of the first version of this tool.

### Printer settings
The *Printer* page reads the settings from the printer when it is opened (*Uitlezen* reads them again). *Toepassen* only sends the settings that changed and reads them back afterwards.

| Setting | Values | Command |
|---|---|---|
| Printsnelheid | 125, 100, 90, 80, 70, 60 mm/s | `ESC G n` |
| Donkerte | 0-8 | `ESC H n` |
| Label of bon | label, receipt, continuous | `ESC M n` (label also `ESC C9 1`) |
| Offset | 0-12 (sent as step x 8) | `ESC O nnn` |
| Wegneemsensor | on/off: wait until the label is taken | `ESC A n` |
| Motoren: Rewind | on/off | `ESC x 2 00000` (on) / `ESC x 3 00000` (off) |
| Motoren: Takeup | on/off | `ESC x 5 00000` (on) / `ESC x 6 00000` (off) |

Settings are sent between `ESC l 1` and `ESC l 0` and read with `ESC 9F`, the same commands as the Goenga Scale app.
*Printer kalibreren* measures the labels after loading a new roll or another label size (the printer feeds a few labels).

### Weight decimals
Some scales send the weight without a decimal point. *Decimalen zonder punt* sets how many decimals are applied in that case (default 2). The raw scale answer is shown under the weight display.

## Printer driver (Zadig) - usually not needed
The tool talks to the printer with LibUsbDotNet. The factory Windows image of the UC scales already has a libusb driver for the printer, so printing works without installing anything.

> [!IMPORTANT]
> Don't replace the factory printer driver with Zadig's default **WinUSB** driver: the tool then can't find the printer anymore (*Printer niet gevonden (USB 0EB8:3000)*). If that happened, uninstall the Zadig driver in Device Manager (tick *Delete the driver software for this device*) and reconnect/restart.

Only use Zadig on a clean Windows install where the printer has no driver, and pick **libusb-win32** as the driver:

Download Zadig https://zadig.akeo.ie/

Run zadig and select **Options** -> **List All Devices**

![Zadig setup 1](https://github.com/Lg0enga/MettlerToledoUcTestTools/blob/main/assets/Zadig_Setup_01.png)

Select Device `Mettler UCP00001H432` (Label Printer) and choose the **libusb-win32** driver

![Zadig setup 2](https://github.com/Lg0enga/MettlerToledoUcTestTools/blob/main/assets/Zadig_Setup_02.png)

## Old version
`MettlerToledoUcTestTools/MettlerToledoLoadCellTool` is the first version (JIDA only). Its build is in `TestApp`.

![UC_test_tool](https://github.com/Lg0enga/MettlerToledoUcTestTools/blob/main/assets/UC_test_tool.png)

## Todo
- Scale setup
    - Monitor brightness

## Tested scales
Tested with the earlier versions (JIDA tool and XP/UspEpc tool); UC Scale Tool v2 still has to be verified on these scales.

- UC-HTT-M (Windows 7, JIDA)
- UC-SPCT-M (Windows XP, JIDA)
- UC-CTTM (Windows 7, JIDA)
- UC-GTT-S (Windows XP, UspEpc)
- UC-CDDT-M
- UC-SOCT-M

## Documentation

[Jida](https://github.com/Lg0enga/MettlerToledoUcTestTools/blob/main/docs/jida32.pdf)

[Scale commands](https://github.com/Lg0enga/MettlerToledoUcTestTools/blob/main/docs/mmt.pdf)

## Support

If you need help, create an issue. I would like to help you!
