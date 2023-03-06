using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace EzTCP
{
    
    /// <summary>
    /// 基於帶有HeadInfo溝通的標準data類
    /// </summary>
    public sealed class MessageProtocol
    {
        public const int BUFFERSIZE = 65536;
        private HeadInfo _headInfo = null;
        /// <summary>
        /// 包含command、param、dataLength
        /// </summary>
        public HeadInfo headInfo { get { return _headInfo; } }
        private byte[] _messageData = new byte[0];
        /// <summary>
        /// 實際的data byte array
        /// </summary>
        public byte[] messageData { get => _messageData; }

        private byte[] _moreData = new byte[0];
        /// <summary>
        /// 超過headinfo中所記載的datalength後的data，用於與後續的包合併
        /// </summary>
        public byte[] moreData { get=> _moreData; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer">包含headInfo與data本身的bytes</param>
        public MessageProtocol(byte[] buffer) 
        {
            //如果buffer長度還不到header 代表資料一定不完整 可以再等等
            if (buffer==null||buffer.Length < HeadInfo.HEADLENGTH) 
            {
                return;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            try
            {
                //把Head取出來
                _headInfo = new HeadInfo(br.ReadByte(), br.ReadByte(), br.ReadInt32());

                //如果buffer的長度扣掉head後 大於等於datalength
                //代表buffer有完整的data  可以把這些data讀出來
                if (buffer.Length - HeadInfo.HEADLENGTH >= _headInfo.dataLength) 
                {
                    _messageData = br.ReadBytes(_headInfo.dataLength);
                }

                //如果buffer的長度扣掉head後大於datalength
                //代表buffer中的資料不只有head+data 所以要把多餘的東西放在下一包中繼續使用
                if (buffer.Length - HeadInfo.HEADLENGTH > _headInfo.dataLength) 
                {
                    _moreData = br.ReadBytes(buffer.Length - HeadInfo.HEADLENGTH - _headInfo.dataLength);
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {
                br.Close();
                ms.Close();
            }


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command">命令byte</param>
        /// <param name="param">附帶參數byte</param>
        /// <param name="dataLength">data實際長度</param>
        /// <param name="messageData">此次塞入的data內容</param>
        public MessageProtocol(byte command, byte param, int dataLength, byte[] messageData)
        {
            _headInfo = new HeadInfo(command, param, dataLength);
            _messageData = messageData;
        }

        /// <summary>
        /// 把這包data轉成bytes
        /// </summary>
        /// <returns></returns>
        public byte[] GetThisBytes()
        {
            byte[] bytes = null;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            try
            {
                bw.Write(headInfo.command);
                bw.Write(headInfo.param);
                bw.Write(headInfo.dataLength);
                bw.Write(_messageData);

                bytes = ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {
                bw.Close();
                ms.Close();
            }            
            return bytes;
        }

        /// <summary>
        /// 回傳將命令、附帶參數與資料本體直接合併成的bytes
        /// </summary>
        /// <param name="command">命令</param>
        /// <param name="param">附帶參數</param>
        /// <param name="messageData">資料本體</param>
        /// <returns></returns>
        public static byte[] GetBytes(byte command, byte param, byte[] messageData) 
        {
            byte[] bytes = null;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            try
            {
                bw.Write(command);
                bw.Write(param);
                bw.Write(messageData.Length);
                bw.Write(messageData);

                bytes = ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                bw.Close();
                ms.Close();
            }
            return bytes;
        }
        
        /// <summary>
        /// 將兩包byteS黏在一起
        /// </summary>
        /// <param name="frontBytes"></param>
        /// <param name="EndBytes"></param>
        /// <returns></returns>
        public static byte[] CombineBytes(byte[] frontBytes, byte[] EndBytes,int receiveLength) 
        {
            byte[] bytes = null;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            try
            {
                bw.Write(frontBytes, 0, frontBytes.Length);
                bw.Write(EndBytes, 0, receiveLength);
                bytes = ms.ToArray();
                return bytes;
            }
            catch (Exception)
            {
                throw;
            }
            finally 
            {
                bw.Close();
                ms.Close();
            }
        }

        
        /// <summary>
        /// 將此包當作string輸出
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return Encoding.UTF8.GetString(messageData, 0, headInfo.dataLength);
        }

        /// <summary>
        /// 將此包做為一個file輸出儲存
        /// </summary>
        /// <param name="fileName">檔案名稱</param>
        /// <param name="path">儲存路徑</param>
        /// <returns></returns>
        public bool SaveAsFile(string fileName, string path) 
        {
            FileInfo fi = new FileInfo(fileName);
            FileStream fs = new FileStream(path + '/' + fi.Name, FileMode.Create, FileAccess.ReadWrite);

            try
            {
                fs.Write(messageData, 0, headInfo.dataLength);                
            }
            catch (Exception)
            {
                return false;
            }
            finally 
            {
                fs.Close();
            }
            return true;
        }
    }

    /// <summary>
    /// 帶有命令、附帶參數、資料長度的標頭
    /// </summary>
    public sealed class HeadInfo 
    {
        /// <summary>
        /// byte[0] command byte 
        /// byte[1] command param byte
        /// byte[2~5] 接下來的data length
        /// </summary>
        public const int HEADLENGTH = 6;

        private byte _command = 0;
        /// <summary>
        /// 命令
        /// </summary>
        public byte command { get => _command; }
        private byte _param = 0;
        /// <summary>
        /// 附帶參數
        /// </summary>
        public byte param { get => _param; }
        private int _dataLength = 0;
        /// <summary>
        /// 後續的data長度
        /// </summary>
        public int dataLength { get => _dataLength; }

        public HeadInfo(byte command, byte param, int dataLength)
        {
            _command = command;
            _param = param;
            _dataLength = dataLength;
        }

        /// <summary>
        /// 將bytes轉乘HeadInfo
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        public static HeadInfo GetHeadInfo(byte[] buffer)
        {
            if (buffer ==null ||buffer.Length < HeadInfo.HEADLENGTH)
            {
                return new HeadInfo(0, 0, 0);
            }

            return new HeadInfo(buffer[0], buffer[1], BitConverter.ToInt32(buffer, 2));
        }


    }
}
