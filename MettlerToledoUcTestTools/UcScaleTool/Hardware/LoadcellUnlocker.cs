namespace UcScaleTool.Hardware
{
    /// <summary>
    /// The UC loadcell only talks on COM2 after a challenge/response over I2C: read 12
    /// challenge bytes (registers 0x94-0x9F), calculate a response and write 4 bytes to
    /// register 0xD0. This must be repeated about every 30 seconds.
    /// </summary>
    public static class LoadcellUnlocker
    {
        public const byte DefaultAddress = 0xC0;

        private const int WakeRegister = 0x10;
        private const int FirstChallengeRegister = 0x94;
        private const int ResponseRegister = 0xD0;

        /// <summary>
        /// Runs the full sequence (also when a step fails, like the original tool did).
        /// Returns false when one or more I2C calls reported a failure.
        /// </summary>
        public static bool Unlock(IBoardBackend board, int bus, byte address)
        {
            byte[] buf = { 0x02, 0x29, 0x20, 0x04, 0x04, 0x22, 0x97, 0x71, 0x45, 0x15, 0x00, 0x26 };
            bool allOk = true;

            byte ignored;
            allOk &= board.ReadRegister(bus, address, WakeRegister, out ignored);

            for (int i = 0; i < buf.Length; i++)
            {
                byte value;
                if (board.ReadRegister(bus, address, FirstChallengeRegister + i, out value))
                    buf[i] = value;
                else
                    allOk = false;
            }

            CalculateResponse(buf);

            for (int i = 0; i < 4; i++)
                allOk &= board.WriteRegister(bus, address, ResponseRegister, buf[i]);

            return allOk;
        }

        private static void CalculateResponse(byte[] buf)
        {
            byte tmp;

            buf[0] = (byte)(buf[0] + buf[1]);
            tmp = buf[0];
            buf[0] = (byte)(buf[2] + tmp);
            tmp = (byte)(buf[3] + buf[2] + tmp);
            buf[0] = tmp;
            buf[3] = tmp;
            buf[4] = (byte)(buf[4] + buf[5]);
            tmp = buf[4];
            buf[4] = (byte)(buf[6] + tmp);
            tmp = (byte)(buf[7] + buf[6] + tmp);
            buf[4] = tmp;
            buf[1] = tmp;
            buf[3] = (byte)(buf[3] + buf[4]);
            buf[8] = (byte)(buf[8] + buf[9]);
            tmp = buf[8];
            buf[8] = (byte)(buf[10] + tmp);
            tmp = (byte)(buf[0xb] + buf[10] + tmp);
            buf[8] = tmp;
            buf[2] = tmp;
            buf[3] = (byte)(buf[3] + buf[8]);
        }
    }
}
