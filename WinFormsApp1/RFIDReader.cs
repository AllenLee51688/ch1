using System.Text;

namespace WinFormsApp1
{
    public class RFIDReader
    {
        private MW_EasyPOD easyPOD;
        private bool isConnected = false;
        public event EventHandler<string>? DataReceived;      

        public unsafe void Disconnect()
        {
            if (isConnected)
            {
                try
                {
                    fixed (MW_EasyPOD* pEasyPOD = &easyPOD)
                    {
                        PODfuncs.DisconnectPOD(pEasyPOD);
                    }
                }
                catch { }
            }
            isConnected = false;
        }

        // 補助函式
        static byte[] HexStringToByteArray(string hex)
        {
            hex = hex.Replace(" ", "");
            if (hex.Length % 2 != 0) return null;
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return bytes;
        }

        // 根據 RD300 Protocol Manual 實現讀卡功能 (Command 0x01)
        // 嚴格按照官方文檔格式
        public unsafe string ReadCardData(bool autoEraseAfterRemove = false)
        {
            UInt32 uiLength = 64, uiRead = 0, uiWritten = 0;
            uint dwResult;

            // 準備 RD300 讀卡指令 - 完全按照手冊格式
            byte[] WriteBuffer;

            if (autoEraseAfterRemove)
            {
                // 手冊格式: <STX> [02] [01] [01] - 讀卡並在移除卡片後清除
                WriteBuffer = new byte[4];
                WriteBuffer[0] = 0x02;  // STX (Start of Transmission)
                WriteBuffer[1] = 0x02;  // LEN (Message Length, not including STX and LEN)
                WriteBuffer[2] = 0x01;  // COMMAND (Read Tag Data)
                WriteBuffer[3] = 0x01;  // DATA (auto-erase after remove card)
            }
            else
            {
                // 手冊格式: <STX> [01] [01] - 讀卡並在移除卡片前清除
                WriteBuffer = new byte[3];
                WriteBuffer[0] = 0x02;  // STX (Start of Transmission)
                WriteBuffer[1] = 0x01;  // LEN (Message Length, not including STX and LEN)
                WriteBuffer[2] = 0x01;  // COMMAND (Read Tag Data)
            }

            // 設備連接參數 - 使用手冊中的 VID/PID
            easyPOD.VID = 0x0E6A;
            easyPOD.PID = 0x0317;
            uint Index = 1;

            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                dwResult = PODfuncs.ConnectPOD(pPOD, Index);

                // 不依賴 dwResult 來判斷，直接嘗試操作
                try
                {
                    easyPOD.ReadTimeOut = 2000;   // 2秒等待卡片
                    easyPOD.WriteTimeOut = 1000;  // 1秒寫入超時

                    // 發送讀卡指令
                    dwResult = PODfuncs.WriteData(pPOD, WriteBuffer, (uint)WriteBuffer.Length, &uiWritten);

                    // 讀取回應 - 使用較小的緩衝區
                    byte[] ReadBuffer = new byte[64];
                    uiRead = 0;
                    uiLength = (uint)ReadBuffer.Length;
                    dwResult = PODfuncs.ReadData(pPOD, ReadBuffer, uiLength, &uiRead);

                    if (uiRead < 4)
                    {
                        return $"回應資料不足，只收到 {uiRead} 位元組";
                    }

                    // 解析 RD300 回應格式
                    return ParseRD300Response(ReadBuffer, (int)uiRead);
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
        }

        // 解析 RD300 讀卡回應 - 嚴格按照官方文檔
        private string ParseRD300Response(byte[] response, int length)
        {
            // 先顯示原始回應數據用於調試
            string rawHex = BitConverter.ToString(response, 0, Math.Min(length, 16)).Replace("-", " ");
            string debugInfo = $"原始回應 ({length} bytes): {rawHex}\n";

            if (length < 4)
            {
                return debugInfo + "回應資料長度不足，至少需要4位元組";
            }

            byte stx = response[0];
            byte len = response[1];
            byte cmd = response[2];
            byte status = response[3];

            debugInfo += $"STX=0x{stx:X2}, LEN=0x{len:X2}, CMD=0x{cmd:X2}, STATUS=0x{status:X2}\n";

            // 驗證 STX
            if (stx != 0x02)
            {
                return debugInfo + $"錯誤：STX 應為 0x02，實際為 0x{stx:X2}";
            }

            // 驗證命令回應
            if (cmd != 0x01)
            {
                return debugInfo + $"錯誤：命令回應應為 0x01，實際為 0x{cmd:X2}";
            }

            // 根據 RD300 Protocol Manual 第9頁解析狀態
            switch (status)
            {
                case 0x10:
                    return debugInfo + "指令錯誤 (Command error)";

                case 0x01:
                    return debugInfo + "沒有偵測到卡片 (No card)";

                case 0x00:
                    // 成功讀取 - 手冊指出格式為: <STX> [0A] [01] [00] [XX]*8
                    if (len == 0x0A)
                    {
                        if (length >= 12) // STX(1) + LEN(1) + CMD(1) + STATUS(1) + DATA(8) = 12
                        {
                            // 8 位元組卡片資料從索引 4 開始
                            byte[] cardData = new byte[8];
                            Array.Copy(response, 4, cardData, 0, 8);

                            string hexWithSpaces = BitConverter.ToString(cardData).Replace("-", " ");
                            string hexNoSpaces = BitConverter.ToString(cardData).Replace("-", "");

                            // 檢查是否有更多資料可用
                            string additionalData = "";
                            if (length > 12)
                            {
                                int extraBytes = length - 12;
                                byte[] extraData = new byte[extraBytes];
                                Array.Copy(response, 12, extraData, 0, extraBytes);
                                additionalData = BitConverter.ToString(extraData).Replace("-", "");
                            }

                            // 組合完整結果，類似範例格式
                            string fullResult = hexNoSpaces + additionalData;

                            return debugInfo +
                                   $"✅ 讀取成功!\n" +
                                   $"卡片資料 (有空格): {hexWithSpaces}\n" +
                                   $"卡片資料 (無空格): {hexNoSpaces}\n" +
                                   $"完整結果: {fullResult}\n" +
                                   $"期望格式結果: {fullResult}";
                        }
                        else
                        {
                            return debugInfo + $"資料不完整：預期12位元組，實際收到{length}位元組";
                        }
                    }
                    else
                    {
                        return debugInfo + $"長度錯誤：成功回應的LEN應為0x0A，實際為0x{len:X2}";
                    }

                default:
                    return debugInfo + $"未知狀態碼: 0x{status:X2}";
            }
        }

        private string ParseMifareBlockResponse(byte[] response, int length)
        {
            if (length < 4) return "回應資料不足";

            byte stx = response[0];
            byte len = response[1];
            byte cmd = response[2];
            byte status = response[3];

            string debugInfo = $"回應解析: STX:{stx:X2} LEN:{len:X2} CMD:{cmd:X2} STATUS:{status:X2}\n";

            if (stx == 0x02 && cmd == 0x15)
            {
                if (status == 0x00)
                {
                    // 根據手冊：成功回應格式為 <STX> [12] [15] [00] [資料]*16
                    if (len == 0x12 && length >= 20) // STX(1) + LEN(1) + CMD(1) + STATUS(1) + DATA(16) = 20
                    {
                        byte[] blockData = new byte[16];
                        Array.Copy(response, 4, blockData, 0, 16);
                        string result = BitConverter.ToString(blockData).Replace("-", "");
                        return result;
                    }
                    else
                    {
                        // 嘗試解析其他長度的回應
                        int dataLength = len - 2; // 扣除 CMD 和 STATUS
                        if (dataLength > 0 && length >= (4 + dataLength))
                        {
                            byte[] blockData = new byte[dataLength];
                            Array.Copy(response, 4, blockData, 0, dataLength);
                            string result = BitConverter.ToString(blockData).Replace("-", "");
                            return debugInfo + $"✅ 區塊資料 ({dataLength}字節): {result}\n" +
                                   $"期望16字節，但得到{dataLength}字節";
                        }
                        else
                        {
                            return debugInfo + $"資料長度不符: LEN={len:X2}，期望0x12 (18)，實際收到{length}字節";
                        }
                    }
                }
                else if (status == 0x01)
                {
                    return debugInfo + "❌ 無卡片或無效金鑰 (手冊狀態碼0x01)";
                }
                else if (status == 0x10)
                {
                    return debugInfo + "❌ 指令錯誤 (手冊狀態碼0x10)";
                }
                else
                {
                    return debugInfo + $"❌ 未知錯誤狀態: {status:X2}";
                }
            }

            return debugInfo + "❌ 回應格式不正確";
        }

        public unsafe string ReadMifareSpecificBlock(int sector, int block, string keyType, string loadKey)
        {
            UInt32 uiLength = 64, uiRead = 0, uiWritten = 0;
            uint dwResult;

            // 轉換金鑰字符串為字節陣列
            byte[] keyBytes = HexStringToByteArray(loadKey);
            if (keyBytes == null || keyBytes.Length != 6)
            {
                return "金鑰格式錯誤";
            }

            byte[] WriteBuffer = new byte[12];
            WriteBuffer[0] = 0x02;  // STX
            WriteBuffer[1] = 0x0A;  // LEN
            WriteBuffer[2] = 0x15;  // COMMAND

            // 根據 keyType 參數設定正確的金鑰類型
            WriteBuffer[3] = (byte)(keyType == "A" ? 0x60 : 0x61); // Key A = 0x60, Key B = 0x61

            // 金鑰6字節
            Array.Copy(keyBytes, 0, WriteBuffer, 4, 6);
            WriteBuffer[10] = (byte)sector; // 扇區號碼
            WriteBuffer[11] = (byte)block;  // 區塊號碼

            easyPOD.VID = 0x0E6A;
            easyPOD.PID = 0x0317;
            uint Index = 1;

            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                dwResult = PODfuncs.ConnectPOD(pPOD, Index);

                try
                {
                    easyPOD.ReadTimeOut = 2000;
                    easyPOD.WriteTimeOut = 1000;

                    dwResult = PODfuncs.WriteData(pPOD, WriteBuffer, (uint)WriteBuffer.Length, &uiWritten);

                    byte[] ReadBuffer = new byte[64];
                    uiRead = 0;
                    uiLength = (uint)ReadBuffer.Length;
                    dwResult = PODfuncs.ReadData(pPOD, ReadBuffer, uiLength, &uiRead);

                    if (uiRead >= 4)
                    {
                        byte status = ReadBuffer[3];
                        if (status == 0x00)
                        {
                            string rawHex = BitConverter.ToString(ReadBuffer, 0, (int)uiRead).Replace("-", " ");
                            return ParseMifareBlockResponse(ReadBuffer, (int)uiRead);
                        }
                        else if (status == 0x01)
                        {
                            return $"❌ 扇區{sector}區塊{block} 無卡片或金鑰{keyType}無效";
                        }
                        else
                        {
                            return $"❌ 扇區{sector}區塊{block} 金鑰{keyType} 錯誤狀態: 0x{status:X2}";
                        }
                    }
                    else
                    {
                        return $"❌ 扇區{sector}區塊{block} 金鑰{keyType} 回應資料不足";
                    }
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
        }
    }
}