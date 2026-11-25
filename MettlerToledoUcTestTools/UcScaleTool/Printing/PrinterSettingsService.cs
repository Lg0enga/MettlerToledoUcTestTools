using System;
using System.Collections.Generic;
using System.Text;

namespace UcScaleTool.Printing
{
    /// <summary>
    /// Settings stored in the UC label printer. Values are indexes as the printer uses them.
    /// </summary>
    public class PrinterConfig
    {
        /// <summary>0..5 = 125, 100, 90, 80, 70, 60 mm/s.</summary>
        public int Speed;

        /// <summary>Darkness (contrast), 0..8.</summary>
        public int Darkness;

        /// <summary>0 = label, 1 = receipt, 2 = continuous.</summary>
        public int Mode;

        /// <summary>Offset step 0..12, sent to the printer as step * 8.</summary>
        public int OffsetStep;

        /// <summary>Takeaway sensor: the printer waits until the label is taken.</summary>
        public bool TakeawaySensor;

        /// <summary>Rewind motor of the printer.</summary>
        public bool RewindMotor;

        /// <summary>Takeup motor of the printer.</summary>
        public bool TakeupMotor;

        /// <summary>Raw answer to the read command, for the log.</summary>
        public string RawHex = "";

        public PrinterConfig Clone()
        {
            return (PrinterConfig)MemberwiseClone();
        }
    }

    /// <summary>
    /// Reads and writes the printer settings and calibrates the label sensor.
    /// Same commands as the Goenga Scale app (PrinterSettingsService / EvoLabelPrinter).
    /// All methods block: call them from a background thread.
    /// </summary>
    public static class PrinterSettingsService
    {
        public static readonly string[] SpeedNames = { "125 mm/s", "100 mm/s", "90 mm/s", "80 mm/s", "70 mm/s", "60 mm/s" };
        public static readonly string[] ModeNames = { "Label", "Bon (receipt)", "Doorlopend (continuous)" };
        public const int MaxDarkness = 8;
        public const int MaxOffsetStep = 12;

        private const byte Esc = 0x1B;

        // Positions of the values in the answer to ESC 0x9F (ASCII digits)
        private const int TakeawaySensorIndex = 4;
        private const int SpeedIndex = 5;
        private const int DarknessIndex = 6;
        private const int OffsetIndex = 7; // 3 digits: 7, 8, 9
        private const int TakeupMotorIndex = 23;
        private const int RewindMotorIndex = 24;
        private const int ModeIndex = 34;

        public static PrinterConfig Read()
        {
            return LabelPrinter.WithPrinter(delegate(LibUsbDotNet.UsbEndpointWriter writer, LibUsbDotNet.UsbEndpointReader reader)
            {
                byte[] response = new byte[1024];
                LabelPrinter.Exchange(writer, reader, new byte[] { 0x01, Esc, 0x9F }, response);

                PrinterConfig config = new PrinterConfig();
                config.TakeawaySensor = Digit(response, TakeawaySensorIndex) == 1;
                config.Speed = Clamp(Digit(response, SpeedIndex), 0, SpeedNames.Length - 1);
                config.Darkness = Clamp(Digit(response, DarknessIndex), 0, MaxDarkness);
                int offset = Digit(response, OffsetIndex) * 100 + Digit(response, OffsetIndex + 1) * 10 + Digit(response, OffsetIndex + 2);
                config.OffsetStep = Clamp((int)Math.Round(offset / 8.0), 0, MaxOffsetStep);
                config.Mode = Clamp(Digit(response, ModeIndex), 0, ModeNames.Length - 1);
                config.RewindMotor = Digit(response, RewindMotorIndex) == 1;
                config.TakeupMotor = Digit(response, TakeupMotorIndex) == 1;
                config.RawHex = ToHex(response, 40);
                return config;
            });
        }

