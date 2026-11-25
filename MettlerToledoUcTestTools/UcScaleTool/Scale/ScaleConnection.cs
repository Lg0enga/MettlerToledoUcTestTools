using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;
using UcScaleTool.Hardware;

namespace UcScaleTool.Scale
{
    /// <summary>
    /// Connection to the UC loadcell: unlocks it over I2C (and re-unlocks every 30 s),
    /// polls the weight over the serial port and sends zero/tare commands.
    /// Must be created and used on the UI thread; events are raised on the UI thread.
    /// </summary>
    public sealed class ScaleConnection : IDisposable
    {
        private static readonly byte[] InitCommand = { 0x16, 0x1B, 0x3F };
        private static readonly byte[] WeightRequestCommand = { 0x06, (byte)'S', (byte)'X', (byte)'I', 0x0D, 0x0A };
        private static readonly byte[] ZeroCommand = { 0x06, (byte)'Z', 0x0D, 0x0A };
        private static readonly byte[] TareCommand = { 0x06, (byte)'T', 0x0D, 0x0A };

        private const int WeightPollInterval = 500;
        private const int UnlockInterval = 30000;

        private readonly Control _uiOwner;
        private readonly AppSettings _settings;
        private IBoardBackend _board;
        private SerialPort _serialPort;
        private Timer _weightTimer;
        private Timer _unlockTimer;
        private int _bus;

        public event Action<string> Log;
        public event Action<WeightReading> WeightReceived;

        public ScaleConnection(Control uiOwner, AppSettings settings)
        {
            _uiOwner = uiOwner;
            _settings = settings;
        }

        public bool IsOpen { get { return _serialPort != null; } }

        public string BoardDescription { get { return _board != null ? _board.BoardDescription : ""; } }

        /// <summary>Throws on failure; everything opened so far is closed again.</summary>
        public void Open()
        {
            if (IsOpen)
                return;

            byte address = _settings.I2cAddress;
            if (I2cScanner.IsProtectedAddress(address))
                throw new BoardException(string.Format(
                    "Adres 0x{0:X2} is een EEPROM/klokgenerator, schrijven kan het board beschadigen", address));

            try
            {
                List<string> boardLog = new List<string>();
                try
                {
                    _board = BoardDetector.OpenBackend(_settings, boardLog);
                }
                finally
                {
                    foreach (string line in boardLog)
                        RaiseLog(line);
                }
                _bus = _settings.BusFor(_board.Kind);
                RaiseLog("Geopend: " + _board.BoardDescription);

                Unlock();

                _serialPort = new SerialPort(_settings.ComPort, 9600, Parity.Even, 7, StopBits.Two);
                _serialPort.RtsEnable = false;
                _serialPort.DtrEnable = false;
                _serialPort.Handshake = Handshake.None;
                _serialPort.NewLine = "\n";
                _serialPort.DataReceived += SerialPort_DataReceived;
                _serialPort.Open();
                RaiseLog("Seriele poort " + _settings.ComPort + " geopend");

                _serialPort.Write(InitCommand, 0, InitCommand.Length);

                _weightTimer = new Timer();
                _weightTimer.Interval = WeightPollInterval;
                _weightTimer.Tick += WeightTimer_Tick;
                _weightTimer.Start();

                _unlockTimer = new Timer();
                _unlockTimer.Interval = UnlockInterval;
                _unlockTimer.Tick += UnlockTimer_Tick;
                _unlockTimer.Start();
            }
            catch
            {
                Close();
                throw;
            }
        }

        public void Close()
        {
            if (_weightTimer != null)
            {
                _weightTimer.Stop();
                _weightTimer.Dispose();
                _weightTimer = null;
            }
            if (_unlockTimer != null)
            {
                _unlockTimer.Stop();
                _unlockTimer.Dispose();
                _unlockTimer = null;
            }
            if (_serialPort != null)
            {
                _serialPort.DataReceived -= SerialPort_DataReceived;
                try
                {
                    if (_serialPort.IsOpen)
                        _serialPort.Close();
                }
                catch (Exception ex)
                {
                    RaiseLog("Fout bij sluiten seriele poort: " + ex.Message);
                }
                _serialPort.Dispose();
                _serialPort = null;
                RaiseLog("Seriele poort gesloten");
            }
            if (_board != null)
            {
                _board.Dispose();
                _board = null;
                RaiseLog("Board gesloten");
            }
        }

        public void Zero()
        {
            Send(ZeroCommand, "Nulstellen");
        }

        public void Tare()
        {
            Send(TareCommand, "Tarreren");
        }

        private void Send(byte[] command, string description)
        {
            if (!IsOpen)
                return;
            _serialPort.Write(command, 0, command.Length);
            RaiseLog(description);
        }

        private void Unlock()
        {
            bool ok = LoadcellUnlocker.Unlock(_board, _bus, _settings.I2cAddress);
            RaiseLog(ok
                ? string.Format("Loadcell vrijgegeven via I2C bus {0}, adres 0x{1:X2}", _bus, _settings.I2cAddress)
                : string.Format("Waarschuwing: een of meer I2C-aanroepen mislukt op bus {0}, adres 0x{1:X2}", _bus, _settings.I2cAddress));
        }

        private void WeightTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                _serialPort.Write(WeightRequestCommand, 0, WeightRequestCommand.Length);
            }
            catch (Exception ex)
            {
                RaiseLog("Fout bij opvragen gewicht: " + ex.Message);
            }
        }

        private void UnlockTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                Unlock();
            }
            catch (Exception ex)
            {
                RaiseLog("Fout bij vrijgeven loadcell: " + ex.Message);
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string line;
            try
            {
                SerialPort port = (SerialPort)sender;
                line = port.ReadLine();
            }
            catch (Exception)
            {
                return;
            }

            try
            {
                _uiOwner.BeginInvoke(new Action<string>(HandleLine), line);
            }
            catch (InvalidOperationException)
            {
                // Form is closing
            }
        }

        private void HandleLine(string line)
        {
            // Strip control characters (ACK etc.)
            string clean = StripControlChars(line);
            if (clean.Length == 0)
                return;

            WeightReading reading;
            if (WeightParser.TryParse(clean, _settings.WeightDecimals, out reading))
            {
                if (WeightReceived != null)
                    WeightReceived(reading);
            }
            else
            {
                RaiseLog("Ontvangen: " + clean);
            }
        }

        private static string StripControlChars(string text)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(text.Length);
            foreach (char c in text)
            {
                if (c >= ' ')
                    sb.Append(c);
            }
            return sb.ToString().Trim();
        }

        private void RaiseLog(string message)
        {
            if (Log != null)
                Log(message);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
