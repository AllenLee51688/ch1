namespace WinFormsApp1
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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

        private System.Windows.Forms.Label lblSector;
        private System.Windows.Forms.ComboBox cmbSector;
        private System.Windows.Forms.Label lblBlock;
        private System.Windows.Forms.ComboBox cmbBlock;
        private System.Windows.Forms.Label lblKeyAB;
        private System.Windows.Forms.ComboBox cmbKeyAB;
        private System.Windows.Forms.Label lblLoadKey;
        private System.Windows.Forms.TextBox txtLoadKey;
        private System.Windows.Forms.Button btnReadData;
        private System.Windows.Forms.Button btnReadCard;
        private System.Windows.Forms.TextBox txtResult;
        private System.Windows.Forms.Button btnClose;

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblSector = new System.Windows.Forms.Label();
            this.cmbSector = new System.Windows.Forms.ComboBox();
            this.lblBlock = new System.Windows.Forms.Label();
            this.cmbBlock = new System.Windows.Forms.ComboBox();
            this.lblKeyAB = new System.Windows.Forms.Label();
            this.cmbKeyAB = new System.Windows.Forms.ComboBox();
            this.lblLoadKey = new System.Windows.Forms.Label();
            this.txtLoadKey = new System.Windows.Forms.TextBox();
            this.btnReadData = new System.Windows.Forms.Button();
            this.btnReadCard = new System.Windows.Forms.Button();
            this.txtResult = new System.Windows.Forms.TextBox();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();

            // lblSector
            this.lblSector.AutoSize = true;
            this.lblSector.Location = new System.Drawing.Point(30, 30);
            this.lblSector.Name = "lblSector";
            this.lblSector.Size = new System.Drawing.Size(48, 15);
            this.lblSector.TabIndex = 0;
            this.lblSector.Text = "Sector :";

            // cmbSector
            this.cmbSector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbSector.FormattingEnabled = true;
            this.cmbSector.Location = new System.Drawing.Point(100, 27);
            this.cmbSector.Name = "cmbSector";
            this.cmbSector.Size = new System.Drawing.Size(80, 23);
            this.cmbSector.TabIndex = 1;
            this.cmbSector.SelectedIndexChanged += new System.EventHandler(this.ValidateInputs);

            // lblBlock
            this.lblBlock.AutoSize = true;
            this.lblBlock.Location = new System.Drawing.Point(200, 30);
            this.lblBlock.Name = "lblBlock";
            this.lblBlock.Size = new System.Drawing.Size(41, 15);
            this.lblBlock.TabIndex = 2;
            this.lblBlock.Text = "Block :";

            // cmbBlock
            this.cmbBlock.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbBlock.FormattingEnabled = true;
            this.cmbBlock.Location = new System.Drawing.Point(250, 27);
            this.cmbBlock.Name = "cmbBlock";
            this.cmbBlock.Size = new System.Drawing.Size(80, 23);
            this.cmbBlock.TabIndex = 3;
            this.cmbBlock.SelectedIndexChanged += new System.EventHandler(this.ValidateInputs);

            // lblKeyAB
            this.lblKeyAB.AutoSize = true;
            this.lblKeyAB.Location = new System.Drawing.Point(350, 30);
            this.lblKeyAB.Name = "lblKeyAB";
            this.lblKeyAB.Size = new System.Drawing.Size(52, 15);
            this.lblKeyAB.TabIndex = 4;
            this.lblKeyAB.Text = "KeyAB :";

            // cmbKeyAB
            this.cmbKeyAB.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbKeyAB.FormattingEnabled = true;
            this.cmbKeyAB.Location = new System.Drawing.Point(410, 27);
            this.cmbKeyAB.Name = "cmbKeyAB";
            this.cmbKeyAB.Size = new System.Drawing.Size(60, 23);
            this.cmbKeyAB.TabIndex = 5;
            this.cmbKeyAB.SelectedIndexChanged += new System.EventHandler(this.ValidateInputs);

            // lblLoadKey
            this.lblLoadKey.AutoSize = true;
            this.lblLoadKey.Location = new System.Drawing.Point(490, 30);
            this.lblLoadKey.Name = "lblLoadKey";
            this.lblLoadKey.Size = new System.Drawing.Size(64, 15);
            this.lblLoadKey.TabIndex = 6;
            this.lblLoadKey.Text = "Load Key :";

            // txtLoadKey
            this.txtLoadKey.Location = new System.Drawing.Point(570, 27);
            this.txtLoadKey.MaxLength = 12;
            this.txtLoadKey.Name = "txtLoadKey";
            this.txtLoadKey.Size = new System.Drawing.Size(120, 23);
            this.txtLoadKey.TabIndex = 7;
            this.txtLoadKey.Text = "FFFFFFFFFFFF";
            this.txtLoadKey.TextChanged += new System.EventHandler(this.ValidateInputs);

            // btnReadData
            this.btnReadData.Enabled = false;
            this.btnReadData.Location = new System.Drawing.Point(570, 80);
            this.btnReadData.Name = "btnReadData";
            this.btnReadData.Size = new System.Drawing.Size(120, 30);
            this.btnReadData.TabIndex = 8;
            this.btnReadData.Text = "Read data";
            this.btnReadData.UseVisualStyleBackColor = true;
            this.btnReadData.Click += new System.EventHandler(this.btnReadData_Click);

            // btnReadCard
            this.btnReadCard.Location = new System.Drawing.Point(570, 115);
            this.btnReadCard.Name = "btnReadCard";
            this.btnReadCard.Size = new System.Drawing.Size(120, 30);
            this.btnReadCard.TabIndex = 9;
            this.btnReadCard.Text = "Read Card (RD300)";
            this.btnReadCard.UseVisualStyleBackColor = true;
            this.btnReadCard.Click += new System.EventHandler(this.btnReadCard_Click);

            // txtResult
            this.txtResult.Location = new System.Drawing.Point(30, 80);
            this.txtResult.Multiline = true;
            this.txtResult.Name = "txtResult";
            this.txtResult.ReadOnly = true;
            this.txtResult.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtResult.Size = new System.Drawing.Size(520, 135);
            this.txtResult.TabIndex = 10;

            // btnClose
            this.btnClose.Location = new System.Drawing.Point(570, 185);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 30);
            this.btnClose.TabIndex = 11;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // Form1
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(720, 250);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.txtResult);
            this.Controls.Add(this.btnReadCard);
            this.Controls.Add(this.btnReadData);
            this.Controls.Add(this.txtLoadKey);
            this.Controls.Add(this.lblLoadKey);
            this.Controls.Add(this.cmbKeyAB);
            this.Controls.Add(this.lblKeyAB);
            this.Controls.Add(this.cmbBlock);
            this.Controls.Add(this.lblBlock);
            this.Controls.Add(this.cmbSector);
            this.Controls.Add(this.lblSector);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ISO14443A";
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    }
}
