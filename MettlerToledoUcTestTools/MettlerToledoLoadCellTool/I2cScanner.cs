using System;
using System.Collections.Generic;

namespace MettlerToledoLoadCellTool
{
    /// <summary>
    /// Scans all JIDA I2C buses for responding devices. Uses only plain I2C reads
    /// (no register reads, which would first write a register index): nothing is written.
    /// </summary>
    public static class I2cScanner
    {
        /// <summary>
        /// Addresses that must never be written to by the unlock sequence:
        /// 0xA0-0xAF are EEPROMs (RAM SPD / board config, corrupting them can stop the board
        /// from booting) and 0xD2 is the usual clock generator on the SMBus.
        /// </summary>
        public static bool IsProtectedAddress(byte address)
        {
            return (address >= 0xA0 && address <= 0xAF) || address == 0xD2;
        }

        public static List<string> Scan(IntPtr handle)
        {
            List<string> lines = new List<string>();
            int busCount = JiddaWrapper.JidaI2CCount(handle);
            lines.Add(string.Format("I2C scan: {0} bus(es) found", busCount));

            for (int bus = 0; bus < busCount; bus++)
            {
                int type = JiddaWrapper.JidaI2CType(handle, bus);
                bool available = JiddaWrapper.JidaI2CIsAvailable(handle, bus);
                lines.Add(string.Format("Bus {0}: type {1} ({2}){3}",
                    bus, type, TypeName(type), available ? "" : " - not available"));
                if (!available)
                    continue;

                int found = 0;
                for (int address = 0x02; address < 0xF0; address += 2)
                {
                    if (!Probe(handle, bus, (byte)address))
                        continue;

                    found++;
                    lines.Add(string.Format("    Device 0x{0:x2}{1}", address,
                        IsProtectedAddress((byte)address) ? "  (protected: EEPROM/clock, never used for unlock)" : "  <- candidate"));
                }

                if (found == 0)
                    lines.Add("    No devices");
            }

            lines.Add("I2C scan done");
            return lines;
        }

        private static unsafe bool Probe(IntPtr handle, int bus, byte address)
        {
            byte value = 0;
            // Bit 0 set = read
            return JiddaWrapper.JidaI2CRead(handle, bus, (byte)(address | 0x01), &value, 1);
        }

        private static string TypeName(int type)
        {
            switch (type)
            {
                case 1: return "PRIMARY";
                case 2: return "SMB";
                case 3: return "JILI";
                default: return "UNKNOWN";
            }
        }
    }
}
