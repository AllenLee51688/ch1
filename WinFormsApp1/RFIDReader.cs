using System.Text;

namespace WinFormsApp1
{
    public class RFIDReader
    {
        private MW_EasyPOD easyPOD;
        private bool isConnected = false;

        public bool IsConnected => isConnected;

        public event EventHandler<string>? DataReceived;

        public unsafe bool Connect(uint deviceIndex = 0)
        {
            try
            {
                easyPOD.VID = 0xe6a;           // 設定正確的設備VID
                easyPOD.PID = 0x317;           // 設定正確的設備PID

                fixed (MW_EasyPOD* pEasyPOD = &easyPOD)
                {
                    uint result = PODfuncs.ConnectPOD(pEasyPOD, deviceIndex);
                    if (result == 0) // 0表示成功
                    {
                        // 連接成功後設定超時值
                        easyPOD.ReadTimeOut = 200;
                        easyPOD.WriteTimeOut = 200;
                        isConnected = true;
                        return true;
                    }
                }

                isConnected = false;
                return false;
            }
            catch
            {
                isConnected = false;
                return false;
            }
        }

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

        public unsafe string ReadMifareBlock2(int sector, int block, string keyType, string key)
        {
            UInt32 uiLength, uiRead, uiResult, uiWritten;
            UInt32 dwResult;
            byte[] ReadBuffer = new byte[0x40];

            // 根據RD300工具程式的TX命令格式: 02 04 15 60 FF FF FF FF FF FF 03 03
            byte[] WriteBuffer = new byte[12];
            WriteBuffer[0] = 0x02; // STX
            WriteBuffer[1] = 0x04; // 命令代碼？
            WriteBuffer[2] = 0x15; // MIFARE命令
            WriteBuffer[3] = (byte)(keyType == "A" ? 0x60 : 0x61); // Key type (0x60=Key A, 0x61=Key B)

            // 添加6字節的Key
            byte[] keyBytes = HexStringToByteArray(key);
            for (int i = 0; i < 6; i++)
            {
                WriteBuffer[4 + i] = keyBytes[i];
            }

            // 最後兩個字節，根據截圖是 03 03，可能跟sector/block有關
            WriteBuffer[10] = (byte)sector;
            WriteBuffer[11] = (byte)block;

            easyPOD.VID = 0xe6a;
            easyPOD.PID = 0x317;
            uint Index = 1;
            uiLength = 64;

            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                dwResult = PODfuncs.ConnectPOD(pPOD, Index);

                if ((dwResult != 0))
                {
                    return "Not connected yet";
                }
                else
                {
                    easyPOD.ReadTimeOut = 200;
                    easyPOD.WriteTimeOut = 200;

                    dwResult = PODfuncs.WriteData(pPOD, WriteBuffer, (uint)WriteBuffer.Length, &uiWritten);
                    if (dwResult != 0)
                    {
                        PODfuncs.ClearPODBuffer(pPOD);
                        PODfuncs.DisconnectPOD(pPOD);
                        return $"Write command failed: {dwResult}";
                    }

                    uiResult = PODfuncs.ReadData(pPOD, ReadBuffer, uiLength, &uiRead);
                    if (uiResult != 0)
                    {
                        PODfuncs.ClearPODBuffer(pPOD);
                        PODfuncs.DisconnectPOD(pPOD);
                        return $"Read data failed: {uiResult}";
                    }

                    // 根據RX回應解析資料
                    if (uiRead >= 4)
                    {
                        string hexResult = BitConverter.ToString(ReadBuffer, 4, (Int32)uiRead - 4).Replace("-", " ");

                        PODfuncs.ClearPODBuffer(pPOD);
                        PODfuncs.DisconnectPOD(pPOD);

                        return $"MIFARE Data: {hexResult}";
                    }
                    else
                    {
                        PODfuncs.ClearPODBuffer(pPOD);
                        PODfuncs.DisconnectPOD(pPOD);
                        return "No data received";
                    }
                }
            }
        }


        private byte[] HexStringToByteArray2(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("十六進制字串長度必須為偶數");

            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }

        private string ByteArrayToHexString(byte[] bytes)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        public unsafe void ClearBuffer()
        {
            if (isConnected)
            {
                try
                {
                    fixed (MW_EasyPOD* pEasyPOD = &easyPOD)
                    {
                        PODfuncs.ClearPODBuffer(pEasyPOD);
                    }
                }
                catch { }
            }
        }

