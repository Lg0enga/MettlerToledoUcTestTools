using System;
using System.IO.Ports;
using System.Text;
using System.Windows.Forms;

namespace MettlerToledoLoadCellTool
{
    public partial class MettlerScaleReader : Form
    {
        SerialPort _serialPort = new SerialPort("COM2", 9600, Parity.Even, 7, StopBits.Two);
        
        Timer _timer = new Timer();

        IntPtr handle = IntPtr.Zero;

        public MettlerScaleReader()
        {
            InitializeComponent();
        }

        private void openBtn_Click(object sender, EventArgs e)
        {
            _serialPort.RtsEnable = false;
            _serialPort.DtrEnable = false;
            _serialPort.Handshake = Handshake.None;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.Open();
            eventBox.Items.Insert(0, "Open serial scale port");
        }

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string data = _serialPort.ReadLine();

            this.BeginInvoke(new SetTextDeleg(si_DataReceived), new object[] { data });
        }

        private delegate void SetTextDeleg(string text);

        private void si_DataReceived(string data) {
            try
            {
                string nettWeight = data.Substring(21, 16);
                string tarraWeight = data.Substring(38, 16);
                weightLabel.Text = nettWeight;
                tarraWeightLabel.Text = tarraWeight;
            }
            catch (Exception ex)
            {
                eventBox.Items.Insert(0, "Error: parsing weight to string");
            }
            eventBox.Items.Insert(0, data.Trim()); 
        }

        private void closeBtn_Click(object sender, EventArgs e)
        {
            _serialPort.DataReceived -= new SerialDataReceivedEventHandler(sp_DataReceived);
            _serialPort.Dispose();
            _serialPort.Close();
            eventBox.Items.Insert(0, "Close serial scale port");
        }

        private void openBoardBtn_Click(object sender, EventArgs e)
        {
            try
            {
                bool status;

                status = NativeMethods.JidaDllInitialize();
                eventBox.Items.Insert(0, "Jida initilize: " + status);
                status = NativeMethods.JidaDllIsAvailable();
                eventBox.Items.Insert(0, "Jida enabled: " + status);

                status = NativeMethods.JidaBoardOpenByNameA("UUP6", ref handle);
                eventBox.Items.Insert(0, "Board opend: " + status + " location: " + handle);

                status = NativeMethods.JidaI2CIsAvailable(handle, 5);
                eventBox.Items.Insert(0, "JidaI2CIsAvailable: " + status);

                var i2cType = NativeMethods.JidaI2CType(handle, 5);
                eventBox.Items.Insert(0, "JidaI2CType: " + i2cType);

                ResetScale();
            }
            catch (Exception ex)
            {
                eventBox.Items.Insert(0, "error: " + ex.Message);
            }
        }   

        private void resetJidaBtn_Click(object sender, EventArgs e)
        {
            ResetScale();
        }

        private void closeBoardBtn_Click(object sender, EventArgs e)
        {
            bool status;
            status = NativeMethods.JidaBoardClose(handle);
            eventBox.Items.Insert(0, "Board closed: " + status);
            status = NativeMethods.JidaDllUninitialize();
            eventBox.Items.Insert(0, "Uninitilize: " + status);
        }

        private void initScaleBtn_Click(object sender, EventArgs e)
        {
            byte[] bytestosend = { 0x16, 0x1B, 0x3F };
            _serialPort.Write(bytestosend, 0, bytestosend.Length);
            eventBox.Items.Insert(0, "Init loadcell");
        }

        private void startReadingWeightBtn_Click(object sender, EventArgs e)
        {
            _timer.Interval = 500;
            _timer.Tick += new EventHandler(timer_Tick);
            _timer.Start();   
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            byte[] bytestosend = { 0x06, 0x53, 0x58, 0x49, 0x0d, 0x0a };
            _serialPort.Write(bytestosend, 0, bytestosend.Length);
        }

        private void stopReadingWeightBtn_Click(object sender, EventArgs e)
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private void ResetScale()
        {
            byte[] buf = { 0x02, 0x29, 0x20, 0x04, 0x04, 0x22, 0x97, 0x71, 0x45, 0x15, 0x00, 0x26 };
            byte unknow = 0x00;
            bool status;

            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x10, ref unknow); // not sure what this does, but best to still perform the read like the original SW
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x94, ref buf[0]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x95, ref buf[1]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x96, ref buf[2]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x97, ref buf[3]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x98, ref buf[4]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x99, ref buf[5]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x9a, ref buf[6]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x9b, ref buf[7]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x9c, ref buf[8]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x9d, ref buf[9]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x9e, ref buf[10]);
            eventBox.Items.Insert(0, "read: " + status);
            status = NativeMethods.JidaI2CReadRegister(handle, 0, 0xc0, 0x9f, ref buf[11]);
            eventBox.Items.Insert(0, "read: " + status);

            eventBox.Items.Insert(0, "reading: " + ByteArrayToString(buf));

            calculateResponse(buf);

            eventBox.Items.Insert(0, "calculated: " + ByteArrayToString(buf));

            status = NativeMethods.JidaI2CWriteRegister(handle, 0, 0xc0, 0xd0, buf[0]);
            eventBox.Items.Insert(0, "write: " + status);
            status = NativeMethods.JidaI2CWriteRegister(handle, 0, 0xc0, 0xd0, buf[1]);
            eventBox.Items.Insert(0, "write: " + status);
            status = NativeMethods.JidaI2CWriteRegister(handle, 0, 0xc0, 0xd0, buf[2]);
            eventBox.Items.Insert(0, "write: " + status);
            status = NativeMethods.JidaI2CWriteRegister(handle, 0, 0xc0, 0xd0, buf[3]);
            eventBox.Items.Insert(0, "write: " + status);
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

        private void tarraScaleBtn_Click(object sender, EventArgs e)
        {
            byte[] bytestosend = { 0x06, 0x54, 0x0d, 0x0a };
            _serialPort.Write(bytestosend, 0, bytestosend.Length);
            eventBox.Items.Insert(0, "Tarra scale");
        }

        private void nullScaleBtn_Click(object sender, EventArgs e)
        {
            byte[] bytestosend = { 0x06, 0x5A, 0x0d, 0x0a };
            _serialPort.Write(bytestosend, 0, bytestosend.Length);
            eventBox.Items.Insert(0, "Set scale to null");
        }
    }
}