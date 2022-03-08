using System;

namespace MettlerToledoLoadCellTool
{
    public class UcLoadcell
    {
        private IntPtr _handle = IntPtr.Zero;

        public UcLoadcell(IntPtr handle)
        {
            _handle = handle;
        }

        public void CloseBoard()
        {
            JiddaWrapper.JidaBoardClose(_handle);
        }

        public void ReOpenBoardCommunication()
        {
            byte[] buf = { 0x02, 0x29, 0x20, 0x04, 0x04, 0x22, 0x97, 0x71, 0x45, 0x15, 0x00, 0x26 };
            byte unknow = 0x00;
            bool status;

            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x10, ref unknow);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x94, ref buf[0]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x95, ref buf[1]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x96, ref buf[2]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x97, ref buf[3]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x98, ref buf[4]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x99, ref buf[5]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x9a, ref buf[6]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x9b, ref buf[7]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x9c, ref buf[8]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x9d, ref buf[9]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x9e, ref buf[10]);
            status = JiddaWrapper.JidaI2CReadRegister(_handle, 0, 0xc0, 0x9f, ref buf[11]);

            calculateResponse(buf);

            status = JiddaWrapper.JidaI2CWriteRegister(_handle, 0, 0xc0, 0xd0, buf[0]);
            status = JiddaWrapper.JidaI2CWriteRegister(_handle, 0, 0xc0, 0xd0, buf[1]);
            status = JiddaWrapper.JidaI2CWriteRegister(_handle, 0, 0xc0, 0xd0, buf[2]);
            status = JiddaWrapper.JidaI2CWriteRegister(_handle, 0, 0xc0, 0xd0, buf[3]);
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
