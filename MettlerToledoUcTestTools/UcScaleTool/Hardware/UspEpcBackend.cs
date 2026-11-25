using System;
using System.Runtime.InteropServices;
using System.Text;
using UcScaleTool.Native;

namespace UcScaleTool.Hardware
{
    /// <summary>
    /// Board access through the MSC Universal Board Support Package (UspEpc.dll).
    /// </summary>
    public sealed class UspEpcBackend : IBoardBackend
    {
        private bool _open;

        private UspEpcBackend()
        {
        }

        public BackendKind Kind { get { return BackendKind.UspEpc; } }

        public UspBoardInfo Info { get; private set; }

        public string BoardDescription
        {
            get { return "UspEpc board " + (Info != null ? Info.Platform : "<unknown>"); }
        }

        /// <summary>The XP version that works on the UC-GTT-S uses bus 1.</summary>
        public int DefaultLoadcellBus { get { return 1; } }

        /// <summary>UspEpc has no call to count buses.</summary>
        public int BusCount { get { return -1; } }

        /// <summary>Throws <see cref="BoardException"/> when UspOpen fails.</summary>
        public static UspEpcBackend Open()
        {
            if (!UspEpcNative.UspOpen())
                throw new BoardException("UspOpen failed: " + LastError());

            UspEpcBackend backend = new UspEpcBackend();
            backend._open = true;
            backend.Info = ReadBoardInfo();
            return backend;
        }

        internal static UspBoardInfo ReadBoardInfo()
        {
            // Oversized buffer: the documented part is 88 bytes, BIOSTIME size is unknown
            IntPtr buffer = Marshal.AllocHGlobal(512);
            try
            {
                for (int i = 0; i < 512; i++)
                    Marshal.WriteByte(buffer, i, 0);
                if (!UspEpcNative.UspGetBoardInfo(buffer))
                    return null;

                UspBoardInfo info = new UspBoardInfo();
                info.Platform = ReadFixedString(buffer, 0);
                info.BiosVersion = ReadFixedString(buffer, 16);
                info.BootBlockVersion = ReadFixedString(buffer, 32);
                info.HardwareRevision = ReadFixedString(buffer, 48);
                info.SerialNumber = ReadFixedString(buffer, 64);
                info.BootCounter = (uint)Marshal.ReadInt32(buffer, 80);
                info.OperatingHours = (uint)Marshal.ReadInt32(buffer, 84);
                return info;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }

        private static string ReadFixedString(IntPtr buffer, int offset)
        {
            StringBuilder sb = new StringBuilder(16);
            for (int i = 0; i < 16; i++)
            {
                byte b = Marshal.ReadByte(buffer, offset + i);
                if (b == 0)
                    break;
                sb.Append((char)b);
            }
            return sb.ToString().Trim();
        }

        internal static string LastError()
        {
            try
            {
                uint code = 0;
                StringBuilder text = new StringBuilder(128);
                UspEpcNative.UspGetLastError(ref code, text, (uint)text.Capacity);
                return string.Format("{0} (code {1})", text, code);
            }
            catch (Exception)
            {
                return "unknown error";
            }
        }

        public string DescribeBus(int bus)
        {
            return "UspEpc bus " + bus;
        }

        public bool ReadRegister(int bus, byte address, int register, out byte value)
        {
            value = 0;
            return UspEpcNative.UspI2CRead8((byte)bus, address, (ushort)register, ref value, UspEpcNative.Protocol8BitOffset);
        }

        public bool WriteRegister(int bus, byte address, int register, byte value)
        {
            return UspEpcNative.UspI2CWrite8((byte)bus, address, (ushort)register, value, UspEpcNative.Protocol8BitOffset);
        }

        public bool Probe(int bus, byte address)
        {
            byte value = 0;
            return UspEpcNative.UspI2CRead8((byte)bus, address, 0, ref value, UspEpcNative.ProtocolNoOffset);
        }

        public void Dispose()
        {
            if (_open)
            {
                UspEpcNative.UspClose();
                _open = false;
            }
        }
    }

    public class UspBoardInfo
    {
        public string Platform;
        public string BiosVersion;
        public string BootBlockVersion;
        public string HardwareRevision;
        public string SerialNumber;
        public uint BootCounter;
        public uint OperatingHours;
    }

    public class BoardException : Exception
    {
        public BoardException(string message) : base(message)
        {
        }
    }
}
