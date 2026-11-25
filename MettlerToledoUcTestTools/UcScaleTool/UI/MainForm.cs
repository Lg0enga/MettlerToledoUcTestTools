using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Windows.Forms;
using UcScaleTool.Hardware;
using UcScaleTool.Printing;
using UcScaleTool.Scale;

namespace UcScaleTool.UI
{
    public class MainForm : Form
    {
        private readonly AppSettings _settings;
        private readonly ScaleConnection _scale;
        private WeightReading _lastReading;
        private DetectionResult _detection;
        private bool _busy;

        // Header / navigation
        private Label _headerStatus;
        private readonly List<Button> _navButtons = new List<Button>();
        private readonly List<Control> _pages = new List<Control>();
        private Panel _adminBanner;

        // Scale page
        private Label _weightValue;
        private Label _weightUnit;
        private Font _weightValueFont;
        private Font _weightUnitFont;
        private Label _stabilityLabel;
        private Label _tareGrossLabel;
        private Label _rawLabel;
        private Button _openCloseButton;
        private Button _zeroButton;
        private Button _tareButton;

        // Label section
        private TextBox _productInput;
        private TextBox _priceInput;
        private NumericUpDown _pluInput;
        private Label _totalLabel;
        private PictureBox _preview;
        private Button _printButton;
        private Button _feedButton;

        // Printer page
        private ComboBox _speedInput;
        private TrackBar _darknessInput;
        private Label _darknessValue;
        private ComboBox _modeInput;
        private ComboBox _offsetInput;
        private CheckBox _takeawayInput;
        private CheckBox _rewindInput;
        private CheckBox _takeupInput;
        private Button _readPrinterButton;
        private Button _applyPrinterButton;
        private Button _calibrateButton;
        private TextBox _printerInfo;
        private Control _printerSettingsTable;
        private PrinterConfig _printerConfig;
        private bool _printerReadOnce;

        // Hardware page
        private TextBox _detectionOutput;
        private Button _detectButton;
        private Button _applyDetectionButton;
        private ComboBox _backendInput;
        private ComboBox _boardNameInput;
        private NumericUpDown _jidaBusInput;
        private NumericUpDown _uspBusInput;
        private NumericUpDown _addressInput;
        private ComboBox _comPortInput;
        private NumericUpDown _decimalsInput;
        private ComboBox _printerProtocolInput;
        private Button _saveButton;
        private Button _scanButton;
        private Control _settingsTable;

        // Log page
        private ListBox _logList;

        // Status bar
        private ToolStripStatusLabel _statusBoard;
        private ToolStripStatusLabel _statusScale;
        private ToolStripStatusLabel _statusPrinter;

        public MainForm()
        {
            _settings = AppSettings.Load();
            _scale = new ScaleConnection(this, _settings);
            _scale.Log += AddLog;
            _scale.WeightReceived += OnWeightReceived;

            BuildUi();
            LoadSettingsIntoUi();
            UpdateScaleState();
            UpdatePreview();
        }

        //
        // Layout
        //

        private void BuildUi()
        {
            Text = "Mettler Toledo UC Scale Tool";
            BackColor = Theme.Background;
            Font = Theme.CreateFont(10f);
            MinimumSize = new Size(800, 580);
            Size = new Size(1024, 720);
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;

            Panel content = new Panel();
            content.Dock = DockStyle.Fill;
            content.Padding = new Padding(8);

            _pages.Add(BuildScalePage());
            _pages.Add(BuildPrinterPage());
            _pages.Add(BuildHardwarePage());
            _pages.Add(BuildLogPage());
            foreach (Control page in _pages)
            {
                page.Dock = DockStyle.Fill;
                content.Controls.Add(page);
            }

            // Dock order: added last = docked first
            Controls.Add(content);
            Controls.Add(BuildAdminBanner());
            Controls.Add(BuildNavigation());
            Controls.Add(BuildHeader());
            Controls.Add(BuildStatusBar());

            ShowPage(0);
        }

        private Control BuildHeader()
        {
            Panel header = new Panel();
            header.Dock = DockStyle.Top;
            header.Height = 56;
            header.BackColor = Theme.Header;
            header.Padding = new Padding(16, 0, 16, 0);

            Label title = new Label();
            title.Text = "UC Scale Tool";
            title.Font = Theme.CreateFont(17f, FontStyle.Bold);
            title.ForeColor = Color.White;
            title.Dock = DockStyle.Left;
            title.AutoSize = false;
            title.Width = 260;
            title.TextAlign = ContentAlignment.MiddleLeft;

            _headerStatus = new Label();
            _headerStatus.Font = Theme.CreateFont(11f);
            _headerStatus.ForeColor = Color.FromArgb(200, 210, 225);
            _headerStatus.Dock = DockStyle.Fill;
            _headerStatus.TextAlign = ContentAlignment.MiddleRight;
            _headerStatus.Text = "Hardware wordt gedetecteerd...";

            header.Controls.Add(_headerStatus);
            header.Controls.Add(title);
            return header;
        }

        private Control BuildNavigation()
        {
            Panel nav = new Panel();
            nav.Dock = DockStyle.Top;
            nav.Height = 50;
            nav.BackColor = Theme.NavBar;
            nav.Padding = new Padding(8, 0, 8, 0);

            string[] titles = { "Weegschaal", "Printer", "Hardware", "Log" };
            for (int i = titles.Length - 1; i >= 0; i--)
            {
                int index = i;
                Button button = new Button();
                button.Text = titles[i];
                button.Dock = DockStyle.Left;
                button.Width = 170;
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.FlatAppearance.MouseOverBackColor = Color.FromArgb(50, 72, 102);
                button.ForeColor = Color.White;
                button.Font = Theme.CreateFont(12f, FontStyle.Bold);
                button.Cursor = Cursors.Hand;
                button.Click += delegate { ShowPage(index); };
                nav.Controls.Add(button);
                _navButtons.Insert(0, button);
            }
            return nav;
        }

