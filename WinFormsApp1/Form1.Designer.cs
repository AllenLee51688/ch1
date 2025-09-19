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
            lblSector = new Label();
            cmbSector = new ComboBox();
            lblBlock = new Label();
            cmbBlock = new ComboBox();
            lblKeyAB = new Label();
            cmbKeyAB = new ComboBox();
            lblLoadKey = new Label();
            txtLoadKey = new TextBox();
            btnReadData = new Button();
            btnReadCard = new Button();
            txtResult = new TextBox();
            btnClose = new Button();
            SuspendLayout();
            // 
            // lblSector
            // 
            lblSector.AutoSize = true;
            lblSector.Location = new Point(30, 30);
            lblSector.Name = "lblSector";
            lblSector.Size = new Size(49, 15);
            lblSector.TabIndex = 0;
            lblSector.Text = "Sector :";
            // 
            // cmbSector
            // 
            cmbSector.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSector.FormattingEnabled = true;
            cmbSector.Location = new Point(100, 27);
            cmbSector.Name = "cmbSector";
            cmbSector.Size = new Size(80, 23);
            cmbSector.TabIndex = 1;
            cmbSector.SelectedIndexChanged += ValidateInputs;
            // 
            // lblBlock
            // 
            lblBlock.AutoSize = true;
            lblBlock.Location = new Point(200, 30);
            lblBlock.Name = "lblBlock";
            lblBlock.Size = new Size(43, 15);
            lblBlock.TabIndex = 2;
            lblBlock.Text = "Block :";
            // 
            // cmbBlock
            // 
            cmbBlock.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbBlock.FormattingEnabled = true;
            cmbBlock.Location = new Point(250, 27);
            cmbBlock.Name = "cmbBlock";
            cmbBlock.Size = new Size(80, 23);
            cmbBlock.TabIndex = 3;
            cmbBlock.SelectedIndexChanged += ValidateInputs;
            // 
            // lblKeyAB
            // 
            lblKeyAB.AutoSize = true;
            lblKeyAB.Location = new Point(350, 30);
            lblKeyAB.Name = "lblKeyAB";
            lblKeyAB.Size = new Size(48, 15);
            lblKeyAB.TabIndex = 4;
            lblKeyAB.Text = "KeyAB :";
            // 
            // cmbKeyAB
            // 
            cmbKeyAB.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbKeyAB.FormattingEnabled = true;
            cmbKeyAB.Location = new Point(410, 27);
            cmbKeyAB.Name = "cmbKeyAB";
            cmbKeyAB.Size = new Size(60, 23);
            cmbKeyAB.TabIndex = 5;
            cmbKeyAB.SelectedIndexChanged += ValidateInputs;
            // 
            // lblLoadKey
            // 
            lblLoadKey.AutoSize = true;
            lblLoadKey.Location = new Point(490, 30);
            lblLoadKey.Name = "lblLoadKey";
            lblLoadKey.Size = new Size(65, 15);
            lblLoadKey.TabIndex = 6;
            lblLoadKey.Text = "Load Key :";
            // 
            // txtLoadKey
            // 
            txtLoadKey.Location = new Point(570, 27);
            txtLoadKey.MaxLength = 12;
            txtLoadKey.Name = "txtLoadKey";
            txtLoadKey.Size = new Size(120, 23);
            txtLoadKey.TabIndex = 7;
            txtLoadKey.Text = "FFFFFFFFFFFF";
            txtLoadKey.TextChanged += ValidateInputs;
            // 
            // btnReadData
            // 
            btnReadData.Enabled = false;
            btnReadData.Location = new Point(570, 80);
            btnReadData.Name = "btnReadData";
            btnReadData.Size = new Size(120, 30);
            btnReadData.TabIndex = 8;
            btnReadData.Text = "Read data";
            btnReadData.UseVisualStyleBackColor = true;
            btnReadData.Click += btnReadData_Click;
            // 
            // btnReadCard
            // 
            //btnReadCard.Location = new Point(570, 115);
            //btnReadCard.Name = "btnReadCard";
            //btnReadCard.Size = new Size(120, 30);
            //btnReadCard.TabIndex = 9;
            //btnReadCard.Text = "Read Card (RD300)";
            //btnReadCard.UseVisualStyleBackColor = true;
            //btnReadCard.Visible = false;
            //btnReadCard.Click += btnReadCard_Click;
            // 
            // txtResult
            // 
            txtResult.Location = new Point(30, 80);
            txtResult.Multiline = true;
            txtResult.Name = "txtResult";
            txtResult.ReadOnly = true;
            txtResult.ScrollBars = ScrollBars.Both;
            txtResult.Size = new Size(520, 135);
            txtResult.TabIndex = 10;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(570, 185);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(120, 30);
            btnClose.TabIndex = 11;
            btnClose.Text = "Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(720, 250);
            Controls.Add(btnClose);
            Controls.Add(txtResult);
            Controls.Add(btnReadCard);
            Controls.Add(btnReadData);
            Controls.Add(txtLoadKey);
            Controls.Add(lblLoadKey);
            Controls.Add(cmbKeyAB);
            Controls.Add(lblKeyAB);
            Controls.Add(cmbBlock);
            Controls.Add(lblBlock);
            Controls.Add(cmbSector);
            Controls.Add(lblSector);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "Form1";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "ISO14443A";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
    }
}
