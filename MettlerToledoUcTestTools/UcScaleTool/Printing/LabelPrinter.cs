using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using LibUsbDotNet;
using LibUsbDotNet.Main;

namespace UcScaleTool.Printing
{
    public enum PrinterProtocol
    {
        /// <summary>Command set of the working XP version (with status check).</summary>
        Standard,

        /// <summary>Command set of the first version of the tool.</summary>
        Legacy
    }

    /// <summary>
    /// Own delegate instead of Func&lt;,,&gt;: Func lives in System.Core (.NET 3.5), which
    /// some XP scales don't have.
    /// </summary>
    internal delegate T PrinterAction<T>(UsbEndpointWriter writer, UsbEndpointReader reader);

    public enum PrinterStatus
    {
        Normal,
        RemoveLabel,
        EndOfRoll,
        Error
    }

    /// <summary>
    /// The built-in UC label printer ("Mettler UCP00001H432"), accessed over USB with
    /// LibUsbDotNet. Needs the libusb driver (install with Zadig, see README).
    /// The device is opened per job, like the XP version does.
    /// </summary>
    public static class LabelPrinter
    {
        private const int VendorId = 0x0EB8;
        private const int ProductId = 0x3000;
        private const int Timeout = 500;
        private const int ChunkSize = 4000;

        private const int MaxAttempts = 20;
        private const int RetryDelayMs = 1500;

        /// <summary>
        /// Prints a label; while the printer reports "remove label" it retries for a while.
        /// Blocking: call from a background thread. <paramref name="progress"/> gets status text.
        /// </summary>
        public static PrinterStatus Print(Bitmap label, PrinterProtocol protocol, Action<string> progress)
        {
            byte[] data = protocol == PrinterProtocol.Standard ? ToStandardData(label) : ToLegacyData(label);

            for (int attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                PrinterStatus status = PrintOnce(data, protocol, progress);
                if (status != PrinterStatus.RemoveLabel)
                    return status;

                progress("Haal het vorige label weg...");
                Thread.Sleep(RetryDelayMs);
            }
            return PrinterStatus.RemoveLabel;
        }

        public static void Feed(Action<string> progress)
        {
            UsbDevice device = OpenDevice();
            try
            {
                using (UsbEndpointWriter writer = device.OpenEndpointWriter(WriteEndpointID.Ep02, EndpointType.Bulk))
                using (UsbEndpointReader reader = device.OpenEndpointReader(ReadEndpointID.Ep02))
                {
                    byte[] buffer = new byte[1024];
                    int length;
                    writer.Write(new byte[] { 0x01, 0x1B, (byte)'S' }, Timeout, out length);
                    reader.Read(buffer, Timeout, out length);
                }
                progress("Label doorgevoerd");
            }
            finally
            {
                CloseDevice(device);
            }
        }

        private static PrinterStatus PrintOnce(byte[] data, PrinterProtocol protocol, Action<string> progress)
        {
            UsbDevice device = OpenDevice();
            try
            {
                using (UsbEndpointWriter writer = device.OpenEndpointWriter(WriteEndpointID.Ep02, EndpointType.Bulk))
                using (UsbEndpointReader reader = device.OpenEndpointReader(ReadEndpointID.Ep02))
                {
                    byte[] response = new byte[1024];
                    int length;

                    foreach (byte[] command in SetupCommands(protocol))
                    {
                        Array.Clear(response, 0, response.Length);
                        writer.Write(command, Timeout, out length);
                        reader.Read(response, Timeout, out length);
                    }

                    PrinterStatus status = ParseStatus(response);
                    if (status != PrinterStatus.Normal)
                    {
                        writer.Abort();
                        return status;
                    }

                    progress("Label wordt geprint...");
                    foreach (byte[] chunk in Split(data, ChunkSize))
                    {
                        ErrorCode error = writer.Write(chunk, Timeout, out length);
                        reader.Read(response, Timeout, out length);
                        if (error == ErrorCode.IoTimedOut)
                            writer.Reset();
                    }
                    return PrinterStatus.Normal;
                }
            }
            finally
            {
                CloseDevice(device);
            }
        }

        private static IEnumerable<byte[]> SetupCommands(PrinterProtocol protocol)
        {
            if (protocol == PrinterProtocol.Standard)
            {
                // As in the XP version (PrinterService.PrintLabel)
                yield return new byte[] { 0x01, 0x1B, (byte)'d' };
                yield return new byte[] { 0x02, 0x1B, (byte)'Z' };
                yield return new byte[] { 0x03, 0x1B, 0xBE };
                yield return Command(0x04, "W4320850");
            }
            else
            {
                // As in the first version of the tool
                yield return Command(0x01, "d1112");
                yield return Command(0x02, "]01648");
                yield return new byte[] { 0x03, 0x1B, (byte)'~' };
                yield return new byte[] { 0x04, 0x1B, (byte)'Z', 0x0D, 0x0A };
                yield return new byte[] { 0x05, 0x1B, (byte)'Z' };
                yield return new byte[] { 0x06, 0x1B, (byte)'Z' };
                yield return new byte[] { 0x07, 0x1B, 0xBE };
                yield return Command(0x08, "W4320400");
            }
        }

