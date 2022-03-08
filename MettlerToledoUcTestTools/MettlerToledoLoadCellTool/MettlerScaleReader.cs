using LibUsbDotNet;
using LibUsbDotNet.Main;
using System;
using System.IO.Ports;
using System.Windows.Forms;

namespace MettlerToledoLoadCellTool
{
    public partial class MettlerScaleReader : Form
    {
        private SerialPort _serialPort = new SerialPort("COM2", 9600, Parity.Even, 7, StopBits.Two);

        private Timer _timer = new Timer();
        private Timer _jidaTimer = new Timer();

        private UcLoadcell _ucLoadcell;
        private UsbDevice _evoLinePrinter;

        public MettlerScaleReader()
        {
            InitializeComponent();
        }

        //
        // Loadcell
        //

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
    
        private void timer_Tick(object sender, EventArgs e)
        {
            byte[] bytestosend = { 0x06, 0x53, 0x58, 0x49, 0x0d, 0x0a };
            _serialPort.Write(bytestosend, 0, bytestosend.Length);
        }

        private void jidaTimer_Tick(object sender, EventArgs e)
        {
            _ucLoadcell.ReOpenBoardCommunication();
        }

        private void openScaleBtn_Click(object sender, EventArgs e)
        {
            try
            {
                // Open serial port
                _serialPort.RtsEnable = false;
                _serialPort.DtrEnable = false;
                _serialPort.Handshake = Handshake.None;
                _serialPort.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
                _serialPort.Open();
                eventBox.Items.Insert(0, "Open serial scale port");

                // Open jida board
                var initializeStatus = JiddaWrapper.JidaDllInitialize();
                if (initializeStatus && JiddaWrapper.JidaDllIsAvailable())
                {
                    IntPtr handle = IntPtr.Zero;
                    JiddaWrapper.JidaBoardOpenByNameA("UUP6", ref handle);
                    _ucLoadcell = new UcLoadcell(handle);
                    _ucLoadcell.ReOpenBoardCommunication();

                    // Send inital command for loadcell
                    byte[] bytestosend = { 0x16, 0x1B, 0x3F };
                    _serialPort.Write(bytestosend, 0, bytestosend.Length);

                    // Setup weight reading timer
                    _timer.Interval = 500;
                    _timer.Tick += new EventHandler(timer_Tick);
                    _timer.Start();

                    // Setup jida board reopen timer
                    _jidaTimer.Interval = 30000;
                    _jidaTimer.Tick += new EventHandler(timer_Tick);
                    _jidaTimer.Start();

                    nullScaleBtn.Enabled = true;
                    tarraScaleBtn.Enabled = true;
                    closeScaleBtn.Enabled = true;
                }
                else
                {
                    eventBox.Items.Insert(0, "No access to jida board. Open application as administrator");
                }
            }
            catch (Exception ex)
            {
                eventBox.Items.Insert(0, $"Open scale failed {ex.Message}");
            }
        }

        private void closeScaleBtn_click(object sender, EventArgs e)
        {
            try
            {
                // Stop weight reading
                _timer.Stop();
                _timer.Dispose();                              

                // Close Jida
                JiddaWrapper.JidaDllUninitialize();
                _jidaTimer.Stop();
                _jidaTimer.Dispose();

                // Close serial port
                _serialPort.DataReceived -= new SerialDataReceivedEventHandler(sp_DataReceived);
                _serialPort.Dispose();
                _serialPort.Close();
                eventBox.Items.Insert(0, "Close serial scale port");

                nullScaleBtn.Enabled = false;
                tarraScaleBtn.Enabled = false;
                closeScaleBtn.Enabled = false;

            }
            catch(Exception ex)
            {
                eventBox.Items.Insert(0, "Error when closing scale");
            }
        }

        private void nullScaleBtn_Click_1(object sender, EventArgs e)
        {
            byte[] bytestosend = { 0x06, 0x5A, 0x0d, 0x0a };
            _serialPort.Write(bytestosend, 0, bytestosend.Length);
            eventBox.Items.Insert(0, "Set scale to null");
        }

        private void tarraScaleBtn_Click_1(object sender, EventArgs e)
        {
            byte[] bytestosend = { 0x06, 0x54, 0x0d, 0x0a };
            _serialPort.Write(bytestosend, 0, bytestosend.Length);
            eventBox.Items.Insert(0, "Tarra scale");
        }

        //
        // Printer
        //

        private void openPrinterBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _evoLinePrinter = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(0x0EB8, 0x3000));
                _evoLinePrinter.Open();

