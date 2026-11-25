using System;

namespace MettlerToledoLoadCellTool
{
    public class UcLoadcell
    {
        private IntPtr _handle = IntPtr.Zero;
        private int _busNumber;
        private byte _address;

        public UcLoadcell(IntPtr handle, int busNumber, byte address)
        {
            _handle = handle;
            _busNumber = busNumber;
            _address = address;
        }

        public void CloseBoard()
        {
            JiddaWrapper.JidaBoardClose(_handle);
        }

        /// <summary>
        /// Unlocks the loadcell communication via I2C. Always runs the full sequence;
        /// returns false when one or more I2C calls reported a failure.
        /// </summary>
        public bool ReOpenBoardCommunication()
        {
            byte[] buf = { 0x02, 0x29, 0x20, 0x04, 0x04, 0x22, 0x97, 0x71, 0x45, 0x15, 0x00, 0x26 };
            byte unknow = 0x00;
            bool allOk = true;

            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x10, ref unknow);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x94, ref buf[0]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x95, ref buf[1]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x96, ref buf[2]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x97, ref buf[3]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x98, ref buf[4]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x99, ref buf[5]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x9a, ref buf[6]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x9b, ref buf[7]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x9c, ref buf[8]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x9d, ref buf[9]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x9e, ref buf[10]);
            allOk &= JiddaWrapper.JidaI2CReadRegister(_handle, _busNumber, _address, 0x9f, ref buf[11]);

            calculateResponse(buf);

            allOk &= JiddaWrapper.JidaI2CWriteRegister(_handle, _busNumber, _address, 0xd0, buf[0]);
            allOk &= JiddaWrapper.JidaI2CWriteRegister(_handle, _busNumber, _address, 0xd0, buf[1]);
            allOk &= JiddaWrapper.JidaI2CWriteRegister(_handle, _busNumber, _address, 0xd0, buf[2]);
            allOk &= JiddaWrapper.JidaI2CWriteRegister(_handle, _busNumber, _address, 0xd0, buf[3]);
            return allOk;
        }

        private void calculateResponse(byte[] buf)
        {
            byte tmp;

            buf[0] = (byte)(buf[0] + buf[1]);
            tmp = buf[0];
            buf[0] = (byte)(buf[2] + tmp);
            tmp = (byte)(buf[3] + buf[2] + tmp);
            buf[0] = tmp;
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
