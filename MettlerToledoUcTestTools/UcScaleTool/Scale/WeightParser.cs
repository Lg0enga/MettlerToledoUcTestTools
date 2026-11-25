using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace UcScaleTool.Scale
{
    public class WeightReading
    {
        public decimal Net;
        public decimal? Tare;
        public decimal? Gross;
        public string Unit = "kg";

        /// <summary>True when the scale reports a moving (dynamic) weight.</summary>
        public bool Dynamic;

        public string Raw;

        public decimal NetInKg
        {
            get
            {
                string unit = Unit.ToLowerInvariant();
                if (unit == "g")
                    return Net / 1000m;
                if (unit == "lb")
                    return Net * 0.45359237m;
                return Net;
            }
        }
    }

    /// <summary>
    /// Parses the answer to the SXI command (data record). Per the Mettler Toledo interface
    /// description the standard record has three blocks:
    ///   SX D A011   23.650 kg  A012   21.650 kg  A013    2.000 kg
    /// with A011 = gross, A012 = net, A013 = tare and "D" = dynamic. Some UC scales send the
    /// value without decimal point; then <c>decimals</c> is applied.
    /// </summary>
    public static class WeightParser
    {
        private static readonly Regex BlockRegex =
            new Regex(@"A0(1[123])\s+(-?\s*\d+(?:[.,]\d+)?)\s*([A-Za-z]{0,3})", RegexOptions.Compiled);

        private static readonly Regex NumberRegex =
            new Regex(@"-?\d+(?:[.,]\d+)?", RegexOptions.Compiled);

        private static readonly Regex StatusRegex =
            new Regex(@"SX\s*([A-Z])?", RegexOptions.Compiled);

        public static bool TryParse(string data, int decimals, out WeightReading reading)
        {
            reading = null;
            if (string.IsNullOrEmpty(data))
                return false;

            WeightReading r = new WeightReading();
            r.Raw = data.Trim();

            Match status = StatusRegex.Match(data);
            if (status.Success && status.Groups[1].Success)
                r.Dynamic = status.Groups[1].Value == "D";

            bool netFound = false;
            foreach (Match block in BlockRegex.Matches(data))
            {
                decimal value = ParseValue(block.Groups[2].Value, decimals);
                string unit = block.Groups[3].Value;
                switch (block.Groups[1].Value)
                {
                    case "11":
                        r.Gross = value;
                        break;
                    case "12":
                        r.Net = value;
                        netFound = true;
                        if (unit.Length > 0)
                            r.Unit = unit;
                        break;
                    case "13":
                        r.Tare = value;
                        break;
                }
            }

            if (!netFound)
            {
                if (r.Gross.HasValue)
                {
                    r.Net = r.Gross.Value - (r.Tare ?? 0m);
                    netFound = true;
                }
                else
                {
                    // Unknown format: use the first number that isn't a block id
                    foreach (Match number in NumberRegex.Matches(data))
                    {
                        if (number.Value.Length == 3 && number.Value.StartsWith("0"))
                            continue;
                        r.Net = ParseValue(number.Value, decimals);
                        netFound = true;
                        break;
                    }
                }
            }

            if (!netFound)
                return false;

            reading = r;
            return true;
        }

        private static decimal ParseValue(string text, int decimals)
        {
            string clean = text.Replace(" ", "").Replace(',', '.');
            decimal value = decimal.Parse(clean, NumberStyles.Number, CultureInfo.InvariantCulture);
            if (clean.IndexOf('.') < 0 && decimals > 0)
                value /= (decimal)Math.Pow(10, decimals);
            return value;
        }
    }
}