        private Control BuildAdminBanner()
        {
            _adminBanner = new Panel();
            _adminBanner.Dock = DockStyle.Top;
            _adminBanner.Height = 34;
            _adminBanner.BackColor = Theme.Warning;
            _adminBanner.Visible = !BoardDetector.IsAdministrator();

            Label text = new Label();
            text.Text = "  De tool draait niet als administrator. De loadcell en printer werken dan meestal niet.";
            text.Dock = DockStyle.Fill;
            text.ForeColor = Color.White;
            text.Font = Theme.CreateFont(10.5f, FontStyle.Bold);
            text.TextAlign = ContentAlignment.MiddleLeft;
            _adminBanner.Controls.Add(text);
            return _adminBanner;
        }

        private Control BuildStatusBar()
        {
            StatusStrip strip = new StatusStrip();
            strip.SizingGrip = false;
            strip.BackColor = Color.White;
            strip.Font = Theme.CreateFont(9.5f);

            _statusBoard = new ToolStripStatusLabel("Board: -");
            _statusScale = new ToolStripStatusLabel("Weegschaal: gesloten");
            _statusPrinter = new ToolStripStatusLabel("Printer: -");
            _statusBoard.Spring = true;
            _statusBoard.TextAlign = ContentAlignment.MiddleLeft;
            _statusScale.BorderSides = ToolStripStatusLabelBorderSides.Left;
            _statusPrinter.BorderSides = ToolStripStatusLabelBorderSides.Left;

            strip.Items.Add(_statusBoard);
            strip.Items.Add(_statusScale);
            strip.Items.Add(_statusPrinter);
            return strip;
        }

        private void ShowPage(int index)
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                _pages[i].Visible = i == index;
                _navButtons[i].BackColor = i == index ? Theme.NavSelected : Theme.NavBar;
            }

