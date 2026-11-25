using System;
using System.Collections.Generic;
using System.Text;
using UcScaleTool.Native;

namespace UcScaleTool.Hardware
{
    /// <summary>
    /// Board access through Kontron JIDA (jida.dll).
    /// </summary>
    public sealed class JidaBackend : IBoardBackend
    {
        private IntPtr _handle = IntPtr.Zero;
        private bool _initialized;

        private JidaBackend()
        {
        }

        public BackendKind Kind { get { return BackendKind.Jida; } }

        public string BoardName { get; private set; }

        public string BoardDescription { get { return "JIDA board " + BoardName; } }

        public int DefaultLoadcellBus { get { return 0; } }

        public int BusCount { get { return JidaNative.JidaI2CCount(_handle); } }

        /// <summary>
        /// Opens the JIDA board. With an empty <paramref name="boardName"/> (or a name that
        /// isn't found) the first board is opened. Throws <see cref="BoardException"/> on failure.
        /// </summary>
        public static JidaBackend Open(string boardName, List<string> log)
        {
            JidaBackend backend = new JidaBackend();
            try
            {
                if (!JidaNative.JidaDllInitialize() || !JidaNative.JidaDllIsAvailable())
                    throw new BoardException("JIDA driver not available (run as administrator?)");
                backend._initialized = true;

                boardName = (boardName ?? "").Trim();
                IntPtr handle = IntPtr.Zero;
                if (boardName.Length > 0)
                {
                    if (JidaNative.JidaBoardOpenByNameA(boardName, ref handle) && handle != IntPtr.Zero)
                    {
                        backend._handle = handle;
                        backend.BoardName = boardName;
                        return backend;
                    }
                    log.Add(string.Format("JIDA board '{0}' not found, opening first board", boardName));
                }

                handle = IntPtr.Zero;
                if (!JidaNative.JidaBoardOpen(null, 0, 0, ref handle) || handle == IntPtr.Zero)
                    throw new BoardException("No JIDA board could be opened");

                backend._handle = handle;
                backend.BoardName = GetBoardName(handle);
                return backend;
            }
            catch
            {
                backend.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Lists the names of all JIDA boards. Assumes JIDA is initialized.
        /// </summary>
        internal static List<string> ListBoardNames()
        {
            List<string> names = new List<string>();
            int count = JidaNative.JidaBoardCount(null, 0);
            for (int i = 0; i < count; i++)
            {
                IntPtr handle = IntPtr.Zero;
                if (!JidaNative.JidaBoardOpen(null, i, 0, ref handle) || handle == IntPtr.Zero)
                    continue;
                names.Add(GetBoardName(handle));
                JidaNative.JidaBoardClose(handle);
            }
            return names;
        }

        internal static string GetBoardName(IntPtr handle)
        {
            try
            {
                StringBuilder name = new StringBuilder(64);
                if (JidaNative.JidaBoardGetNameA(handle, name, name.Capacity))
                    return name.ToString().Trim();
            }
            catch (EntryPointNotFoundException)
            {
                // Old jida.dll without JidaBoardGetNameA
            }
            return "<unknown>";
        }

        public string DescribeBus(int bus)
        {
            if (!JidaNative.JidaI2CIsAvailable(_handle, bus))
                return "not available";
            switch (JidaNative.JidaI2CType(_handle, bus))
            {
                case 1: return "PRIMARY";
                case 2: return "SMB";
                case 3: return "JILI";
                default: return "UNKNOWN";
            }
        }

        public bool ReadRegister(int bus, byte address, int register, out byte value)
        {
            value = 0;
            return JidaNative.JidaI2CReadRegister(_handle, bus, address, register, ref value);
        }

        public bool WriteRegister(int bus, byte address, int register, byte value)
        {
            return JidaNative.JidaI2CWriteRegister(_handle, bus, address, register, value);
        }

        public unsafe bool Probe(int bus, byte address)
        {
            byte value = 0;
            return JidaNative.JidaI2CRead(_handle, bus, (byte)(address | 0x01), &value, 1);
        }

        public void Dispose()
        {
            if (_handle != IntPtr.Zero)
            {
                JidaNative.JidaBoardClose(_handle);
                _handle = IntPtr.Zero;
            }
            if (_initialized)
            {
                JidaNative.JidaDllUninitialize();
                _initialized = false;
            }
        }
    }
}
