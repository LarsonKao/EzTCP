using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.IO;

namespace EzTCP
{

    /// <summary>
    /// Tcp的send data的基類
    /// </summary>
    public class TcpCommonBase
    {
        /// <summary>
        /// 傳送byte data，不一定有包含headinfo
        /// </summary>
        /// <param name="name"></param>
        public void SendData(NetworkStream stream,byte[] data)
        {
            stream.Write(data, 0, data.Length);
        }

        /// <summary>
        /// 傳送包括HeadInfo的標準格式Data
        /// </summary>
        /// <param name="stream">接收方的NetworkStream</param>
        /// <param name="command">命令 一個byte</param>
        /// <param name="param">命令附帶參數 一個byte</param>
        /// <param name="data">實際傳送data的byte array</param>
        /// <returns></returns>
        public bool SendDataWithCmd(NetworkStream stream, byte command, byte param, byte[] data)
        {
            try
            {
                byte[] bytes = MessageProtocol.GetBytes(command, param, data);
                stream.Write(bytes, 0, bytes.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 傳送接下來要傳送的檔案的總數
        /// </summary>
        /// <param name="stream">接收方的NetworkStream</param>
        /// <param name="count">檔案總數</param>
        /// <param name="command">命令 一個byte</param>
        /// <param name="param">命令附帶參數 一個byte</param>
        /// <returns></returns>
        public bool SendFileCount(NetworkStream stream, int count, byte command, byte param)
        {
            try
            {
                byte[] fileCount = Encoding.UTF8.GetBytes(count.ToString());
                byte[] data = MessageProtocol.GetBytes(command, param, fileCount);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 傳送接下來要傳送的檔案名稱
        /// </summary>
        /// <param name="stream">接收方的NetworkStream</param>
        /// <param name="name">檔案名稱</param>
        /// <param name="command">命令 一個byte</param>
        /// <param name="param">命令附帶參數 一個byte</param>
        /// <returns></returns>
        public bool SendFileName(NetworkStream stream, string name, byte command, byte param)
        {
            try
            {
                byte[] nameBytes = Encoding.UTF8.GetBytes(name);
                byte[] data = MessageProtocol.GetBytes(command, param, nameBytes);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
            
        }

        /// <summary>
        /// 傳送檔案大小(Byte單位)
        /// </summary>
        /// <param name="stream">接收方的NetworkStream</param>
        /// <param name="file">設定好的fileInfo</param>
        /// <param name="command">命令 一個byte</param>
        /// <param name="param">命令附帶參數 一個byte</param>
        /// <returns></returns>
        public bool SendFileSize(NetworkStream stream, FileInfo file, byte command, byte param, SizeUnit sizeUnit)
        {
            int size = Convert.ToInt32(file.Length);

            switch (sizeUnit) 
            {
                case SizeUnit.Byte:
                    break;
                case SizeUnit.KB:
                    size/=1024;
                    break;
                case SizeUnit.MB:
                    size /= 1024 * 1024;
                    break;
                case SizeUnit.GB:
                    size/= 1024 * 1024*1024;
                    break;
                default:
                    break;
            }

            try
            {
                byte[] sizeBytes = Encoding.UTF8.GetBytes(size.ToString());
                byte[] data = MessageProtocol.GetBytes(command, param, sizeBytes);
                stream.Write(data, 0, data.Length);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 傳送檔案本體
        /// </summary>
        /// <param name="stream">接收方的NetworkStream</param>
        /// <param name="file">設定好的fileInfo</param>
        /// <param name="command">命令 一個byte</param>
        /// <param name="param">命令附帶參數 一個byte</param>
        /// <returns></returns>
        public int SendFile(NetworkStream stream, FileInfo file, byte command, byte param)
        {
            
            byte[] head = null;
            MemoryStream ms = new MemoryStream();
            BinaryWriter bw = new BinaryWriter(ms);

            try
            {
                //建立HeadInfo
                bw.Write(command);
                bw.Write(param);
                int size = Convert.ToInt32(file.Length);
                bw.Write(size);
                head = ms.ToArray();
            }
            catch (Exception)
            {
                return 1;
            }
            

            //先將HeadInfo傳送過去  讓接收方知道接下來要收的檔案資訊
            stream.Write(head, 0, head.Length);

            byte[] sendFile = new byte[MessageProtocol.BUFFERSIZE];
            int lengthBytes;
            FileStream FS = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);
            try
            {
                do
                {
                    //將file的data讀出來
                    lengthBytes = FS.Read(sendFile, 0, sendFile.Length);
                    //直接傳給接收方
                    stream.Write(sendFile, 0, lengthBytes);
                } while (lengthBytes == MessageProtocol.BUFFERSIZE);
            }
            catch (Exception)
            {
                return 2;
            }
            finally 
            {
                FS.Close();
            }
            return 0;          

        }

        public int SendFileByChunks(NetworkStream stream, FileInfo file, byte command, byte param,int chunkCount)
        {

            byte[] head = null;
            

            
            int lengthBytes;
            FileStream FS = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);



            
            int remainingSize = Convert.ToInt32(file.Length);
            int chunkSize;
            if (remainingSize % chunkCount == 0) 
            {
                chunkSize = Convert.ToInt32(file.Length) / chunkCount;
            }
            else 
            {
                chunkSize = Convert.ToInt32(file.Length) / (chunkCount-1);
            }
            

            byte[] sendFile = new byte[chunkSize];

            while (remainingSize > 0) 
            {
                MemoryStream ms = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(ms);
                head = null;
                if (remainingSize >= chunkSize) 
                {
                    try
                    {
                        //建立HeadInfo
                        bw.Write(command);
                        bw.Write(param);
                        int size = chunkSize;
                        bw.Write(size);
                        head = ms.ToArray();
                        remainingSize -= size;
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                }
                else 
                {
                    try
                    {
                        //建立HeadInfo
                        bw.Write(command);
                        bw.Write(param);
                        int size = remainingSize;
                        bw.Write(size);
                        head = ms.ToArray();
                        remainingSize = 0;
                    }
                    catch (Exception)
                    {
                        return 1;
                    }
                }

                //先將HeadInfo傳送過去  讓接收方知道接下來要收的檔案資訊
                stream.Write(head, 0, head.Length);


                try
                {
                    //將file的data讀出來
                    lengthBytes = FS.Read(sendFile, 0, sendFile.Length);
                    //直接傳給接收方
                    stream.Write(sendFile, 0, lengthBytes);
                }
                catch (Exception)
                {
                    FS.Close();
                    return 2;
                }
            }
            FS.Close();
            return 0;

        }
    }


    public enum SizeUnit
    {
        Byte = 0,
        KB = 1,
        MB = 2,
        GB = 3,
    }

    public class ReceiveEventArgs 
    {
        public NetworkStream senderStream;
        public MessageProtocol mp;

        public ReceiveEventArgs(NetworkStream senderStream, MessageProtocol mp) 
        {
            this.senderStream = senderStream;
            this.mp = mp;
        }
    }

    public class ReceiveClientEventArgs
    {
        public NetworkStream senderStream;
        public MessageProtocol mp;
        public string name;
        public TcpClient client;
        public ReceiveClientEventArgs(NetworkStream senderStream, MessageProtocol mp, string name, TcpClient client)
        {
            this.senderStream = senderStream;
            this.mp = mp;
            this.name = name;
            this.client = client;
        }
    }

    public class NewClientJoinEventArgs 
    {
        public string clientName;
        public TcpClient client;
        public NewClientJoinEventArgs(string clientName,TcpClient client)
        {
            this.clientName = clientName;
            this.client=client;
        }
    }

    public class NewClientNameRepeatedEventArgs
    {
        public string clientName;
        public TcpClient client;
        public Dictionary<string, TcpClient> clients;
        public NewClientNameRepeatedEventArgs(string clientName, TcpClient client, Dictionary<string, TcpClient> clients)
        {
            this.clientName = clientName;
            this.client = client;
            this.clients = clients;
        }
    }

    public class LetClientDisconnectEventArgs
    {
        public string clientName;
        public LetClientDisconnectEventArgs(string clientName)
        {
            this.clientName = clientName;
        }
    }

    public class ClientUnExceptedDisconnectEventArgs
    {
        public string clientName;
        public ClientUnExceptedDisconnectEventArgs(string clientName)
        {
            this.clientName = clientName;
        }
    }

    public class ClientDisconnectEventArgs
    {
        public string clientName;
        public ClientDisconnectEventArgs(string clientName)
        {
            this.clientName = clientName;
        }
    }

    public class ReceivingEventArgs 
    {
        public int now;
        public int all;
        public ReceivingEventArgs(int now,int all) 
        {
            this.now = now;
            this.all = all;
        }
    }
}