        private static byte[] Command(byte sequence, string text)
        {
            byte[] command = new byte[text.Length + 2];
            command[0] = sequence;
            command[1] = 0x1B;
            for (int i = 0; i < text.Length; i++)
                command[i + 2] = (byte)text[i];
            return command;
        }

        /// <summary>
        /// Opens the printer, runs <paramref name="action"/> with its bulk endpoints and
        /// closes the printer again.
        /// </summary>
        internal static T WithPrinter<T>(PrinterAction<T> action)
        {
            UsbDevice device = OpenDevice();
            try
            {
                using (UsbEndpointWriter writer = device.OpenEndpointWriter(WriteEndpointID.Ep02, EndpointType.Bulk))
                using (UsbEndpointReader reader = device.OpenEndpointReader(ReadEndpointID.Ep02))
                {
                    return action(writer, reader);
                }
            }
            finally
            {
                CloseDevice(device);
            }
        }

        /// <summary>Writes a command and reads the printer's answer into <paramref name="response"/>.</summary>
        internal static void Exchange(UsbEndpointWriter writer, UsbEndpointReader reader, byte[] command, byte[] response)
        {
            int length;
            Array.Clear(response, 0, response.Length);
            writer.Write(command, Timeout, out length);
            reader.Read(response, Timeout, out length);
        }

        private static UsbDevice OpenDevice()
        {
            UsbDevice device = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(VendorId, ProductId));
            if (device == null)
                throw new InvalidOperationException(
                    "Printer niet gevonden (USB 0EB8:3000). Is de libusb-driver met Zadig geinstalleerd?");

            IUsbDevice wholeDevice = device as IUsbDevice;
            if (!ReferenceEquals(wholeDevice, null))
            {
                wholeDevice.SetConfiguration(1);
                wholeDevice.ClaimInterface(0);
            }
            return device;
        }

        private static void CloseDevice(UsbDevice device)
        {
            try
            {
                IUsbDevice wholeDevice = device as IUsbDevice;
                if (!ReferenceEquals(wholeDevice, null))
                    wholeDevice.ReleaseInterface(0);
                device.Close();
            }
            catch (Exception)
            {
                // Closing a device that's gone is not an error worth reporting
            }
        }

        private static PrinterStatus ParseStatus(byte[] response)
        {
            if (Contains(response, new byte[] { 0x30, 0x15, 0xBE }))
                return PrinterStatus.RemoveLabel;
            if (Contains(response, new byte[] { 0x31, 0x15, 0xBE }))
                return PrinterStatus.EndOfRoll;
            return PrinterStatus.Normal;
        }

        private static bool Contains(byte[] haystack, byte[] needle)
        {
            for (int i = 0; i <= haystack.Length - needle.Length; i++)
            {
                int j = 0;
                while (j < needle.Length && haystack[i + j] == needle[j])
                    j++;
                if (j == needle.Length)
                    return true;
            }
            return false;
        }

        private static IEnumerable<byte[]> Split(byte[] data, int size)
        {
            for (int offset = 0; offset < data.Length; offset += size)
            {
                int length = Math.Min(size, data.Length - offset);
                byte[] chunk = new byte[length];
                Buffer.BlockCopy(data, offset, chunk, 0, length);
                yield return chunk;
            }
        }

        /// <summary>
        /// XP version: flip vertically, 1 bit per pixel with 1 = white, rows packed.
        /// </summary>
        private static byte[] ToStandardData(Bitmap label)
        {
            int width = label.Width;
            int height = label.Height;
            int bytesPerRow = (width + 7) / 8;
            byte[] data = new byte[bytesPerRow * height];
            for (int y = 0; y < height; y++)
            {
                int sourceY = height - 1 - y;
                for (int x = 0; x < width; x++)
                {
                    if (label.GetPixel(x, sourceY).GetBrightness() >= 0.5f)
                        data[y * bytesPerRow + x / 8] |= (byte)(0x80 >> (x % 8));
                }
            }
            return data;
        }

        /// <summary>
        /// First version: no flip, 1 bit per pixel with 1 = black (any non-white pixel).
        /// </summary>
        private static byte[] ToLegacyData(Bitmap label)
        {
            int width = label.Width;
            int height = label.Height;
            int bytesPerRow = (width + 7) / 8;
            byte[] data = new byte[bytesPerRow * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color c = label.GetPixel(x, y);
                    if (c.R != 255 || c.G != 255 || c.B != 255)
                        data[y * bytesPerRow + x / 8] |= (byte)(0x80 >> (x % 8));
                }
            }
            return data;
        }
    }
}