        public unsafe string ReadMifareBlock(int sector, int block, string keyType, string key)
        {
            UInt32 uiLength = 1024, uiRead = 0, uiWritten = 0;
            uint dwResult;

            // 準備 WriteBuffer -> 使用你從 Tool 拿到的範例 TX
            byte[] WriteBuffer = new byte[12];
            WriteBuffer[0] = 0x02;                 // STX
            WriteBuffer[1] = 0x0A;                 // LEN (工具範例為 0x0A)
            WriteBuffer[2] = 0x15;                 // CMD (MIFARE)
            WriteBuffer[3] = (byte)(keyType == "A" ? 0x60 : 0x61); // KeyType
            byte[] keyBytes = HexStringToByteArray(key); // 6 bytes
            if (keyBytes == null || keyBytes.Length < 6) return "Invalid key";
            for (int i = 0; i < 6; i++) WriteBuffer[4 + i] = keyBytes[i];
            WriteBuffer[10] = 0x03; // 根據工具，這兩位是 03 03 (尾)
            WriteBuffer[11] = 0x03;

            // 如果你要把 sector/block 傳進去，請確認協定格式是否真的要放在這兩個位子
            // 工具範例沒有把 sector/block 放在 TX 的固定位置，若協定要加入請依文件修改

            // prepare device struct
            easyPOD.VID = 0x0E6A;
            easyPOD.PID = 0x0317;
            uint Index = 1;

            fixed (MW_EasyPOD* pPOD = &easyPOD)
            {
                dwResult = PODfuncs.ConnectPOD(pPOD, Index);
                if (dwResult != 0)
                {
                    return $"ConnectPOD failed: 0x{dwResult:X}";
                }

                try
                {
                    easyPOD.ReadTimeOut = 1000;   // ms：適當放大以免逾時
                    easyPOD.WriteTimeOut = 1000;

                    // 寫入 TX
                    dwResult = PODfuncs.WriteData(pPOD, WriteBuffer, (uint)WriteBuffer.Length, &uiWritten);
                    if (dwResult != 0)
                    {
                        return $"WriteData failed: 0x{dwResult:X}";
                    }

                    // 讀取 RX
                    byte[] ReadBuffer = new byte[2048];
                    uiRead = 0;
                    uiLength = (uint)ReadBuffer.Length;
                    dwResult = PODfuncs.ReadData(pPOD, ReadBuffer, uiLength, &uiRead);//3758096392

                    if (dwResult != 0)
                    {
                        return $"ReadData failed: 0x{dwResult:X}";
                    }

                    if (uiRead < 2)
                    {
                        return "No valid response (too short)";
                    }

                    // 依 LEN field 解析 payload
                    int totalRead = (int)uiRead;
                    byte stx = ReadBuffer[0];
                    byte lenField = ReadBuffer[1]; // e.g. 0x12
                    int expectedTotal = 2 + lenField; // STX + LEN + payloadLen
                    if (totalRead < expectedTotal)
                    {
                        // 還是拿到資料，但比 LEN 指示少，我們仍嘗試取現有的
                    }

                    // 取得 payload (從 index 2 開始，長度 lenField 或到 uiRead)
                    int payloadLen = Math.Min(lenField, totalRead - 2);
                    byte[] payload = new byte[payloadLen];
                    Array.Copy(ReadBuffer, 2, payload, 0, payloadLen);

                    // 將 payload 轉 HEX 與 ASCII（方便檢查）
                    string hexPayload = BitConverter.ToString(payload).Replace("-", " ");
                    string asciiPayload = BytesToAscii(payload);

                    // 嘗試取出最後 16 bytes 當 MIFARE block data（常見情形）
                    string blockHex = "";
                    if (payloadLen >= 16)
                    {
                        byte[] last16 = new byte[16];
                        Array.Copy(payload, payloadLen - 16, last16, 0, 16);
                        blockHex = BitConverter.ToString(last16).Replace("-", " ");
                    }

                    return $"RX totalRead={totalRead} lenField={lenField} payloadLen={payloadLen}\nHEX payload: {hexPayload}\nASCII payload: {asciiPayload}\nAssumed block (last16): {blockHex}";
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
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

        static string BytesToAscii(byte[] b)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var x in b)
            {
                sb.Append(x >= 32 && x <= 126 ? (char)x : '.');
            }
            return sb.ToString();
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

        // 根據RD300手冊第22頁 - Read Mifare UID (0x11)
        public unsafe string ReadMifareUID()
        {
            UInt32 uiLength = 64, uiRead = 0, uiWritten = 0;
            uint dwResult;

            // 手冊格式: <STX> [01] [11] - Read Mifare card UID
            byte[] WriteBuffer = new byte[3];
            WriteBuffer[0] = 0x02;  // STX
            WriteBuffer[1] = 0x01;  // LEN
            WriteBuffer[2] = 0x11;  // COMMAND (Read Mifare UID)

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

                    if (uiRead < 4)
                    {
                        return $"Read Mifare UID 回應資料不足，只收到 {uiRead} 位元組";
                    }

                    return ParseMifareUIDResponse(ReadBuffer, (int)uiRead);
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
        }

        // 解析 Mifare UID 回應
        private string ParseMifareUIDResponse(byte[] response, int length)
        {
            string rawHex = BitConverter.ToString(response, 0, Math.Min(length, 16)).Replace("-", " ");
            string debugInfo = $"Mifare UID 原始回應 ({length} bytes): {rawHex}\n";

            if (length < 4) return debugInfo + "回應資料長度不足";

            byte stx = response[0];
            byte len = response[1];
            byte cmd = response[2];
            byte status = response[3];

            debugInfo += $"STX=0x{stx:X2}, LEN=0x{len:X2}, CMD=0x{cmd:X2}, STATUS=0x{status:X2}\n";

            if (stx != 0x02 || cmd != 0x11)
            {
                return debugInfo + "格式錯誤";
            }

            switch (status)
            {
                case 0x10:
                    return debugInfo + "指令錯誤";
                case 0x01:
                    return debugInfo + "沒有卡片";
                case 0x00:
                    // 成功 - 手冊: <STX> [06] [11] [00] [Data]*4
                    if (len >= 0x04 && length >= 8)
                    {
                        int dataLen = len - 2; // 扣除 CMD 和 STATUS
                        byte[] cardData = new byte[dataLen];
                        Array.Copy(response, 4, cardData, 0, dataLen);

                        string result = BitConverter.ToString(cardData).Replace("-", "");
                        return debugInfo + $"✅ Mifare UID: {result}";
                    }
                    break;
            }

            return debugInfo + $"未知回應格式";
        }

        // 根據手冊第3.5節正確實現 Mifare Read Data (0x15)
        public unsafe string ReadMifareBlock0()
        {
            UInt32 uiLength = 64, uiRead = 0, uiWritten = 0;
            uint dwResult;

            // 手冊格式：<STX> [0A] [15] [金鑰類型] + [金鑰值] + [扇區號碼] + [區塊號碼]
            byte[] WriteBuffer = new byte[12];
            WriteBuffer[0] = 0x02;  // STX
            WriteBuffer[1] = 0x0A;  // LEN (10 bytes following)
            WriteBuffer[2] = 0x15;  // COMMAND (Read Data)
            WriteBuffer[3] = 0x60;  // Key Type (Key A = 0x60, Key B = 0x61)
            // 金鑰值 FF FF FF FF FF FF (6 bytes)
            WriteBuffer[4] = 0xFF;
            WriteBuffer[5] = 0xFF;
            WriteBuffer[6] = 0xFF;
            WriteBuffer[7] = 0xFF;
            WriteBuffer[8] = 0xFF;
            WriteBuffer[9] = 0xFF;
            WriteBuffer[10] = 0x00; // 扇區號碼 (Sector 0)
            WriteBuffer[11] = 0x00; // 區塊號碼 (Block 0)

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

                    string rawHex = BitConverter.ToString(ReadBuffer, 0, Math.Min((int)uiRead, 32)).Replace("-", " ");
                    return ParseMifareBlockResponse(ReadBuffer, (int)uiRead);
                    //return $"Mifare Block 0 原始回應 ({uiRead} bytes): {rawHex}\n" +
                    //       ParseMifareBlockResponse(ReadBuffer, (int)uiRead);
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
        }


        private unsafe string ReadMifareBlockWithKey(string keyString)
        {
            UInt32 uiLength = 64, uiRead = 0, uiWritten = 0;
            uint dwResult;

            // 轉換金鑰字符串為字節陣列
            byte[] keyBytes = HexStringToByteArray(keyString);
            if (keyBytes == null || keyBytes.Length != 6)
            {
                return "金鑰格式錯誤";
            }

            byte[] WriteBuffer = new byte[12];
            WriteBuffer[0] = 0x02;  // STX
            WriteBuffer[1] = 0x0A;  // LEN
            WriteBuffer[2] = 0x15;  // COMMAND
            WriteBuffer[3] = 0x60;  // Key A
            // 金鑰6字節
            Array.Copy(keyBytes, 0, WriteBuffer, 4, 6);
            WriteBuffer[10] = 0x00; // 扇區號碼 (Sector 0)
            WriteBuffer[11] = 0x00; // 區塊號碼 (Block 0)

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
                            return $"✅ 成功！原始回應: {rawHex}\n" +
                                   ParseMifareBlockResponse(ReadBuffer, (int)uiRead);
                        }
                        else if (status == 0x01)
                        {
                            return "❌ 無卡片或金鑰無效";
                        }
                        else
                        {
                            return $"❌ 錯誤狀態: 0x{status:X2}";
                        }
                    }
                    else
                    {
                        return "❌ 回應資料不足";
                    }
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
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

                        //return debugInfo + $"✅ 完整 MIFARE 區塊資料 (16字節): {result}\n" +
                        //       $"這就是你要的格式！與 {result} 比較";
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

        // 簡化的讀卡方法，自動處理連接
        public string ReadCard()
        {
            return ReadCardData(false);  // 預設使用移除前清除模式
        }

        // 嘗試多種方法讀取完整資料
        public string ReadCardComplete()
        {
            string result = "";
            int block = 0;
            for (int sector = 0; sector < 16; sector++)
            {
                for (block = 0; block < 4; block++)
                {
                    // 跳過控制區塊
                    //if (block == 3) continue;
                    //result += $"--- 扇區 {sector} 區塊 {block} ---\n";
                    result += "("+sector+"/"+ block+")"+ ReadMifareSpecificBlock(sector, block) + "\r\n";
                }
            }

            return result;
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

        public unsafe string ReadMifareSpecificBlock(int sector, int block, string keyString = "FFFFFFFFFFFF")
        {
            UInt32 uiLength = 64, uiRead = 0, uiWritten = 0;
            uint dwResult;

            // 轉換金鑰字符串為字節陣列
            byte[] keyBytes = HexStringToByteArray(keyString);
            if (keyBytes == null || keyBytes.Length != 6)
            {
                return "金鑰格式錯誤";
            }

            byte[] WriteBuffer = new byte[12];
            WriteBuffer[0] = 0x02;  // STX
            WriteBuffer[1] = 0x0A;  // LEN
            WriteBuffer[2] = 0x15;  // COMMAND
            WriteBuffer[3] = 0x60;  // Key A
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
                            return $"❌ 扇區{sector}區塊{block} 無卡片或金鑰無效";
                        }
                        else
                        {
                            return $"❌ 扇區{sector}區塊{block} 錯誤狀態: 0x{status:X2}";
                        }
                    }
                    else
                    {
                        return $"❌ 扇區{sector}區塊{block} 回應資料不足";
                    }
                }
                finally
                {
                    PODfuncs.ClearPODBuffer(pPOD);
                    PODfuncs.DisconnectPOD(pPOD);
                }
            }
        }

        //public unsafe string ReadAllMifareData(string keyString = "FFFFFFFFFFFF")
        //{
        //    StringBuilder allData = new StringBuilder();
        //    StringBuilder resultLog = new StringBuilder();
        //    int successfulReads = 0;
        //    int totalBlocks = 0;

        //    resultLog.AppendLine("=== 讀取整張 MIFARE 卡片資料 ===");
        //    resultLog.AppendLine($"使用金鑰: {keyString}");
        //    resultLog.AppendLine();

        //    // MIFARE Classic 1K: 16個扇區，每個扇區4個區塊
        //    for (int sector = 0; sector < 16; sector++)
        //    {
        //        resultLog.AppendLine($"--- 扇區 {sector} ---");

        //        for (int block = 0; block < 4; block++)
        //        {
        //            totalBlocks++;

        //            // 區塊3是控制區塊，通常無法讀取，但仍嘗試
        //            if (block == 3)
        //            {
        //                resultLog.AppendLine($"區塊 {block} (控制區塊): 跳過讀取");
        //                continue;
        //            }

        //            try
        //            {
        //                string blockData = ReadSpecificMifareBlock(sector, block, keyString);

        //                if (!string.IsNullOrEmpty(blockData) && !blockData.Contains("❌") && !blockData.Contains("錯誤"))
        //                {
        //                    allData.Append(blockData);
        //                    successfulReads++;
        //                    resultLog.AppendLine($"區塊 {block}: {blockData}");
        //                }
        //                else
        //                {
        //                    resultLog.AppendLine($"區塊 {block}: 讀取失敗 - {blockData}");
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                resultLog.AppendLine($"區塊 {block}: 例外錯誤 - {ex.Message}");
        //            }
        //        }
        //        resultLog.AppendLine();
        //    }

        //    resultLog.AppendLine("=== 讀取結果統計 ===");
        //    resultLog.AppendLine($"成功讀取區塊數: {successfulReads}/{totalBlocks - 16}"); // 扣除16個控制區塊
        //    resultLog.AppendLine($"總資料長度: {allData.Length} 字元 ({allData.Length / 2} 位元組)");
        //    resultLog.AppendLine();
        //    resultLog.AppendLine("=== 完整資料 (連續) ===");
        //    resultLog.AppendLine(allData.ToString());

        //    return resultLog.ToString();
        //}
        //// 輔助方法：讀取特定扇區和區塊
        //private unsafe string ReadSpecificMifareBlock(int sector, int block, string keyString)
        //{
        //    UInt32 uiLength = 64, uiRead = 0, uiWritten = 0;
        //    uint dwResult;

        //    // 轉換金鑰字符串為字節陣列
        //    byte[] keyBytes = HexStringToByteArray(keyString);
        //    if (keyBytes == null || keyBytes.Length != 6)
        //    {
        //        return "金鑰格式錯誤";
        //    }

        //    byte[] WriteBuffer = new byte[12];
        //    WriteBuffer[0] = 0x02;  // STX
        //    WriteBuffer[1] = 0x0A;  // LEN
        //    WriteBuffer[2] = 0x15;  // COMMAND
        //    WriteBuffer[3] = 0x60;  // Key A
        //                            // 金鑰6字節
        //    Array.Copy(keyBytes, 0, WriteBuffer, 4, 6);
        //    WriteBuffer[10] = (byte)sector; // 扇區號碼
        //    WriteBuffer[11] = (byte)block;  // 區塊號碼

        //    easyPOD.VID = 0x0E6A;
        //    easyPOD.PID = 0x0317;
        //    uint Index = 1;

        //    fixed (MW_EasyPOD* pPOD = &easyPOD)
        //    {
        //        dwResult = PODfuncs.ConnectPOD(pPOD, Index);

        //        try
        //        {
        //            easyPOD.ReadTimeOut = 1000;  // 縮短超時時間，加快讀取速度
        //            easyPOD.WriteTimeOut = 1000;

        //            dwResult = PODfuncs.WriteData(pPOD, WriteBuffer, (uint)WriteBuffer.Length, &uiWritten);
        //            if (dwResult != 0)
        //            {
        //                return $"寫入失敗: 0x{dwResult:X}";
        //            }

        //            byte[] ReadBuffer = new byte[64];
        //            uiRead = 0;
        //            uiLength = (uint)ReadBuffer.Length;
        //            dwResult = PODfuncs.ReadData(pPOD, ReadBuffer, uiLength, &uiRead);

        //            if (dwResult != 0)
        //            {
        //                return $"讀取失敗: 0x{dwResult:X}";
        //            }

        //            if (uiRead >= 4)
        //            {
        //                byte status = ReadBuffer[3];
        //                if (status == 0x00)
        //                {
        //                    // 成功讀取，提取16位元組資料
        //                    if (uiRead >= 20) // STX(1) + LEN(1) + CMD(1) + STATUS(1) + DATA(16) = 20
        //                    {
        //                        byte[] blockData = new byte[16];
        //                        Array.Copy(ReadBuffer, 4, blockData, 0, 16);
        //                        return BitConverter.ToString(blockData).Replace("-", "");
        //                    }
        //                    else
        //                    {
        //                        return "資料長度不足";
        //                    }
        //                }
        //                else if (status == 0x01)
        //                {
        //                    return "無卡片或金鑰無效";
        //                }
        //                else
        //                {
        //                    return $"錯誤狀態: 0x{status:X2}";
        //                }
        //            }
        //            else
        //            {
        //                return "回應資料不足";
        //            }
        //        }
        //        finally
        //        {
        //            PODfuncs.ClearPODBuffer(pPOD);
        //            PODfuncs.DisconnectPOD(pPOD);
        //        }
        //    }
        //}

    }
}