        /// <summary>Sends only the settings that differ from <paramref name="current"/>.</summary>
        public static List<string> Apply(PrinterConfig current, PrinterConfig wanted)
        {
            List<string> changes = new List<string>();
            List<byte[]> commands = new List<byte[]>();

            if (wanted.Speed != current.Speed)
            {
                commands.Add(new byte[] { 0x02, Esc, (byte)'G', DigitChar(wanted.Speed) });
                changes.Add("snelheid " + SpeedNames[wanted.Speed]);
            }
            if (wanted.Darkness != current.Darkness)
            {
                commands.Add(new byte[] { 0x02, Esc, (byte)'H', DigitChar(wanted.Darkness) });
                changes.Add("donkerte " + wanted.Darkness);
            }
            if (wanted.Mode != current.Mode)
            {
                commands.Add(new byte[] { 0x02, Esc, (byte)'M', DigitChar(wanted.Mode) });
                if (wanted.Mode == 0)
                    commands.Add(new byte[] { 0x02, Esc, 0xC9, (byte)'1' });
                changes.Add("modus " + ModeNames[wanted.Mode]);
            }
            if (wanted.OffsetStep != current.OffsetStep)
            {
                string digits = (wanted.OffsetStep * 8).ToString("000");
                commands.Add(new byte[] { 0x02, Esc, (byte)'O', (byte)digits[0], (byte)digits[1], (byte)digits[2] });
                changes.Add("offset " + wanted.OffsetStep);
            }
            if (wanted.TakeawaySensor != current.TakeawaySensor)
            {
                commands.Add(new byte[] { 0x02, Esc, (byte)'A', (byte)(wanted.TakeawaySensor ? '1' : '0') });
                changes.Add("wegneemsensor " + (wanted.TakeawaySensor ? "aan" : "uit"));
            }
            if (wanted.RewindMotor != current.RewindMotor)
            {
                // ESC x 2 = rewind motor on, ESC x 3 = off
                commands.Add(new byte[] { 0x02, Esc, (byte)'x', (byte)(wanted.RewindMotor ? '2' : '3'),
                    (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0' });
                changes.Add("rewind motor " + (wanted.RewindMotor ? "aan" : "uit"));
            }
            if (wanted.TakeupMotor != current.TakeupMotor)
            {
                // ESC x 5 = takeup motor on, ESC x 6 = off
                commands.Add(new byte[] { 0x02, Esc, (byte)'x', (byte)(wanted.TakeupMotor ? '5' : '6'),
                    (byte)'0', (byte)'0', (byte)'0', (byte)'0', (byte)'0' });
                changes.Add("takeup motor " + (wanted.TakeupMotor ? "aan" : "uit"));
            }

            if (commands.Count == 0)
                return changes;

            LabelPrinter.WithPrinter(delegate(LibUsbDotNet.UsbEndpointWriter writer, LibUsbDotNet.UsbEndpointReader reader)
            {
                byte[] response = new byte[1024];
                // Settings are only accepted between ESC l 1 and ESC l 0
                LabelPrinter.Exchange(writer, reader, new byte[] { 0x01, Esc, (byte)'l', (byte)'1' }, response);
                foreach (byte[] command in commands)
                    LabelPrinter.Exchange(writer, reader, command, response);
                LabelPrinter.Exchange(writer, reader, new byte[] { (byte)(0x02 + commands.Count), Esc, (byte)'l', (byte)'0' }, response);
                return true;
            });
            return changes;
        }

        /// <summary>
        /// Calibrates the label/gap sensor: the printer feeds a few labels to measure them.
        /// </summary>
        public static void Calibrate()
        {
            byte[][] commands =
            {
                new byte[] { 0x01, Esc, 0x41, 0x31 },
                new byte[] { 0x02, Esc, 0x9F },
                new byte[] { 0x03, Esc, 0x78, 0x33, 0x30, 0x30, 0x30, 0x30, 0x30 },
                new byte[] { 0x04, Esc, 0x78, 0x35, 0x30, 0x30, 0x30, 0x30, 0x30 },
                new byte[] { 0x05, Esc, 0x78, 0x35, 0x30, 0x30, 0x30, 0x30, 0x30 },
                new byte[] { 0x06, Esc, 0x5A },
                new byte[] { 0x07, Esc, 0x7B },
                new byte[] { 0x08, Esc, 0x5A },
                new byte[] { 0x09, Esc, 0x53 },
                new byte[] { 0x10, Esc, 0x5A },
                new byte[] { 0x11, Esc, 0x41, 0x31 },
            };

            LabelPrinter.WithPrinter(delegate(LibUsbDotNet.UsbEndpointWriter writer, LibUsbDotNet.UsbEndpointReader reader)
            {
                byte[] response = new byte[1024];
                foreach (byte[] command in commands)
                    LabelPrinter.Exchange(writer, reader, command, response);
                return true;
            });
        }

        private static int Digit(byte[] data, int index)
        {
            byte b = data[index];
            return (b >= (byte)'0' && b <= (byte)'9') ? b - (byte)'0' : 0;
        }

        private static byte DigitChar(int value)
        {
            return (byte)('0' + value);
        }

        private static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        private static string ToHex(byte[] data, int count)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < count && i < data.Length; i++)
                sb.AppendFormat("{0:X2} ", data[i]);
            return sb.ToString().TrimEnd();
        }
    }
}
