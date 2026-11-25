using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using BarcodeLib;

namespace UcScaleTool.Printing
{
    public class LabelData
    {
        public string ProductName = "";
        public decimal WeightKg;
        public string WeightText = "";

        /// <summary>Null when no price is used.</summary>
        public decimal? PricePerKg;

        public int Plu = 1;
        public DateTime Timestamp = DateTime.Now;

        public decimal? TotalPrice
        {
            get
            {
                if (!PricePerKg.HasValue)
                    return null;
                return Math.Round(WeightKg * PricePerKg.Value, 2, MidpointRounding.AwayFromZero);
            }
        }
    }

    /// <summary>
    /// Draws the label bitmap (432 dots = 54 mm wide at 203 dpi).
    /// </summary>
    public static class LabelRenderer
    {
        public const int Width = 432;
        public const int Height = 400;

        private static readonly CultureInfo Dutch = new CultureInfo("nl-NL");

        public static Bitmap Render(LabelData data)
        {
            Bitmap bitmap = new Bitmap(Width, Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            using (Font titleFont = new Font("Arial", 30f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font labelFont = new Font("Arial", 22f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Font valueFont = new Font("Arial", 34f, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Font smallFont = new Font("Arial", 18f, FontStyle.Regular, GraphicsUnit.Pixel))
            using (Pen linePen = new Pen(Color.Black, 2f))
            {
                g.Clear(Color.White);
                g.SmoothingMode = SmoothingMode.None;
                // Crisp 1-bit text, the label is converted to black/white anyway
                g.TextRenderingHint = TextRenderingHint.SingleBitPerPixelGridFit;

                StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
                StringFormat right = new StringFormat { Alignment = StringAlignment.Far };
                Brush black = Brushes.Black;

                string title = string.IsNullOrEmpty(data.ProductName) ? "TEST LABEL" : data.ProductName;
                g.DrawString(title, titleFont, black, new RectangleF(8, 8, Width - 16, 40), center);
                g.DrawString(data.Timestamp.ToString("dd-MM-yyyy HH:mm", Dutch), smallFont, black,
                    new RectangleF(8, 48, Width - 16, 24), center);
                g.DrawLine(linePen, 10, 78, Width - 10, 78);

                float y = 92;
                g.DrawString("Gewicht", labelFont, black, 14, y + 8);
                g.DrawString(data.WeightText, valueFont, black, new RectangleF(140, y, Width - 154, 44), right);
                y += 52;

                decimal? total = data.TotalPrice;
                if (total.HasValue)
                {
                    g.DrawString("Prijs/kg", labelFont, black, 14, y + 4);
                    g.DrawString(FormatMoney(data.PricePerKg.Value), labelFont, black,
                        new RectangleF(140, y + 4, Width - 154, 30), right);
                    y += 40;
                    g.DrawLine(linePen, 10, y, Width - 10, y);
                    y += 8;
                    g.DrawString("Te betalen", labelFont, black, 14, y + 8);
                    g.DrawString(FormatMoney(total.Value), valueFont, black,
                        new RectangleF(140, y, Width - 154, 44), right);
                }

                DrawBarcode(g, data, smallFont);
            }
            return bitmap;
        }

        public static string FormatMoney(decimal value)
        {
            return "€ " + value.ToString("0.00", Dutch);
        }

        /// <summary>
        /// In-store EAN-13: "21" + PLU + price in cents, or "28" + PLU + weight in grams
        /// when no price is used. BarcodeLib adds the check digit.
        /// </summary>
        public static string BarcodeData(LabelData data)
        {
            int plu = Math.Max(0, Math.Min(99999, data.Plu));
            decimal? total = data.TotalPrice;
            int value;
            string prefix;
            if (total.HasValue)
            {
                prefix = "21";
                value = (int)Math.Min(99999m, Math.Max(0m, total.Value * 100m));
            }
            else
            {
                prefix = "28";
                value = (int)Math.Min(99999m, Math.Max(0m, data.WeightKg * 1000m));
            }
            return prefix + plu.ToString("00000") + value.ToString("00000");
        }

        /// <summary>EAN-13 check digit for a 12 digit code.</summary>
        public static int Ean13CheckDigit(string code12)
        {
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = code12[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            return (10 - (sum % 10)) % 10;
        }

        private static void DrawBarcode(Graphics g, LabelData data, Font textFont)
        {
            string code = BarcodeData(data);
            string fullCode = code + Ean13CheckDigit(code);
            Rectangle bars = new Rectangle(66, Height - 118, 300, 84);
            Rectangle text = new Rectangle(0, bars.Bottom + 4, Width, 26);
            try
            {
                // IncludeLabel crashes in BarcodeLib 1.3, so the digits are drawn below
                Barcode barcode = new Barcode();
                using (Image image = barcode.Encode(TYPE.EAN13, code, Color.Black, Color.White, bars.Width, bars.Height))
                {
                    g.DrawImage(image, bars.X, bars.Y, bars.Width, bars.Height);
                }
            }
            catch (Exception)
            {
                // Still print the number so the label stays usable
            }
            StringFormat center = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(fullCode, textFont, Brushes.Black, text, center);
        }
    }
}
