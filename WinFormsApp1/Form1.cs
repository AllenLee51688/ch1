using System.IO.Ports;
using System.Text;

namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        private RFIDReader rfidReader;

        public Form1()
        {
            InitializeComponent();
            rfidReader = new RFIDReader();
            InitializeFormData();
        }

        private void InitializeFormData()
        {
            // 初始化 Sector ComboBox (00-15)
            for (int i = 0; i <= 15; i++)
            {
                cmbSector.Items.Add(i.ToString("00"));
            }

            // 初始化 Block ComboBox (00-03)
            for (int i = 0; i <= 3; i++)
            {
                cmbBlock.Items.Add(i.ToString("00"));
            }

            // 初始化 KeyAB ComboBox
            cmbKeyAB.Items.Add("A");
            cmbKeyAB.Items.Add("B");

            // 預設選擇第一項
            cmbSector.SelectedIndex = 0;
            cmbBlock.SelectedIndex = 0;
            cmbKeyAB.SelectedIndex = 0;

            // 驗證初始狀態
            ValidateInputs(null, EventArgs.Empty);
        }

        private void ValidateInputs(object? sender, EventArgs e)
        {
            // 防呆機制：檢查所有必要欄位是否已選擇/填寫
            bool isValid = cmbSector.SelectedIndex >= 0 &&
                          cmbBlock.SelectedIndex >= 0 &&
                          cmbKeyAB.SelectedIndex >= 0 &&
                          txtLoadKey.Text.Length == 12 &&
                          IsValidHexString(txtLoadKey.Text);

            btnReadData.Enabled = isValid;
        }

        private bool IsValidHexString(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            foreach (char c in text)
            {
                if (!char.IsDigit(c) && !(c >= 'A' && c <= 'F') && !(c >= 'a' && c <= 'f'))
                    return false;
            }
            return true;
        }

        private void btnReadData_Click(object? sender, EventArgs e)
        {
            try
            {
                string sector = cmbSector.SelectedItem?.ToString() ?? "";
                string block = cmbBlock.SelectedItem?.ToString() ?? "";
                string keyType = cmbKeyAB.SelectedItem?.ToString() ?? "";
                string loadKey = txtLoadKey.Text;

                int sectorIndex = int.Parse(sector);
                int blockIndex = int.Parse(block);

                // 直接呼叫讀取方法，內部會處理連接
                string result = rfidReader.ReadMifareSpecificBlock(sectorIndex, blockIndex, keyType, loadKey);

                txtResult.Text = result;

                // 驗證結果是否符合預期（如PDF中的範例）
                if (sectorIndex == 0 && blockIndex == 0 && result.StartsWith("86ED89FE1C88"))
                {
                    MessageBox.Show("讀取成功！結果與範例資料相符。", "驗證成功",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"讀取資料時發生錯誤: {ex.Message}", "錯誤",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        //private void btnReadCard_Click(object? sender, EventArgs e)
        //{
        //    try
        //    {
        //        // 使用多種方法嘗試讀取完整的卡片資料
        //        string result = rfidReader.ReadCardComplete();
        //        txtResult.Text = result;

        //        // 檢查是否有任何方法成功讀取到16字節資料
        //        if (result.Contains("完整區塊資料 (16字節)") || result.Contains("✅"))
        //        {
        //            MessageBox.Show("RD300 讀卡成功！", "成功",
        //                MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //        else if (result.Contains("沒有偵測到卡片"))
        //        {
        //            MessageBox.Show("請將卡片放在讀卡機上", "提示",
        //                MessageBoxButtons.OK, MessageBoxIcon.Information);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"RD300 讀卡時發生錯誤: {ex.Message}", "錯誤",
        //            MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        private void btnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            rfidReader?.Disconnect();
            base.OnFormClosed(e);
        }
    }
}
