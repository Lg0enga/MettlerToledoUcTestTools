using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using UcScaleTool.Hardware;
using UcScaleTool.Printing;

namespace UcScaleTool
{
    /// <summary>
    /// Settings, stored as key=value lines in UcScaleTool.ini next to the exe so they stay
    /// with the tool when it's copied to a scale.
    /// </summary>
    public class AppSettings
    {
        public BackendKind Backend = BackendKind.Auto;

        /// <summary>JIDA board name, e.g. UUP6 or MNP1. Empty = first board.</summary>
        public string JidaBoardName = "";

        public int JidaBus = 0;
        public int UspEpcBus = 1;
        public byte I2cAddress = LoadcellUnlocker.DefaultAddress;

        public string ComPort = "COM2";

        /// <summary>Decimals to apply when the scale sends a weight without decimal point.</summary>
        public int WeightDecimals = 2;

        public PrinterProtocol PrinterProtocol = PrinterProtocol.Standard;

        public string ProductName = "Testproduct";
        public string PricePerKg = "";
        public int Plu = 1;

        public static string FilePath
        {
            get { return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "UcScaleTool.ini"); }
        }

        public int BusFor(BackendKind kind)
        {
            return kind == BackendKind.UspEpc ? UspEpcBus : JidaBus;
        }

        public static AppSettings Load()
        {
            AppSettings s = new AppSettings();
            if (!File.Exists(FilePath))
                return s;

            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in File.ReadAllLines(FilePath))
            {
                int eq = line.IndexOf('=');
                if (eq > 0 && !line.TrimStart().StartsWith(";"))
                    values[line.Substring(0, eq).Trim()] = line.Substring(eq + 1).Trim();
            }

            s.Backend = GetEnum(values, "Backend", s.Backend);
            s.JidaBoardName = GetString(values, "JidaBoardName", s.JidaBoardName);
            s.JidaBus = GetInt(values, "JidaBus", s.JidaBus);
            s.UspEpcBus = GetInt(values, "UspEpcBus", s.UspEpcBus);
            s.I2cAddress = (byte)GetInt(values, "I2cAddress", s.I2cAddress);
            s.ComPort = GetString(values, "ComPort", s.ComPort);
            s.WeightDecimals = GetInt(values, "WeightDecimals", s.WeightDecimals);
            s.PrinterProtocol = GetEnum(values, "PrinterProtocol", s.PrinterProtocol);
            s.ProductName = GetString(values, "ProductName", s.ProductName);
            s.PricePerKg = GetString(values, "PricePerKg", s.PricePerKg);
            s.Plu = GetInt(values, "Plu", s.Plu);
            return s;
        }

        public void Save()
        {
            List<string> lines = new List<string>();
            lines.Add("; Mettler Toledo UC Scale Tool settings");
            lines.Add("Backend=" + Backend);
            lines.Add("JidaBoardName=" + JidaBoardName);
            lines.Add("JidaBus=" + JidaBus);
            lines.Add("UspEpcBus=" + UspEpcBus);
            lines.Add("I2cAddress=0x" + I2cAddress.ToString("X2"));
            lines.Add("ComPort=" + ComPort);
            lines.Add("WeightDecimals=" + WeightDecimals);
            lines.Add("PrinterProtocol=" + PrinterProtocol);
            lines.Add("ProductName=" + ProductName);
            lines.Add("PricePerKg=" + PricePerKg);
            lines.Add("Plu=" + Plu);
            File.WriteAllLines(FilePath, lines.ToArray());
        }

        private static string GetString(Dictionary<string, string> values, string key, string fallback)
        {
            string value;
            return values.TryGetValue(key, out value) ? value : fallback;
        }

        private static int GetInt(Dictionary<string, string> values, string key, int fallback)
        {
            string value;
            if (!values.TryGetValue(key, out value))
                return fallback;

            int result;
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(value.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out result))
                    return result;
            }
            else if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result))
            {
                return result;
            }
            return fallback;
        }

        private static T GetEnum<T>(Dictionary<string, string> values, string key, T fallback)
        {
            string value;
            if (!values.TryGetValue(key, out value))
                return fallback;
            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch (ArgumentException)
            {
                return fallback;
            }
        }
    }
}
