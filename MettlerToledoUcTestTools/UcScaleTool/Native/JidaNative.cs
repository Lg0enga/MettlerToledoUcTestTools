using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UcScaleTool.Native
{
    /// <summary>
    /// P/Invoke declarations for the Kontron JIDA API (jida.dll).
    /// Used on UC scales with a Kontron board (e.g. UC-HTT-M, UC-SPCT-M, UC-CTTM).
    /// See docs/jida32.pdf.
    /// </summary>
    internal static class JidaNative
    {
        private const string Dll = "jida.dll";

        [DllImport(Dll)]
        internal static extern int JidaDllGetVersion();

        [DllImport(Dll)]
        internal static extern bool JidaDllInitialize();

        [DllImport(Dll)]
        internal static extern bool JidaDllIsAvailable();

        [DllImport(Dll)]
        internal static extern bool JidaDllUninitialize();

        /// <param name="boardClassName">NULL counts all boards.</param>
        [DllImport(Dll, CharSet = CharSet.Ansi)]
        internal static extern int JidaBoardCount(string boardClassName, int flags);

        /// <param name="boardClassName">NULL opens any board class.</param>
        [DllImport(Dll, CharSet = CharSet.Ansi)]
        internal static extern bool JidaBoardOpen(string boardClassName, int boardNumber, int flags, ref IntPtr boardHandle);

        [DllImport(Dll, CharSet = CharSet.Ansi)]
        internal static extern bool JidaBoardOpenByNameA(string boardName, ref IntPtr boardHandle);

        [DllImport(Dll)]
        internal static extern bool JidaBoardClose(IntPtr boardHandle);

        [DllImport(Dll, CharSet = CharSet.Ansi)]
        internal static extern bool JidaBoardGetNameA(IntPtr boardHandle, StringBuilder name, int size);

        [DllImport(Dll)]
        internal static extern int JidaI2CCount(IntPtr boardHandle);

        [DllImport(Dll)]
        internal static extern bool JidaI2CIsAvailable(IntPtr boardHandle, int busNumber);

        /// <returns>0 unknown, 1 primary, 2 SMBus, 3 JILI.</returns>
        [DllImport(Dll)]
        internal static extern int JidaI2CType(IntPtr boardHandle, int busNumber);

        /// <param name="address">Full 8-bit address, bit 0 set for a read.</param>
        [DllImport(Dll)]
        internal static extern unsafe bool JidaI2CRead(IntPtr boardHandle, int busNumber, byte address, byte* bytes, int length);

        [DllImport(Dll)]
        internal static extern bool JidaI2CReadRegister(IntPtr boardHandle, int busNumber, byte address, int register, ref byte value);

        [DllImport(Dll)]
        internal static extern bool JidaI2CWriteRegister(IntPtr boardHandle, int busNumber, byte address, int register, byte value);
    }
}
