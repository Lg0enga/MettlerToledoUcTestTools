namespace MettlerToledoLoadCellTool
{

    partial class MettlerScaleReader
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.eventBox = new System.Windows.Forms.ListBox();
            this.loadcellGroupBox = new System.Windows.Forms.GroupBox();
            this.closeScaleBtn = new System.Windows.Forms.Button();
            this.tarraScaleBtn = new System.Windows.Forms.Button();
            this.nullScaleBtn = new System.Windows.Forms.Button();
            this.openScaleBtn = new System.Windows.Forms.Button();
            this.tarraWeightLabel = new System.Windows.Forms.Label();
            this.weightLabel = new System.Windows.Forms.Label();
            this.boardNameLabel = new System.Windows.Forms.Label();
            this.boardNameInput = new System.Windows.Forms.TextBox();
            this.i2cBusLabel = new System.Windows.Forms.Label();
            this.i2cBusInput = new System.Windows.Forms.NumericUpDown();
            this.i2cAddressLabel = new System.Windows.Forms.Label();
            this.i2cAddressInput = new System.Windows.Forms.NumericUpDown();
            this.scanI2cBtn = new System.Windows.Forms.Button();
            this.printerGroupBox = new System.Windows.Forms.GroupBox();
            this.feedLabelBtn = new System.Windows.Forms.Button();
            this.printTestLabelBtn = new System.Windows.Forms.Button();
            this.closePrinterBtn = new System.Windows.Forms.Button();
            this.openPrinterBtn = new System.Windows.Forms.Button();
            this.loadcellGroupBox.SuspendLayout();
            this.printerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.i2cBusInput)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.i2cAddressInput)).BeginInit();
            this.SuspendLayout();
            // 
            // eventBox
            // 
            this.eventBox.FormattingEnabled = true;
            this.eventBox.ItemHeight = 20;
            this.eventBox.Location = new System.Drawing.Point(12, 225);
            this.eventBox.Name = "eventBox";
            this.eventBox.Size = new System.Drawing.Size(1257, 724);
            this.eventBox.TabIndex = 0;
            // 
            // loadcellGroupBox
            // 
            this.loadcellGroupBox.Controls.Add(this.closeScaleBtn);
            this.loadcellGroupBox.Controls.Add(this.tarraScaleBtn);
            this.loadcellGroupBox.Controls.Add(this.nullScaleBtn);
            this.loadcellGroupBox.Controls.Add(this.openScaleBtn);
            this.loadcellGroupBox.Controls.Add(this.tarraWeightLabel);
            this.loadcellGroupBox.Controls.Add(this.weightLabel);
            this.loadcellGroupBox.Controls.Add(this.boardNameLabel);
            this.loadcellGroupBox.Controls.Add(this.boardNameInput);
            this.loadcellGroupBox.Controls.Add(this.i2cBusLabel);
            this.loadcellGroupBox.Controls.Add(this.i2cBusInput);
            this.loadcellGroupBox.Controls.Add(this.i2cAddressLabel);
            this.loadcellGroupBox.Controls.Add(this.i2cAddressInput);
            this.loadcellGroupBox.Controls.Add(this.scanI2cBtn);
            this.loadcellGroupBox.Location = new System.Drawing.Point(589, 12);
            this.loadcellGroupBox.Name = "loadcellGroupBox";
            this.loadcellGroupBox.Size = new System.Drawing.Size(680, 205);
            this.loadcellGroupBox.TabIndex = 20;
            this.loadcellGroupBox.TabStop = false;
            this.loadcellGroupBox.Text = "UC loadcell";
            // 
            // closeScaleBtn
            // 
            this.closeScaleBtn.Enabled = false;
            this.closeScaleBtn.Location = new System.Drawing.Point(6, 86);
            this.closeScaleBtn.Name = "closeScaleBtn";
            this.closeScaleBtn.Size = new System.Drawing.Size(134, 50);
            this.closeScaleBtn.TabIndex = 21;
            this.closeScaleBtn.Text = "Close scale";
            this.closeScaleBtn.UseVisualStyleBackColor = true;
            this.closeScaleBtn.Click += new System.EventHandler(this.closeScaleBtn_click);
            // 
            // tarraScaleBtn
            // 
            this.tarraScaleBtn.Enabled = false;
            this.tarraScaleBtn.Location = new System.Drawing.Point(146, 86);
            this.tarraScaleBtn.Name = "tarraScaleBtn";
            this.tarraScaleBtn.Size = new System.Drawing.Size(134, 50);
            this.tarraScaleBtn.TabIndex = 20;
            this.tarraScaleBtn.Text = "Tarra";
            this.tarraScaleBtn.UseVisualStyleBackColor = true;
            this.tarraScaleBtn.Click += new System.EventHandler(this.tarraScaleBtn_Click_1);
            // 
            // nullScaleBtn
            // 
            this.nullScaleBtn.Enabled = false;
            this.nullScaleBtn.Location = new System.Drawing.Point(146, 30);
            this.nullScaleBtn.Name = "nullScaleBtn";
            this.nullScaleBtn.Size = new System.Drawing.Size(134, 50);
            this.nullScaleBtn.TabIndex = 19;
            this.nullScaleBtn.Text = "Null scale";
            this.nullScaleBtn.UseVisualStyleBackColor = true;
            this.nullScaleBtn.Click += new System.EventHandler(this.nullScaleBtn_Click_1);
            // 
            // openScaleBtn
            // 
            this.openScaleBtn.Location = new System.Drawing.Point(6, 30);
            this.openScaleBtn.Name = "openScaleBtn";
            this.openScaleBtn.Size = new System.Drawing.Size(134, 50);
            this.openScaleBtn.TabIndex = 18;
            this.openScaleBtn.Text = "Open scale";
            this.openScaleBtn.UseVisualStyleBackColor = true;
            this.openScaleBtn.Click += new System.EventHandler(this.openScaleBtn_Click);
            // 
            // tarraWeightLabel
            // 
            this.tarraWeightLabel.AutoSize = true;
            this.tarraWeightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tarraWeightLabel.Location = new System.Drawing.Point(348, 91);
            this.tarraWeightLabel.Name = "tarraWeightLabel";
            this.tarraWeightLabel.Size = new System.Drawing.Size(24, 32);
            this.tarraWeightLabel.TabIndex = 17;
            this.tarraWeightLabel.Text = "-";
            // 
            // weightLabel
            // 
            this.weightLabel.AutoSize = true;
            this.weightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weightLabel.Location = new System.Drawing.Point(348, 35);
            this.weightLabel.Name = "weightLabel";
            this.weightLabel.Size = new System.Drawing.Size(24, 32);
            this.weightLabel.TabIndex = 16;
            this.weightLabel.Text = "-";
            // 
            // boardNameLabel
            //
            this.boardNameLabel.AutoSize = true;
            this.boardNameLabel.Location = new System.Drawing.Point(6, 157);
            this.boardNameLabel.Name = "boardNameLabel";
            this.boardNameLabel.Size = new System.Drawing.Size(54, 20);
            this.boardNameLabel.TabIndex = 27;
            this.boardNameLabel.Text = "Board:";
            //
            // boardNameInput
            //
            this.boardNameInput.Location = new System.Drawing.Point(66, 155);
            this.boardNameInput.Name = "boardNameInput";
            this.boardNameInput.Size = new System.Drawing.Size(80, 26);
            this.boardNameInput.TabIndex = 28;
            this.boardNameInput.Text = "UUP6";
            //
            // i2cBusLabel
            // 
            this.i2cBusLabel.AutoSize = true;
            this.i2cBusLabel.Location = new System.Drawing.Point(158, 157);
            this.i2cBusLabel.Name = "i2cBusLabel";
            this.i2cBusLabel.Size = new System.Drawing.Size(65, 20);
            this.i2cBusLabel.TabIndex = 22;
            this.i2cBusLabel.Text = "I2C bus:";
            // 
            // i2cBusInput
            // 
            this.i2cBusInput.Location = new System.Drawing.Point(228, 155);
            this.i2cBusInput.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.i2cBusInput.Name = "i2cBusInput";
            this.i2cBusInput.Size = new System.Drawing.Size(55, 26);
            this.i2cBusInput.TabIndex = 23;
            // 
            // i2cAddressLabel
            // 
            this.i2cAddressLabel.AutoSize = true;
            this.i2cAddressLabel.Location = new System.Drawing.Point(295, 157);
            this.i2cAddressLabel.Name = "i2cAddressLabel";
            this.i2cAddressLabel.Size = new System.Drawing.Size(145, 20);
            this.i2cAddressLabel.TabIndex = 24;
            this.i2cAddressLabel.Text = "I2C address (hex):";
            // 
            // i2cAddressInput
            // 
            this.i2cAddressInput.Hexadecimal = true;
            this.i2cAddressInput.Location = new System.Drawing.Point(445, 155);
            this.i2cAddressInput.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.i2cAddressInput.Name = "i2cAddressInput";
            this.i2cAddressInput.Size = new System.Drawing.Size(65, 26);
            this.i2cAddressInput.TabIndex = 25;
            this.i2cAddressInput.Value = new decimal(new int[] {
            192,
            0,
            0,
            0});
            // 
            // scanI2cBtn
            //
            this.scanI2cBtn.Location = new System.Drawing.Point(525, 148);
            this.scanI2cBtn.Name = "scanI2cBtn";
            this.scanI2cBtn.Size = new System.Drawing.Size(145, 40);
            this.scanI2cBtn.TabIndex = 26;
            this.scanI2cBtn.Text = "Scan I2C";
            this.scanI2cBtn.UseVisualStyleBackColor = true;
            this.scanI2cBtn.Click += new System.EventHandler(this.scanI2cBtn_Click);
            //
            // printerGroupBox
            // 
            this.printerGroupBox.Controls.Add(this.feedLabelBtn);
            this.printerGroupBox.Controls.Add(this.printTestLabelBtn);
            this.printerGroupBox.Controls.Add(this.closePrinterBtn);
            this.printerGroupBox.Controls.Add(this.openPrinterBtn);
            this.printerGroupBox.Location = new System.Drawing.Point(12, 12);
            this.printerGroupBox.Name = "printerGroupBox";
            this.printerGroupBox.Size = new System.Drawing.Size(571, 205);
            this.printerGroupBox.TabIndex = 19;
            this.printerGroupBox.TabStop = false;
            this.printerGroupBox.Text = "UC Printer";
            // 
            // feedLabelBtn
            // 
            this.feedLabelBtn.Enabled = false;
            this.feedLabelBtn.Location = new System.Drawing.Point(431, 86);
            this.feedLabelBtn.Name = "feedLabelBtn";
            this.feedLabelBtn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.feedLabelBtn.Size = new System.Drawing.Size(134, 50);
            this.feedLabelBtn.TabIndex = 3;
            this.feedLabelBtn.Text = "Feed label";
            this.feedLabelBtn.UseVisualStyleBackColor = true;
            this.feedLabelBtn.Click += new System.EventHandler(this.feedLabelBtn_Click);
            // 
            // printTestLabelBtn
            // 
            this.printTestLabelBtn.Enabled = false;
            this.printTestLabelBtn.Location = new System.Drawing.Point(431, 30);
            this.printTestLabelBtn.Name = "printTestLabelBtn";
            this.printTestLabelBtn.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.printTestLabelBtn.Size = new System.Drawing.Size(134, 50);
            this.printTestLabelBtn.TabIndex = 2;
            this.printTestLabelBtn.Text = "Print test label";
            this.printTestLabelBtn.UseVisualStyleBackColor = true;
            this.printTestLabelBtn.Click += new System.EventHandler(this.printTestLabelBtn_Click);
            // 
            // closePrinterBtn
            // 
            this.closePrinterBtn.Enabled = false;
            this.closePrinterBtn.Location = new System.Drawing.Point(7, 86);
            this.closePrinterBtn.Name = "closePrinterBtn";
            this.closePrinterBtn.Size = new System.Drawing.Size(134, 50);
            this.closePrinterBtn.TabIndex = 1;
            this.closePrinterBtn.Text = "Close Printer";
            this.closePrinterBtn.UseVisualStyleBackColor = true;
            this.closePrinterBtn.Click += new System.EventHandler(this.closePrinterBtn_Click);
            // 
            // openPrinterBtn
            // 
            this.openPrinterBtn.Location = new System.Drawing.Point(7, 30);
            this.openPrinterBtn.Name = "openPrinterBtn";
            this.openPrinterBtn.Size = new System.Drawing.Size(134, 50);
            this.openPrinterBtn.TabIndex = 0;
            this.openPrinterBtn.Text = "Open Printer";
            this.openPrinterBtn.UseVisualStyleBackColor = true;
            this.openPrinterBtn.Click += new System.EventHandler(this.openPrinterBtn_Click);
            // 
            // MettlerScaleReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1281, 961);
            this.Controls.Add(this.loadcellGroupBox);
            this.Controls.Add(this.printerGroupBox);
            this.Controls.Add(this.eventBox);
            this.Name = "MettlerScaleReader";
            this.Text = "Mettler Toledo UC Loadcell test tool";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.loadcellGroupBox.ResumeLayout(false);
            this.loadcellGroupBox.PerformLayout();
            this.printerGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.i2cBusInput)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.i2cAddressInput)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox eventBox;
        private System.Windows.Forms.GroupBox loadcellGroupBox;
        private System.Windows.Forms.Button closeScaleBtn;
        private System.Windows.Forms.Button tarraScaleBtn;
        private System.Windows.Forms.Button nullScaleBtn;
        private System.Windows.Forms.Button openScaleBtn;
        private System.Windows.Forms.Label tarraWeightLabel;
        private System.Windows.Forms.Label weightLabel;
        private System.Windows.Forms.GroupBox printerGroupBox;
        private System.Windows.Forms.Button feedLabelBtn;
        private System.Windows.Forms.Button printTestLabelBtn;
        private System.Windows.Forms.Button closePrinterBtn;
        private System.Windows.Forms.Button openPrinterBtn;
        private System.Windows.Forms.Label boardNameLabel;
        private System.Windows.Forms.TextBox boardNameInput;
        private System.Windows.Forms.Label i2cBusLabel;
        private System.Windows.Forms.NumericUpDown i2cBusInput;
        private System.Windows.Forms.Label i2cAddressLabel;
        private System.Windows.Forms.NumericUpDown i2cAddressInput;
        private System.Windows.Forms.Button scanI2cBtn;
    }

}