                IUsbDevice wholeUsbDevice = _evoLinePrinter as IUsbDevice;
                if (!ReferenceEquals(wholeUsbDevice, null))
                {
                    wholeUsbDevice.SetConfiguration(1);

                    wholeUsbDevice.ClaimInterface(0);
                }

                eventBox.Items.Add("Open usb device printer");

                printTestLabelBtn.Enabled = true;
                feedLabelBtn.Enabled = true;
                closePrinterBtn.Enabled = true;
            }
            catch (Exception ex)
            {
                eventBox.Items.Add(ex.Message);
            }
        }      

        private void feedLabelBtn_Click(object sender, EventArgs e)
        {
            try
            {
                var evoEndpointWriter = _evoLinePrinter.OpenEndpointWriter(WriteEndpointID.Ep02, EndpointType.Bulk);
                evoEndpointWriter.Write(new byte[] { 0x01, 0x1b, 0x53 }, 1000, out int outer);
                evoEndpointWriter.Dispose();
                eventBox.Items.Add("Feed evo line printer");
            }
            catch (Exception ex)
            {
                eventBox.Items.Add(ex.Message);
            }
        }

        private void closePrinterBtn_Click(object sender, EventArgs e)
        {
            IUsbDevice wholeUsbDevice = _evoLinePrinter as IUsbDevice;
            if (!ReferenceEquals(wholeUsbDevice, null))
            {
                wholeUsbDevice.ReleaseInterface(0);
            }
            _evoLinePrinter.Close();
            UsbDevice.Exit();

            printTestLabelBtn.Enabled = false;
            feedLabelBtn.Enabled = false;
            closePrinterBtn.Enabled = false;
        }

        private void printTestLabelBtn_Click(object sender, EventArgs e)
        {
            int lenght;
            byte[] data = new byte[1024];
            ErrorCode errorCode;
            int timeout = 500;

            try
            {
                // Open read and writer endpoint
                var evoEndpointWriter = _evoLinePrinter.OpenEndpointWriter(WriteEndpointID.Ep02, EndpointType.Bulk);
                var evoEndpointReader = _evoLinePrinter.OpenEndpointReader(ReadEndpointID.Ep02);

                // Create label
                var bitmap = BitmapConverter.CreatTestBitmap(weightLabel.Text);
                bitmap = BitmapConverter.BitmapTo1Bpp2(bitmap);
                var command = BitmapConverter.Convert(bitmap);

                var arrays = Utils.Split(command, 4000);  
              
                errorCode = evoEndpointWriter.Write(new byte[] { 0x01, 0x1b, 0x64, 0x31, 0x31, 0x31, 0x32 }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");
                errorCode = evoEndpointWriter.Write(new byte[] { 0x02, 0x1b, 0x5d, 0x30, 0x31, 0x36, 0x34, 0x38 }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");
                errorCode = evoEndpointWriter.Write(new byte[] { 0x03, 0x1b, 0x7e }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");
                errorCode = evoEndpointWriter.Write(new byte[] { 0x04, 0x1b, 0x5a, 0x0d, 0x0a }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");
                errorCode = evoEndpointWriter.Write(new byte[] { 0x05, 0x1b, 0x5a }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");
                errorCode = evoEndpointWriter.Write(new byte[] { 0x06, 0x1b, 0x5a }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");
                errorCode = evoEndpointWriter.Write(new byte[] { 0x07, 0x1b, 0xbe }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");
                errorCode = evoEndpointWriter.Write(new byte[] { 0x08, 0x1b, 0x57, 0x34, 0x33, 0x32, 0x30, 0x34, 0x30, 0x30 }, timeout, out lenght);
                evoEndpointReader.Read(data, timeout, out lenght);
                eventBox.Items.Add($"Write status: {errorCode} + {Utils.ByteArrayToString(data)}");

                foreach (var array in arrays)
                {
                    errorCode = evoEndpointWriter.Write(array, timeout, out lenght);
                    evoEndpointReader.Read(data, timeout, out lenght);
                    eventBox.Items.Add($"Write label status: {errorCode} + {Utils.ByteArrayToString(data)}");
                    if (errorCode == ErrorCode.IoTimedOut)
                    {
                        evoEndpointWriter.Reset();
                    }
                }
                
                // Close writer
                evoEndpointWriter.Reset();
                evoEndpointWriter.Abort();
                evoEndpointWriter.Dispose();

                // Close reader
                evoEndpointReader.Reset();
                evoEndpointReader.Abort();
                evoEndpointReader.Dispose();

                eventBox.Items.Add("Print test label evo line printer");
            }
            catch (Exception ex)
            {
                eventBox.Items.Add(ex.Message);
            }
        }
    }
}