            // Read the printer settings the first time the printer page is opened
            if (index == PrinterPageIndex && !_printerReadOnce && IsHandleCreated)
                ReadPrinterSettings();
        }

        private const int PrinterPageIndex = 1;

        private Control BuildScalePage()
        {
            TableLayoutPanel page = new TableLayoutPanel();
            page.ColumnCount = 2;
            page.RowCount = 1;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 56f));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 44f));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            page.Controls.Add(BuildWeightCard(), 0, 0);
            page.Controls.Add(BuildLabelCard(), 1, 0);
            return page;
        }

        private Control BuildWeightCard()
        {
            CardPanel card = Theme.CreateCard("Gewicht");

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 1;
            layout.RowCount = 4;
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 84f));

            // Display
            Panel display = new Panel();
            display.Dock = DockStyle.Fill;
            display.BackColor = Theme.Display;
            display.Padding = new Padding(20, 12, 20, 12);
            display.Margin = new Padding(0, 0, 0, 8);

            Label caption = new Label();
            caption.Text = "NETTO";
            caption.Dock = DockStyle.Top;
            caption.Height = 28;
            caption.ForeColor = Color.FromArgb(140, 160, 185);
            caption.Font = Theme.CreateFont(12f, FontStyle.Bold);

            _stabilityLabel = new Label();
            _stabilityLabel.Dock = DockStyle.Bottom;
            _stabilityLabel.Height = 30;
            _stabilityLabel.Font = Theme.CreateFont(12f, FontStyle.Bold);
            _stabilityLabel.ForeColor = Color.FromArgb(140, 160, 185);
            _stabilityLabel.Text = "● Geen verbinding";

            _weightUnit = new Label();
            _weightUnit.Dock = DockStyle.Right;
            _weightUnit.Width = 90;
            _weightUnit.ForeColor = Theme.DisplayText;
            _weightUnit.TextAlign = ContentAlignment.MiddleLeft;
            _weightUnit.Text = "kg";

            _weightValue = new Label();
            _weightValue.Dock = DockStyle.Fill;
            _weightValue.ForeColor = Theme.DisplayText;
            _weightValue.TextAlign = ContentAlignment.MiddleRight;
            _weightValue.Text = "-.---";

            display.Controls.Add(_weightValue);
            display.Controls.Add(_weightUnit);
            display.Controls.Add(_stabilityLabel);
            display.Controls.Add(caption);
            display.Resize += delegate { ScaleWeightFonts(display); };
            ScaleWeightFonts(display);

            _tareGrossLabel = new Label();
            _tareGrossLabel.Dock = DockStyle.Fill;
            _tareGrossLabel.Font = Theme.CreateFont(14f);
            _tareGrossLabel.ForeColor = Theme.Text;
            _tareGrossLabel.TextAlign = ContentAlignment.MiddleLeft;
            _tareGrossLabel.Text = "Tarra: -      Bruto: -";

            _rawLabel = new Label();
            _rawLabel.Dock = DockStyle.Fill;
            _rawLabel.Font = Theme.CreateMonoFont(9f);
            _rawLabel.ForeColor = Theme.MutedText;
            _rawLabel.TextAlign = ContentAlignment.MiddleLeft;
            _rawLabel.AutoEllipsis = true;

            TableLayoutPanel buttons = new TableLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.ColumnCount = 3;
            buttons.RowCount = 1;
            buttons.Margin = new Padding(0);
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30f));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _openCloseButton = Theme.CreateButton("Weegschaal openen", Theme.Primary, 13f);
            _openCloseButton.Click += OpenCloseButton_Click;
            _zeroButton = Theme.CreateButton("→ 0 ←  Nul", Theme.Neutral, 13f);
            _zeroButton.Click += delegate { RunSafe(_scale.Zero); };
            _tareButton = Theme.CreateButton("T  Tarra", Theme.Neutral, 13f);
            _tareButton.Click += delegate { RunSafe(_scale.Tare); };

            buttons.Controls.Add(_openCloseButton, 0, 0);
            buttons.Controls.Add(_zeroButton, 1, 0);
            buttons.Controls.Add(_tareButton, 2, 0);

            layout.Controls.Add(display, 0, 0);
            layout.Controls.Add(_tareGrossLabel, 0, 1);
            layout.Controls.Add(_rawLabel, 0, 2);
            layout.Controls.Add(buttons, 0, 3);
            card.Body.Controls.Add(layout);
            return card;
        }

        /// <summary>
        /// Sizes the weight digits to the display, so it reads well on 800x600 and on
        /// larger screens.
        /// </summary>
        private void ScaleWeightFonts(Control display)
        {
            int width = display.ClientSize.Width - display.Padding.Horizontal;
            int height = display.ClientSize.Height - display.Padding.Vertical - 58;
            if (width <= 0 || height <= 0)
                return;

            // Room for "-12.345" in the value plus the unit next to it
            float pixels = Math.Max(16f, Math.Min(height * 0.8f, width * 0.15f));
            float points = pixels * 0.75f;

            // Only dispose fonts created here, never the inherited (shared) default font
            Font oldValue = _weightValueFont;
            Font oldUnit = _weightUnitFont;
            _weightValueFont = new Font("Arial", points, FontStyle.Bold);
            _weightUnitFont = new Font("Arial", points * 0.45f, FontStyle.Bold);
            _weightValue.Font = _weightValueFont;
            _weightUnit.Font = _weightUnitFont;
            _weightUnit.Width = (int)(pixels * 0.45f * 1.6f) + 12;
            if (oldValue != null)
                oldValue.Dispose();
            if (oldUnit != null)
                oldUnit.Dispose();
        }

        private Control BuildLabelCard()
        {
            CardPanel card = Theme.CreateCard("Label printen");

            TableLayoutPanel layout = new TableLayoutPanel();
            layout.Dock = DockStyle.Fill;
            layout.ColumnCount = 2;
            layout.RowCount = 6;
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110f));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30f));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 76f));

            _productInput = new TextBox();
            _productInput.Dock = DockStyle.Fill;
            _productInput.Font = Theme.CreateFont(12f);
            _productInput.TextChanged += delegate { UpdatePreview(); };

            _priceInput = new TextBox();
            _priceInput.Dock = DockStyle.Fill;
            _priceInput.Font = Theme.CreateFont(12f);
            _priceInput.TextChanged += delegate { UpdatePreview(); };

            _pluInput = new NumericUpDown();
            _pluInput.Font = Theme.CreateFont(12f);
            _pluInput.Minimum = 0;
            _pluInput.Maximum = 99999;
            _pluInput.Width = 110;
            _pluInput.ValueChanged += delegate { UpdatePreview(); };

            _totalLabel = new Label();
            _totalLabel.Dock = DockStyle.Fill;
            _totalLabel.Font = Theme.CreateFont(11f, FontStyle.Bold);
            _totalLabel.ForeColor = Theme.Primary;
            _totalLabel.TextAlign = ContentAlignment.MiddleLeft;

            _preview = new PictureBox();
            _preview.Dock = DockStyle.Fill;
            _preview.SizeMode = PictureBoxSizeMode.Zoom;
            _preview.BackColor = Color.FromArgb(246, 247, 249);
            _preview.BorderStyle = BorderStyle.FixedSingle;
            _preview.Margin = new Padding(0, 6, 0, 6);

            TableLayoutPanel buttons = new TableLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.ColumnCount = 2;
            buttons.RowCount = 1;
            buttons.Margin = new Padding(0);
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38f));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _printButton = Theme.CreateButton("Print label", Theme.Success, 13f);
            _printButton.Click += PrintButton_Click;
            _feedButton = Theme.CreateButton("Doorvoer", Theme.Neutral, 12f);
            _feedButton.Click += FeedButton_Click;
            buttons.Controls.Add(_printButton, 0, 0);
            buttons.Controls.Add(_feedButton, 1, 0);

            layout.Controls.Add(Theme.CreateFieldLabel("Product"), 0, 0);
            layout.Controls.Add(_productInput, 1, 0);
            layout.Controls.Add(Theme.CreateFieldLabel("Prijs per kg"), 0, 1);
            layout.Controls.Add(_priceInput, 1, 1);
            layout.Controls.Add(Theme.CreateFieldLabel("PLU"), 0, 2);
            layout.Controls.Add(_pluInput, 1, 2);
            layout.Controls.Add(_totalLabel, 0, 3);
            layout.SetColumnSpan(_totalLabel, 2);
            layout.Controls.Add(_preview, 0, 4);
            layout.SetColumnSpan(_preview, 2);
            layout.Controls.Add(buttons, 0, 5);
            layout.SetColumnSpan(buttons, 2);

            card.Body.Controls.Add(layout);
            return card;
        }

        private Control BuildPrinterPage()
        {
            TableLayoutPanel page = new TableLayoutPanel();
            page.ColumnCount = 2;
            page.RowCount = 1;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Settings card
            CardPanel settingsCard = Theme.CreateCard("Printerinstellingen");
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 2;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _printerSettingsTable = grid;

            _speedInput = CreateCombo(ComboBoxStyle.DropDownList);
            _speedInput.Items.AddRange(PrinterSettingsService.SpeedNames);

            Panel darknessPanel = new Panel();
            darknessPanel.Dock = DockStyle.Fill;
            darknessPanel.Margin = new Padding(0);
            _darknessValue = new Label();
            _darknessValue.Dock = DockStyle.Right;
            _darknessValue.Width = 40;
            _darknessValue.Font = Theme.CreateFont(13f, FontStyle.Bold);
            _darknessValue.TextAlign = ContentAlignment.MiddleCenter;
            _darknessValue.Text = "-";
            _darknessInput = new TrackBar();
            _darknessInput.Dock = DockStyle.Fill;
            _darknessInput.Minimum = 0;
            _darknessInput.Maximum = PrinterSettingsService.MaxDarkness;
            _darknessInput.TickStyle = TickStyle.BottomRight;
            _darknessInput.LargeChange = 1;
            _darknessInput.ValueChanged += delegate { _darknessValue.Text = _darknessInput.Value.ToString(); };
            darknessPanel.Controls.Add(_darknessInput);
            darknessPanel.Controls.Add(_darknessValue);

            _modeInput = CreateCombo(ComboBoxStyle.DropDownList);
            _modeInput.Items.AddRange(PrinterSettingsService.ModeNames);

            _offsetInput = CreateCombo(ComboBoxStyle.DropDownList);
            for (int i = 0; i <= PrinterSettingsService.MaxOffsetStep; i++)
                _offsetInput.Items.Add(i.ToString());

            _takeawayInput = new CheckBox();
            // Label column says what it is; keep the text short for 800x600
            _takeawayInput.Text = "Aan";
            _takeawayInput.Font = Theme.CreateFont(11f);
            _takeawayInput.Dock = DockStyle.Fill;

            _rewindInput = new CheckBox();
            _rewindInput.Text = "Rewind";
            _rewindInput.Font = Theme.CreateFont(11f);
            _rewindInput.AutoSize = true;
            _rewindInput.Margin = new Padding(3, 8, 24, 3);

            _takeupInput = new CheckBox();
            _takeupInput.Text = "Takeup";
            _takeupInput.Font = Theme.CreateFont(11f);
            _takeupInput.AutoSize = true;
            _takeupInput.Margin = new Padding(3, 8, 3, 3);

            // Both motors on one row so the page still fits on 800x600
            FlowLayoutPanel motorPanel = new FlowLayoutPanel();
            motorPanel.Dock = DockStyle.Fill;
            motorPanel.Margin = new Padding(0);
            motorPanel.WrapContents = false;
            motorPanel.Controls.Add(_rewindInput);
            motorPanel.Controls.Add(_takeupInput);

            AddSettingRow(grid, "Printsnelheid", _speedInput);
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 46f));
            grid.Controls.Add(Theme.CreateFieldLabel("Donkerte"), 0, grid.RowCount);
            grid.Controls.Add(darknessPanel, 1, grid.RowCount);
            grid.RowCount++;
            AddSettingRow(grid, "Label of bon", _modeInput);
            AddSettingRow(grid, "Offset", _offsetInput);
            AddSettingRow(grid, "Wegneemsensor", _takeawayInput);
            AddSettingRow(grid, "Motoren", motorPanel);

            TableLayoutPanel buttons = new TableLayoutPanel();
            buttons.Dock = DockStyle.Fill;
            buttons.ColumnCount = 2;
            buttons.RowCount = 1;
            buttons.Margin = new Padding(0);
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _readPrinterButton = Theme.CreateButton("Uitlezen", Theme.Neutral);
            _readPrinterButton.Click += delegate { ReadPrinterSettings(); };
            _applyPrinterButton = Theme.CreateButton("Toepassen", Theme.Primary);
            _applyPrinterButton.Click += delegate { ApplyPrinterSettings(); };
            buttons.Controls.Add(_readPrinterButton, 0, 0);
            buttons.Controls.Add(_applyPrinterButton, 1, 0);

            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 58f));
            grid.Controls.Add(buttons, 0, grid.RowCount);
            grid.SetColumnSpan(buttons, 2);
            grid.RowCount++;
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.RowCount++;
            settingsCard.Body.Controls.Add(grid);

            // Calibration / info card
            CardPanel infoCard = Theme.CreateCard("Kalibreren");
            Label explanation = new Label();
            explanation.Dock = DockStyle.Top;
            explanation.Height = 120;
            explanation.Font = Theme.CreateFont(10.5f);
            explanation.ForeColor = Theme.Text;
            explanation.Text = "Kalibreer na het plaatsen van een nieuwe rol of een ander labelformaat. "
                + "De printer voert een paar labels door om de labellengte te meten. "
                + "Wegneemsensor aan = de printer wacht tot het label is weggenomen.";

            Panel calibratePanel = new Panel();
            calibratePanel.Dock = DockStyle.Top;
            calibratePanel.Height = 70;
            _calibrateButton = Theme.CreateButton("Printer kalibreren", Theme.Warning);
            _calibrateButton.Click += delegate { CalibratePrinter(); };
            calibratePanel.Controls.Add(_calibrateButton);

            Label rawCaption = new Label();
            rawCaption.Dock = DockStyle.Top;
            rawCaption.Height = 34;
            rawCaption.Font = Theme.CreateFont(10.5f, FontStyle.Bold);
            rawCaption.TextAlign = ContentAlignment.BottomLeft;
            rawCaption.Text = "Printerantwoord";

            _printerInfo = new TextBox();
            _printerInfo.Dock = DockStyle.Fill;
            _printerInfo.Multiline = true;
            _printerInfo.ReadOnly = true;
            _printerInfo.ScrollBars = ScrollBars.Vertical;
            _printerInfo.BackColor = Color.FromArgb(246, 247, 249);
            _printerInfo.Font = Theme.CreateMonoFont(9.5f);
            _printerInfo.Text = "Nog niet uitgelezen.";

            infoCard.Body.Controls.Add(_printerInfo);
            infoCard.Body.Controls.Add(rawCaption);
            infoCard.Body.Controls.Add(calibratePanel);
            infoCard.Body.Controls.Add(explanation);

            page.Controls.Add(settingsCard, 0, 0);
            page.Controls.Add(infoCard, 1, 0);
            SetPrinterInputsEnabled();
            return page;
        }

        private Control BuildHardwarePage()
        {
            TableLayoutPanel page = new TableLayoutPanel();
            page.ColumnCount = 2;
            page.RowCount = 1;
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52f));
            page.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48f));
            page.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Detection card
            CardPanel detectCard = Theme.CreateCard("Detectie");
            TableLayoutPanel detectLayout = new TableLayoutPanel();
            detectLayout.Dock = DockStyle.Fill;
            detectLayout.ColumnCount = 2;
            detectLayout.RowCount = 2;
            detectLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            detectLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            detectLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
            detectLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _detectButton = Theme.CreateButton("Detecteer hardware", Theme.Primary);
            _detectButton.Click += delegate { RunDetection(); };
            _applyDetectionButton = Theme.CreateButton("Gebruik resultaat", Theme.Neutral);
            _applyDetectionButton.Enabled = false;
            _applyDetectionButton.Click += ApplyDetectionButton_Click;

            _detectionOutput = new TextBox();
            _detectionOutput.Dock = DockStyle.Fill;
            _detectionOutput.Multiline = true;
            _detectionOutput.ReadOnly = true;
            _detectionOutput.ScrollBars = ScrollBars.Vertical;
            _detectionOutput.BackColor = Color.FromArgb(246, 247, 249);
            _detectionOutput.Font = Theme.CreateMonoFont(10f);
            _detectionOutput.Margin = new Padding(6);

            detectLayout.Controls.Add(_detectButton, 0, 0);
            detectLayout.Controls.Add(_applyDetectionButton, 1, 0);
            detectLayout.Controls.Add(_detectionOutput, 0, 1);
            detectLayout.SetColumnSpan(_detectionOutput, 2);
            detectCard.Body.Controls.Add(detectLayout);

            // Settings card
            CardPanel settingsCard = Theme.CreateCard("Instellingen");
            TableLayoutPanel grid = new TableLayoutPanel();
            grid.Dock = DockStyle.Fill;
            grid.ColumnCount = 2;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _settingsTable = grid;

            _backendInput = CreateCombo(ComboBoxStyle.DropDownList);
            _backendInput.Items.AddRange(new object[] { "Automatisch", "JIDA (jida.dll)", "UspEpc (UspEpc.dll)" });

            _boardNameInput = CreateCombo(ComboBoxStyle.DropDown);

            _jidaBusInput = CreateNumeric(0, 15, false);
            _uspBusInput = CreateNumeric(0, 15, false);
            _addressInput = CreateNumeric(0, 255, true);

            _comPortInput = CreateCombo(ComboBoxStyle.DropDown);
            List<string> ports = new List<string>(SerialPort.GetPortNames());
            if (!ports.Contains("COM2"))
                ports.Insert(0, "COM2");
            ports.Sort();
            _comPortInput.Items.AddRange(ports.ToArray());

            _decimalsInput = CreateNumeric(0, 4, false);

            _printerProtocolInput = CreateCombo(ComboBoxStyle.DropDownList);
            _printerProtocolInput.Items.AddRange(new object[] { "Standaard (XP-versie)", "Oud (eerste versie)" });

            AddSettingRow(grid, "Board library", _backendInput);
            AddSettingRow(grid, "JIDA board name", _boardNameInput);
            AddSettingRow(grid, "I2C bus (JIDA)", _jidaBusInput);
            AddSettingRow(grid, "I2C bus (UspEpc)", _uspBusInput);
            AddSettingRow(grid, "I2C adres (hex)", _addressInput);
            AddSettingRow(grid, "COM-poort loadcell", _comPortInput);
            AddSettingRow(grid, "Decimalen zonder punt", _decimalsInput);
            AddSettingRow(grid, "Printerprotocol", _printerProtocolInput);

            Label hint = new Label();
            hint.Text = "Board name leeg = eerste board. Standaard: JIDA bus 0, UspEpc bus 1, adres C0.";
            hint.Dock = DockStyle.Fill;
            hint.ForeColor = Theme.MutedText;
            hint.Font = Theme.CreateFont(9.5f);
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 40f));
            grid.Controls.Add(hint, 0, grid.RowCount);
            grid.SetColumnSpan(hint, 2);
            grid.RowCount++;

            TableLayoutPanel settingButtons = new TableLayoutPanel();
            settingButtons.Dock = DockStyle.Fill;
            settingButtons.ColumnCount = 2;
            settingButtons.RowCount = 1;
            settingButtons.Margin = new Padding(0);
            settingButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            settingButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            settingButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            _saveButton = Theme.CreateButton("Opslaan", Theme.Primary);
            _saveButton.Click += delegate { SaveSettings(); };
            _scanButton = Theme.CreateButton("Scan I2C", Theme.Neutral);
            _scanButton.Click += delegate { RunScan(); };
            settingButtons.Controls.Add(_saveButton, 0, 0);
            settingButtons.Controls.Add(_scanButton, 1, 0);

            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 64f));
            grid.Controls.Add(settingButtons, 0, grid.RowCount);
            grid.SetColumnSpan(settingButtons, 2);
            grid.RowCount++;
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));
            grid.RowCount++;

            settingsCard.Body.Controls.Add(grid);

            page.Controls.Add(detectCard, 0, 0);
            page.Controls.Add(settingsCard, 1, 0);
            return page;
        }

        private Control BuildLogPage()
        {
            CardPanel card = Theme.CreateCard("Log");

            _logList = new ListBox();
            _logList.Dock = DockStyle.Fill;
            _logList.Font = Theme.CreateMonoFont(10f);
            _logList.BorderStyle = BorderStyle.FixedSingle;
            _logList.IntegralHeight = false;
            _logList.HorizontalScrollbar = true;

            TableLayoutPanel buttons = new TableLayoutPanel();
            buttons.Dock = DockStyle.Bottom;
            buttons.Height = 64;
            buttons.ColumnCount = 3;
            buttons.RowCount = 1;
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
            buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            buttons.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            Button copy = Theme.CreateButton("Kopieer log", Theme.Neutral);
            copy.Click += CopyLogButton_Click;
            Button clear = Theme.CreateButton("Wissen", Theme.Neutral);
            clear.Click += delegate { _logList.Items.Clear(); };
            buttons.Controls.Add(copy, 0, 0);
            buttons.Controls.Add(clear, 1, 0);

            card.Body.Controls.Add(_logList);
            card.Body.Controls.Add(buttons);
            return card;
        }

        private static ComboBox CreateCombo(ComboBoxStyle style)
        {
            ComboBox combo = new ComboBox();
            combo.DropDownStyle = style;
            combo.Font = Theme.CreateFont(11f);
            combo.Dock = DockStyle.Fill;
            combo.Margin = new Padding(3, 5, 3, 3);
            return combo;
        }

        private static NumericUpDown CreateNumeric(int min, int max, bool hex)
        {
            NumericUpDown numeric = new NumericUpDown();
            numeric.Minimum = min;
            numeric.Maximum = max;
            numeric.Hexadecimal = hex;
            numeric.Font = Theme.CreateFont(11f);
            numeric.Width = 90;
            numeric.Margin = new Padding(3, 5, 3, 3);
            return numeric;
        }

        private static void AddSettingRow(TableLayoutPanel grid, string label, Control input)
        {
            grid.RowStyles.Add(new RowStyle(SizeType.Absolute, 38f));
            grid.Controls.Add(Theme.CreateFieldLabel(label), 0, grid.RowCount);
            grid.Controls.Add(input, 1, grid.RowCount);
            grid.RowCount++;
        }

        //
        // Settings
        //

        private void LoadSettingsIntoUi()
        {
            _backendInput.SelectedIndex = (int)_settings.Backend;
            _boardNameInput.Text = _settings.JidaBoardName;
            _jidaBusInput.Value = Clamp(_settings.JidaBus, _jidaBusInput);
            _uspBusInput.Value = Clamp(_settings.UspEpcBus, _uspBusInput);
            _addressInput.Value = _settings.I2cAddress;
            _comPortInput.Text = _settings.ComPort;
            _decimalsInput.Value = Clamp(_settings.WeightDecimals, _decimalsInput);
            _printerProtocolInput.SelectedIndex = (int)_settings.PrinterProtocol;
            _productInput.Text = _settings.ProductName;
            _priceInput.Text = _settings.PricePerKg;
            _pluInput.Value = Clamp(_settings.Plu, _pluInput);
        }

        private static decimal Clamp(int value, NumericUpDown input)
        {
            return Math.Max(input.Minimum, Math.Min(input.Maximum, value));
        }

        private void ReadSettingsFromUi()
        {
            _settings.Backend = (BackendKind)Math.Max(0, _backendInput.SelectedIndex);
            _settings.JidaBoardName = _boardNameInput.Text.Trim();
            _settings.JidaBus = (int)_jidaBusInput.Value;
            _settings.UspEpcBus = (int)_uspBusInput.Value;
            _settings.I2cAddress = (byte)_addressInput.Value;
            _settings.ComPort = _comPortInput.Text.Trim().Length > 0 ? _comPortInput.Text.Trim() : "COM2";
            _settings.WeightDecimals = (int)_decimalsInput.Value;
            _settings.PrinterProtocol = (PrinterProtocol)Math.Max(0, _printerProtocolInput.SelectedIndex);
            _settings.ProductName = _productInput.Text;
            _settings.PricePerKg = _priceInput.Text.Trim();
            _settings.Plu = (int)_pluInput.Value;
        }

        private void SaveSettings()
        {
            ReadSettingsFromUi();
            try
            {
                _settings.Save();
                AddLog("Instellingen opgeslagen in " + AppSettings.FilePath);
            }
            catch (Exception ex)
            {
                AddLog("Opslaan mislukt: " + ex.Message);
                MessageBox.Show(this, "Opslaan mislukt: " + ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        //
        // Detection & scan
        //

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            AddLog("UC Scale Tool gestart, instellingen: " + AppSettings.FilePath);
            RunDetection();
        }

        private void RunDetection()
        {
            if (_busy)
                return;
            _detectionOutput.Text = "Bezig met detecteren...";
            RunInBackground(
                delegate { return BoardDetector.Detect(); },
                delegate(object result)
                {
                    _detection = (DetectionResult)result;
                    _detectionOutput.Text = string.Join(Environment.NewLine, _detection.Lines.ToArray());
                    _applyDetectionButton.Enabled = _detection.Found && !_scale.IsOpen;
                    _boardNameInput.Items.Clear();
                    _boardNameInput.Items.AddRange(_detection.JidaBoardNames.ToArray());
                    AddLog("Detectie: " + _detection.Summary);
                    UpdateHeader();
                });
        }

        private void ApplyDetectionButton_Click(object sender, EventArgs e)
        {
            if (_detection == null || !_detection.Found)
                return;
            _backendInput.SelectedIndex = (int)_detection.Recommended;
            if (_detection.Recommended == BackendKind.Jida && _detection.JidaBoardNames.Count > 0)
                _boardNameInput.Text = _detection.JidaBoardNames[0];
            AddLog("Detectieresultaat overgenomen: " + _detection.Summary + " (nog niet opgeslagen)");
        }

        private void RunScan()
        {
            if (_busy || _scale.IsOpen)
                return;
            ReadSettingsFromUi();
            _detectionOutput.Text = "I2C scan bezig (alleen lezen)...";
            RunInBackground(
                delegate
                {
                    List<string> lines = new List<string>();
                    using (IBoardBackend board = BoardDetector.OpenBackend(_settings, lines))
                        lines.AddRange(I2cScanner.Scan(board));
                    return lines;
                },
                delegate(object result)
                {
                    List<string> lines = (List<string>)result;
                    _detectionOutput.Text = string.Join(Environment.NewLine, lines.ToArray());
                    foreach (string line in lines)
                        AddLog(line);
                });
        }

        //
        // Scale
        //

        private void OpenCloseButton_Click(object sender, EventArgs e)
        {
            if (_scale.IsOpen)
            {
                _scale.Close();
                _lastReading = null;
            }
            else
            {
                ReadSettingsFromUi();
                Cursor = Cursors.WaitCursor;
                try
                {
                    _scale.Open();
                    // Remember settings that work on this scale
                    _settings.Save();
                }
                catch (Exception ex)
                {
                    AddLog("Openen mislukt: " + ex.Message);
                    MessageBox.Show(this, "Weegschaal openen mislukt:\n\n" + ex.Message, Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
            UpdateScaleState();
        }

        private void OnWeightReceived(WeightReading reading)
        {
            _lastReading = reading;
            _weightValue.Text = FormatWeight(reading.Net, reading.Unit);
            _weightUnit.Text = reading.Unit;
            _stabilityLabel.Text = reading.Dynamic ? "● In beweging" : "● Stabiel";
            _stabilityLabel.ForeColor = reading.Dynamic ? Theme.Warning : Theme.DisplayText;
            _tareGrossLabel.Text = string.Format("Tarra: {0}      Bruto: {1}",
                reading.Tare.HasValue ? FormatWeight(reading.Tare.Value, reading.Unit) + " " + reading.Unit : "-",
                reading.Gross.HasValue ? FormatWeight(reading.Gross.Value, reading.Unit) + " " + reading.Unit : "-");
            _rawLabel.Text = reading.Raw;
            UpdatePreview();
        }

        private static string FormatWeight(decimal value, string unit)
        {
            string format = unit.ToLowerInvariant() == "g" ? "0" : "0.000";
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        private void UpdateScaleState()
        {
            bool open = _scale.IsOpen;
            _openCloseButton.Text = open ? "Weegschaal sluiten" : "Weegschaal openen";
            Theme.SetButtonColor(_openCloseButton, open ? Theme.Danger : Theme.Primary);
            _zeroButton.Enabled = open;
            _tareButton.Enabled = open;
            _settingsTable.Enabled = !open && !_busy;
            _applyDetectionButton.Enabled = !open && _detection != null && _detection.Found;
            _statusScale.Text = open ? "Weegschaal: open" : "Weegschaal: gesloten";

            if (!open)
            {
                _weightValue.Text = "-.---";
                _stabilityLabel.Text = "● Geen verbinding";
                _stabilityLabel.ForeColor = Color.FromArgb(140, 160, 185);
                _tareGrossLabel.Text = "Tarra: -      Bruto: -";
                _rawLabel.Text = "";
            }
            UpdateHeader();
        }

        private void UpdateHeader()
        {
            string detected = _detection != null ? _detection.Summary : "detectie bezig...";
            if (_scale.IsOpen)
            {
                _headerStatus.Text = "● Verbonden  |  " + _scale.BoardDescription;
                _statusBoard.Text = "Board: " + _scale.BoardDescription;
            }
            else
            {
                _headerStatus.Text = "Gedetecteerd: " + detected;
                _statusBoard.Text = "Board: " + detected;
            }
        }

        //
        // Printing
        //

        private LabelData CreateLabelData()
        {
            LabelData data = new LabelData();
            data.ProductName = _productInput.Text.Trim();
            data.Plu = (int)_pluInput.Value;
            data.Timestamp = DateTime.Now;

            if (_lastReading != null)
            {
                data.WeightKg = _lastReading.NetInKg;
                data.WeightText = FormatWeight(_lastReading.Net, _lastReading.Unit) + " " + _lastReading.Unit;
            }
            else
            {
                data.WeightKg = 0m;
                data.WeightText = "0.000 kg";
            }

            decimal price;
            string priceText = _priceInput.Text.Trim().Replace("€", "").Replace(',', '.').Trim();
            if (priceText.Length > 0 && decimal.TryParse(priceText, NumberStyles.Number, CultureInfo.InvariantCulture, out price))
                data.PricePerKg = price;
            return data;
        }

        private void UpdatePreview()
        {
            if (_preview == null)
                return;
            LabelData data = CreateLabelData();
            decimal? total = data.TotalPrice;
            _totalLabel.Text = total.HasValue
                ? "Te betalen: " + LabelRenderer.FormatMoney(total.Value)
                : "Geen prijs: barcode bevat het gewicht";

            Image old = _preview.Image;
            _preview.Image = LabelRenderer.Render(data);
            if (old != null)
                old.Dispose();
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            if (_busy)
                return;
            ReadSettingsFromUi();
            LabelData data = CreateLabelData();
            PrinterProtocol protocol = _settings.PrinterProtocol;
            _statusPrinter.Text = "Printer: bezig...";
            AddLog(string.Format("Label printen: {0}, {1}", data.WeightText, protocol));

            RunInBackground(
                delegate
                {
                    using (Bitmap label = LabelRenderer.Render(data))
                        return LabelPrinter.Print(label, protocol, ReportPrinterProgress);
                },
                delegate(object result)
                {
                    PrinterStatus status = (PrinterStatus)result;
                    string text;
                    switch (status)
                    {
                        case PrinterStatus.Normal: text = "label geprint"; break;
                        case PrinterStatus.RemoveLabel: text = "vorige label niet weggehaald"; break;
                        case PrinterStatus.EndOfRoll: text = "rol is leeg"; break;
                        default: text = "fout"; break;
                    }
                    _statusPrinter.Text = "Printer: " + text;
                    AddLog("Printer: " + text);
                });
        }

        private void FeedButton_Click(object sender, EventArgs e)
        {
            if (_busy)
                return;
            _statusPrinter.Text = "Printer: doorvoer...";
            RunInBackground(
                delegate
                {
                    LabelPrinter.Feed(ReportPrinterProgress);
                    return null;
                },
                delegate { _statusPrinter.Text = "Printer: label doorgevoerd"; });
        }

        private void ReportPrinterProgress(string message)
        {
            BeginInvoke(new Action<string>(delegate(string m)
            {
                _statusPrinter.Text = "Printer: " + m;
                AddLog("Printer: " + m);
            }), message);
        }

        //
        // Printer settings
        //

        private void ReadPrinterSettings()
        {
            if (_busy)
                return;
            _printerReadOnce = true;
            _printerInfo.Text = "Printerinstellingen uitlezen...";
            RunInBackground(
                delegate { return PrinterSettingsService.Read(); },
                delegate(object result)
                {
                    _printerConfig = (PrinterConfig)result;
                    ShowPrinterConfig(_printerConfig);
                    _printerInfo.Text = "Uitgelezen om " + DateTime.Now.ToString("HH:mm:ss")
                        + Environment.NewLine + Environment.NewLine + _printerConfig.RawHex;
                    AddLog(string.Format("Printer: snelheid {0}, donkerte {1}, modus {2}, offset {3}, wegneemsensor {4}, rewind motor {5}, takeup motor {6}",
                        PrinterSettingsService.SpeedNames[_printerConfig.Speed], _printerConfig.Darkness,
                        PrinterSettingsService.ModeNames[_printerConfig.Mode], _printerConfig.OffsetStep,
                        _printerConfig.TakeawaySensor ? "aan" : "uit",
                        _printerConfig.RewindMotor ? "aan" : "uit",
                        _printerConfig.TakeupMotor ? "aan" : "uit"));
                    SetPrinterInputsEnabled();
                });
        }

        private void ShowPrinterConfig(PrinterConfig config)
        {
            _speedInput.SelectedIndex = config.Speed;
            _darknessInput.Value = config.Darkness;
            _darknessValue.Text = config.Darkness.ToString();
            _modeInput.SelectedIndex = config.Mode;
            _offsetInput.SelectedIndex = config.OffsetStep;
            _takeawayInput.Checked = config.TakeawaySensor;
            _rewindInput.Checked = config.RewindMotor;
            _takeupInput.Checked = config.TakeupMotor;
        }

        private void ApplyPrinterSettings()
        {
            if (_busy || _printerConfig == null)
                return;

            PrinterConfig current = _printerConfig;
            PrinterConfig wanted = current.Clone();
            wanted.Speed = Math.Max(0, _speedInput.SelectedIndex);
            wanted.Darkness = _darknessInput.Value;
            wanted.Mode = Math.Max(0, _modeInput.SelectedIndex);
            wanted.OffsetStep = Math.Max(0, _offsetInput.SelectedIndex);
            wanted.TakeawaySensor = _takeawayInput.Checked;
            wanted.RewindMotor = _rewindInput.Checked;
            wanted.TakeupMotor = _takeupInput.Checked;

            RunInBackground(
                delegate { return PrinterSettingsService.Apply(current, wanted); },
                delegate(object result)
                {
                    List<string> changes = (List<string>)result;
                    if (changes.Count == 0)
                    {
                        AddLog("Printer: geen wijzigingen");
                        _statusPrinter.Text = "Printer: geen wijzigingen";
                        return;
                    }
                    AddLog("Printer ingesteld: " + string.Join(", ", changes.ToArray()));
                    _statusPrinter.Text = "Printer: instellingen toegepast";
                    // Read back so the screen shows what the printer really has now
                    ReadPrinterSettings();
                });
        }

        private void CalibratePrinter()
        {
            if (_busy)
                return;
            _statusPrinter.Text = "Printer: kalibreren...";
            AddLog("Printer kalibreren");
            RunInBackground(
                delegate
                {
                    PrinterSettingsService.Calibrate();
                    return null;
                },
                delegate
                {
                    _statusPrinter.Text = "Printer: gekalibreerd";
                    AddLog("Printer gekalibreerd");
                });
        }

        private void SetPrinterInputsEnabled()
        {
            bool canEdit = _printerConfig != null && !_busy;
            _printerSettingsTable.Enabled = true;
            _speedInput.Enabled = canEdit;
            _darknessInput.Enabled = canEdit;
            _modeInput.Enabled = canEdit;
            _offsetInput.Enabled = canEdit;
            _takeawayInput.Enabled = canEdit;
            _rewindInput.Enabled = canEdit;
            _takeupInput.Enabled = canEdit;
            _applyPrinterButton.Enabled = canEdit;
            _readPrinterButton.Enabled = !_busy;
            _calibrateButton.Enabled = !_busy;
        }

        //
        // Helpers
        //

        private delegate object BackgroundWork();

        /// <summary>
        /// Runs work on a background thread; <paramref name="done"/> runs on the UI thread.
        /// Exceptions are logged and shown.
        /// </summary>
        private void RunInBackground(BackgroundWork work, Action<object> done)
        {
            SetBusy(true);
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += delegate(object s, DoWorkEventArgs args) { args.Result = work(); };
            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                SetBusy(false);
                if (args.Error != null)
                {
                    AddLog("Fout: " + args.Error.Message);
                    _statusPrinter.Text = _statusPrinter.Text.EndsWith("...") ? "Printer: fout" : _statusPrinter.Text;
                    MessageBox.Show(this, args.Error.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                done(args.Result);
            };
            worker.RunWorkerAsync();
        }

        private void SetBusy(bool busy)
        {
            _busy = busy;
            Cursor = busy ? Cursors.AppStarting : Cursors.Default;
            _printButton.Enabled = !busy;
            _feedButton.Enabled = !busy;
            _detectButton.Enabled = !busy;
            _scanButton.Enabled = !busy && !_scale.IsOpen;
            _settingsTable.Enabled = !busy && !_scale.IsOpen;
            SetPrinterInputsEnabled();
        }

        // MethodInvoker instead of Action: non-generic Action is in System.Core (.NET 3.5)
        private void RunSafe(MethodInvoker action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                AddLog("Fout: " + ex.Message);
            }
        }

        private void AddLog(string message)
        {
            if (_logList == null)
                return;
            _logList.Items.Insert(0, DateTime.Now.ToString("HH:mm:ss") + "  " + message);
            while (_logList.Items.Count > 2000)
                _logList.Items.RemoveAt(_logList.Items.Count - 1);
        }

        private void CopyLogButton_Click(object sender, EventArgs e)
        {
            List<string> lines = new List<string>();
            for (int i = _logList.Items.Count - 1; i >= 0; i--)
                lines.Add(_logList.Items[i].ToString());
            if (lines.Count == 0)
                return;
            Clipboard.SetText(string.Join(Environment.NewLine, lines.ToArray()));
            AddLog("Log gekopieerd naar klembord");
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            _scale.Dispose();
            ReadSettingsFromUi();
            try
            {
                _settings.Save();
            }
            catch (Exception)
            {
                // Read-only location; not worth bothering the user on exit
            }
        }
    }
}
