using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UcScaleTool.Native
{
    /// <summary>
    /// P/Invoke declarations for the MSC "Universal Board Support Package" (UspEpc.dll).
    /// Used on older UC scales with an MSC board running Windows XP (e.g. UC-GTT-S).
    /// Requires the UspEpc.sys driver to be installed on the scale.
    /// </summary>
    internal static class UspEpcNative
    {
        private const string Dll = "UspEpc.dll";

        /// <summary>No register offset (plain read/write), high speed.</summary>
        internal const byte ProtocolNoOffset = 0x80;

        /// <summary>8-bit register offset, high speed. Used by the loadcell unlock.</summary>
        internal const byte Protocol8BitOffset = 0x81;

        [DllImport(Dll)]
        internal static extern bool UspOpen();

        [DllImport(Dll)]
        internal static extern bool UspClose();

        [DllImport(Dll)]
        internal static extern bool UspGetDllVersion(ref uint version);

        /// <summary>
        /// Fills a BOARDINFO structure. The exact size of the trailing BIOSTIME fields is not
        /// documented, so callers pass an oversized unmanaged buffer and only read the
        /// documented leading fields (5 x CHAR[16] + 2 x DWORD).
        /// </summary>
        [DllImport(Dll)]
        internal static extern bool UspGetBoardInfo(IntPtr boardInfo);

        [DllImport(Dll)]
        internal static extern bool UspGetLastError(ref uint errorCode, StringBuilder errorString, uint errorLength);

        [DllImport(Dll)]
        internal static extern bool UspI2CRead8(byte busId, byte devAddress, ushort offset, ref byte value, byte protocol);

        [DllImport(Dll)]
        internal static extern bool UspI2CWrite8(byte busId, byte devAddress, ushort offset, byte value, byte protocol);
    }
}
