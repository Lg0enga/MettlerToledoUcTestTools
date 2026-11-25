using System.Collections.Generic;

namespace UcScaleTool.Hardware
{
    /// <summary>
    /// Scans the I2C buses for responding devices. Uses only plain reads
    /// (<see cref="IBoardBackend.Probe"/>), nothing is written to any device.
    /// </summary>
    public static class I2cScanner
    {
        /// <summary>UspEpc can't report the bus count, so this many buses are tried.</summary>
        private const int UspEpcBusesToScan = 4;

        /// <summary>
        /// Addresses the unlock sequence must never write to: 0xA0-0xAF are EEPROMs (RAM SPD /
        /// board configuration, corrupting them can stop the board from booting) and 0xD2 is
        /// the usual clock generator on the SMBus.
        /// </summary>
        public static bool IsProtectedAddress(byte address)
        {
            return (address >= 0xA0 && address <= 0xAF) || address == 0xD2;
        }

        public static List<string> Scan(IBoardBackend board)
        {
            List<string> lines = new List<string>();
            int busCount = board.BusCount;
            if (busCount < 0)
            {
                busCount = UspEpcBusesToScan;
                lines.Add(string.Format("I2C scan ({0}), bus 0-{1}", board.BoardDescription, busCount - 1));
            }
            else
            {
                lines.Add(string.Format("I2C scan ({0}), {1} bus(sen)", board.BoardDescription, busCount));
            }

            for (int bus = 0; bus < busCount; bus++)
            {
                lines.Add(string.Format("Bus {0}: {1}", bus, board.DescribeBus(bus)));

                int found = 0;
                for (int address = 0x02; address < 0xF0; address += 2)
                {
                    if (!board.Probe(bus, (byte)address))
                        continue;

                    found++;
                    lines.Add(string.Format("    Device 0x{0:X2}{1}", address,
                        IsProtectedAddress((byte)address)
                            ? "  (beschermd: EEPROM/klok, nooit gebruikt voor unlock)"
                            : "  <- kandidaat"));
                }

                if (found == 0)
                    lines.Add("    Geen devices");
            }

            lines.Add("I2C scan klaar");
            return lines;
        }
    }
}
