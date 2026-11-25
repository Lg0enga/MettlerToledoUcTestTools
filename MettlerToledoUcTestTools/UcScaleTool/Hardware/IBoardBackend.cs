using System;

namespace UcScaleTool.Hardware
{
    public enum BackendKind
    {
        Auto,
        Jida,
        UspEpc
    }

    /// <summary>
    /// Access to the I2C bus of the scale's CPU board, independent of the board support
    /// library (JIDA or UspEpc).
    /// </summary>
    public interface IBoardBackend : IDisposable
    {
        BackendKind Kind { get; }

        /// <summary>Human readable description of the opened board, for the log/UI.</summary>
        string BoardDescription { get; }

        /// <summary>Default I2C bus of the loadcell for this backend.</summary>
        int DefaultLoadcellBus { get; }

        /// <summary>Number of I2C buses, or -1 when the library can't tell (UspEpc).</summary>
        int BusCount { get; }

        string DescribeBus(int bus);

        /// <summary>Reads one register (writes the register index first, then reads).</summary>
        bool ReadRegister(int bus, byte address, int register, out byte value);

        bool WriteRegister(int bus, byte address, int register, byte value);

        /// <summary>
        /// Checks whether a device answers on the address using a plain read only
        /// (nothing is written to the device).
        /// </summary>
        bool Probe(int bus, byte address);
    }
}
