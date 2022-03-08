using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace MettlerToledoLoadCellTool
{
    public static class BitmapConverter
    {
        public static Bitmap CreatTestBitmap(string weight)
        {
            BarcodeLib.Barcode b = new BarcodeLib.Barcode();
            Image img = b.Encode(BarcodeLib.TYPE.EAN13, "872312898734", Color.Black, Color.White, 290, 80);

            Bitmap objBmpImage = new Bitmap(432, 400);

            // Create the Font object for the image text drawing.
            Font titleFont = new Font("Arial", 28, FontStyle.Bold, GraphicsUnit.Pixel);
            Font defaultFont = new Font("Arial", 21, FontStyle.Bold, GraphicsUnit.Pixel);

            // Add the colors to the new bitmap.
            Graphics objGraphics = Graphics.FromImage(objBmpImage);

            // Set Background color
            objGraphics.Clear(Color.White);
            objGraphics.SmoothingMode = SmoothingMode.AntiAlias;
            objGraphics.TextRenderingHint = TextRenderingHint.AntiAlias;
            objGraphics.DrawString("TEST LABEL", defaultFont, new SolidBrush(Color.Black), 150, 10);
            objGraphics.DrawString("Gewicht", defaultFont, new SolidBrush(Color.Black), 50, 150);
            objGraphics.DrawString(weight, titleFont, new SolidBrush(Color.Black), 210, 150);
            objGraphics.DrawImage(img, 80, 280);
            objGraphics.Flush();

            return (objBmpImage);
        }

        public static Bitmap BitmapTo1Bpp2(Bitmap source)
        {
            int Width = source.Width;
            int Height = source.Height;
            source.RotateFlip(RotateFlipType.RotateNoneFlipY);

            Bitmap dest = new Bitmap(Width, Height, PixelFormat.Format1bppIndexed);
            BitmapData destBmpData = dest.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);

            byte[] destBytes = new byte[(Width + 7) / 8];//19 bytes

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Color c = source.GetPixel(x, y);

                    if (x % 8 == 0)
                    {
                        destBytes[x / 8] = 0;
                    }
                    if (c.GetBrightness() >= 0.5)
                    {
                        destBytes[x / 8] |= (byte)(0x80 >> (x % 8));
                    }
                }
                Marshal.Copy(destBytes, 0, (IntPtr)((long)destBmpData.Scan0 + destBmpData.Stride * y), destBytes.Length);
            }

            dest.UnlockBits(destBmpData);
            return dest;
        }

        public static byte[] Convert(Bitmap bmp)
        {
            var size = bmp.Width * bmp.Height / 8;
            var buffer = new byte[size];

            var i = 0;
            for (var y = 0; y < bmp.Height; y++)
            {
                for (var x = 0; x < bmp.Width; x++)
                {
                    var color = bmp.GetPixel(x, y);
                    if (color.B != 255 || color.G != 255 || color.R != 255)
                    {
                        var pos = i / 8;
                        var bitInByteIndex = x % 8;

                        buffer[pos] |= (byte)(1 << 7 - bitInByteIndex);
                    }
                    i++;
                }
            }

            return buffer;
        }
    }
}
