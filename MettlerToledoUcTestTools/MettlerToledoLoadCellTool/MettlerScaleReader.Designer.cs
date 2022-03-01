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
            this.openBtn = new System.Windows.Forms.Button();
            this.closeBtn = new System.Windows.Forms.Button();
            this.openBoardBtn = new System.Windows.Forms.Button();
            this.startWeightBtn = new System.Windows.Forms.Button();
            this.closeBoardBtn = new System.Windows.Forms.Button();
            this.initScaleButn = new System.Windows.Forms.Button();
            this.weightLabel = new System.Windows.Forms.Label();
            this.stopReadingWeightBtn = new System.Windows.Forms.Button();
            this.resetJidaBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // eventBox
            // 
            this.eventBox.FormattingEnabled = true;
            this.eventBox.ItemHeight = 20;
            this.eventBox.Location = new System.Drawing.Point(12, 154);
            this.eventBox.Name = "eventBox";
            this.eventBox.Size = new System.Drawing.Size(1447, 784);
            this.eventBox.TabIndex = 0;
            // 
            // openBtn
            // 
            this.openBtn.Location = new System.Drawing.Point(12, 12);
            this.openBtn.Name = "openBtn";
            this.openBtn.Size = new System.Drawing.Size(112, 54);
            this.openBtn.TabIndex = 1;
            this.openBtn.Text = "Open";
            this.openBtn.UseVisualStyleBackColor = true;
            this.openBtn.Click += new System.EventHandler(this.openBtn_Click);
            // 
            // closeBtn
            // 
            this.closeBtn.Location = new System.Drawing.Point(12, 77);
            this.closeBtn.Name = "closeBtn";
            this.closeBtn.Size = new System.Drawing.Size(112, 54);
            this.closeBtn.TabIndex = 2;
            this.closeBtn.Text = "Close";
            this.closeBtn.UseVisualStyleBackColor = true;
            this.closeBtn.Click += new System.EventHandler(this.closeBtn_Click);
            // 
            // openBoardBtn
            // 
            this.openBoardBtn.Location = new System.Drawing.Point(210, 12);
            this.openBoardBtn.Name = "openBoardBtn";
            this.openBoardBtn.Size = new System.Drawing.Size(112, 54);
            this.openBoardBtn.TabIndex = 4;
            this.openBoardBtn.Text = " Init Jida";
            this.openBoardBtn.UseVisualStyleBackColor = true;
            this.openBoardBtn.Click += new System.EventHandler(this.openBoardBtn_Click);
            // 
            // startWeightBtn
            // 
            this.startWeightBtn.Location = new System.Drawing.Point(462, 77);
            this.startWeightBtn.Name = "startWeightBtn";
            this.startWeightBtn.Size = new System.Drawing.Size(112, 54);
            this.startWeightBtn.TabIndex = 5;
            this.startWeightBtn.Text = "Start reading";
            this.startWeightBtn.UseVisualStyleBackColor = true;
            this.startWeightBtn.Click += new System.EventHandler(this.startReadingWeightBtn_Click);
            // 
            // closeBoardBtn
            // 
            this.closeBoardBtn.Location = new System.Drawing.Point(328, 77);
            this.closeBoardBtn.Name = "closeBoardBtn";
            this.closeBoardBtn.Size = new System.Drawing.Size(112, 54);
            this.closeBoardBtn.TabIndex = 6;
            this.closeBoardBtn.Text = "Close";
            this.closeBoardBtn.UseVisualStyleBackColor = true;
            this.closeBoardBtn.Click += new System.EventHandler(this.closeBoardBtn_Click);
            // 
            // initScaleButn
            // 
            this.initScaleButn.Location = new System.Drawing.Point(462, 17);
            this.initScaleButn.Name = "initScaleButn";
            this.initScaleButn.Size = new System.Drawing.Size(112, 54);
            this.initScaleButn.TabIndex = 7;
            this.initScaleButn.Text = "Init Scale";
            this.initScaleButn.UseVisualStyleBackColor = true;
            this.initScaleButn.Click += new System.EventHandler(this.initScaleBtn_Click);
            // 
            // weightLabel
            // 
            this.weightLabel.AutoSize = true;
            this.weightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.weightLabel.Location = new System.Drawing.Point(1243, 17);
            this.weightLabel.Name = "weightLabel";
            this.weightLabel.Size = new System.Drawing.Size(24, 32);
            this.weightLabel.TabIndex = 8;
            this.weightLabel.Text = "-";
            // 
            // stopReadingWeightBtn
            // 
            this.stopReadingWeightBtn.Location = new System.Drawing.Point(580, 77);
            this.stopReadingWeightBtn.Name = "stopReadingWeightBtn";
            this.stopReadingWeightBtn.Size = new System.Drawing.Size(112, 54);
            this.stopReadingWeightBtn.TabIndex = 9;
            this.stopReadingWeightBtn.Text = "Stop reading";
            this.stopReadingWeightBtn.UseVisualStyleBackColor = true;
            this.stopReadingWeightBtn.Click += new System.EventHandler(this.stopReadingWeightBtn_Click);
            // 
            // resetJidaBtn
            // 
            this.resetJidaBtn.Location = new System.Drawing.Point(210, 77);
            this.resetJidaBtn.Name = "resetJidaBtn";
            this.resetJidaBtn.Size = new System.Drawing.Size(112, 54);
            this.resetJidaBtn.TabIndex = 10;
            this.resetJidaBtn.Text = "Reset Jida";
            this.resetJidaBtn.UseVisualStyleBackColor = true;
            this.resetJidaBtn.Click += new System.EventHandler(this.resetJidaBtn_Click);
            // 
            // MettlerScaleReader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1471, 961);
            this.Controls.Add(this.resetJidaBtn);
            this.Controls.Add(this.stopReadingWeightBtn);
            this.Controls.Add(this.weightLabel);
            this.Controls.Add(this.initScaleButn);
            this.Controls.Add(this.closeBoardBtn);
            this.Controls.Add(this.startWeightBtn);
            this.Controls.Add(this.openBoardBtn);
            this.Controls.Add(this.closeBtn);
            this.Controls.Add(this.openBtn);
            this.Controls.Add(this.eventBox);
            this.Name = "MettlerScaleReader";
            this.Text = "Mettler Toledo UC Loadcell test tool";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox eventBox;
        private System.Windows.Forms.Button openBtn;
        private System.Windows.Forms.Button closeBtn;
        private System.Windows.Forms.Button openBoardBtn;
        private System.Windows.Forms.Button startWeightBtn;
        private System.Windows.Forms.Button closeBoardBtn;
        private System.Windows.Forms.Button initScaleButn;
        private System.Windows.Forms.Label weightLabel;
        private System.Windows.Forms.Button stopReadingWeightBtn;
        private System.Windows.Forms.Button resetJidaBtn;
    }

}