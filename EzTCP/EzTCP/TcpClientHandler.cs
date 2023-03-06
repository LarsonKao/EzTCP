using System;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace EzTCP
{
    /// <summary>
    /// 基於帶HeadInfo的標準data傳輸的Client
    /// </summary>
    public class TcpClientHandler : TcpCommonBase
    {
        /// <summary>
        /// 使用中的TcpClient
        /// </summary>
        public TcpClient client;
        private Thread listenThread;

        /// <summary>
        /// 接收到data後的事件
        /// </summary>
        /// <param name="sender">指的是TcpClientHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void ReceiveEvent(object sender, ReceiveEventArgs args);
        /// <summary>
        /// 接收到data後的事件
        /// </summary>
        public event ReceiveEvent receiveEvent;

        /// <summary>
        /// 與Listener斷開連線的事件
        /// </summary>
        /// <param name="sender">指的是TcpClientHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void ListenerDisconnectEvent(object sender, EventArgs args);
        /// <summary>
        /// 與Listener斷開連線的事件
        /// </summary>
        public event ListenerDisconnectEvent listenerDisconnectEvent;

        /// <summary>
        /// 被從Listener斷開連線的事件
        /// </summary>
        /// <param name="sender">指的是TcpClientHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void DisconnectFromListenerEvent(object sender, EventArgs args);
        /// <summary>
        /// 被從Listener斷開連線的事件
        /// </summary>
        public event DisconnectFromListenerEvent disconnectFromListenerEvent;

        /// <summary>
        /// 接收data中的事件
        /// </summary>
        /// <param name="sender">指的是TcpClientHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void ReceivingEvent(object sender, ReceivingEventArgs args);
        /// <summary>
        /// 接收data中的事件
        /// </summary>
        public event ReceivingEvent receivingEvent;

        /// <summary>
        /// 開始連線到Listener
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <returns></returns>
        public int StartConnect(string IP, string PORT,string uniqueName)
        {
            //建立TCP連線必須的兩個參數 : IP跟PORT
            IPAddress ip = IPAddress.Parse(IP);
            int port = int.Parse(PORT);

            try
            {
                client = new TcpClient();
                client.Connect(ip, port);
            }
            catch (Exception)
            {
                return 1;
            }

            //第一次建立連線時先跟對方說自己的名字
            //一對多連線辨別彼此的方法
            byte[] me = Encoding.UTF8.GetBytes(uniqueName);

            try
            {
                SendData(client.GetStream(),me);
            }
            catch (Exception)
            {
                //傳送名稱失敗
                return 2;
            }

            try
            {
                if (client.GetStream().ReadByte() == 1) 
                {
                    //連線結果為1 代表被拒絕連線 原因是名稱重複
                    return 3;
                }
            }
            catch (Exception)
            {
                return 3;
            }


            //另開Thread處理持續監聽Server傳過來的訊息
            //避免造成UI Thread阻塞
            listenThread = new Thread(ReceiveMessageWithCmd);
            listenThread.Start();

            return 0;
        }

        /// <summary>
        /// 讀取stream持續接收data
        /// </summary>
        public void ReceiveMessageWithCmd()
        {
            NetworkStream stream = client.GetStream();
            MessageProtocol mp = null;
            int receiveLength = 0;

            //新讀到的data
            byte[] receiveBuffer = new byte[MessageProtocol.BUFFERSIZE];

            //已經讀到且還沒裝包的data( storageBuffer )
            byte[] storageBuffer = new byte[] { };
            try
            {
                //持續從stream讀取data
                while ((receiveLength = stream.Read(receiveBuffer, 0, receiveBuffer.Length)) != 0)
                {
                    //每次讀到data都將已經讀到的data( storageBuffer )與新讀到的data (receiveBuffer)合併
                    storageBuffer = MessageProtocol.CombineBytes(storageBuffer, receiveBuffer, receiveLength);

                    //如果當前的data長度還不到達headinfo的長度  代表無法解析當前這包的head  所以直接下個迴圈繼續讀
                    if (storageBuffer.Length < HeadInfo.HEADLENGTH)
                    {
                        continue;
                    }
                    else
                    {                        
                        //解析headinfo
                        HeadInfo headInfo = HeadInfo.GetHeadInfo(storageBuffer);

                        if(receivingEvent!=null)
                            receivingEvent(this, new ReceivingEventArgs(storageBuffer.Length - HeadInfo.HEADLENGTH, headInfo.dataLength));

                        //如果這包資料扣去headinfo的長度後 大於等於 headinfo中紀錄的data長度  代表這包buffer中已經包含至少一個完整的訊息
                        while (storageBuffer.Length - HeadInfo.HEADLENGTH >= headInfo.dataLength)
                        {
                            //將buffer裝包
                            mp = new MessageProtocol(storageBuffer);

                            //將還沒處理的buffer重新指到mp中多出的moreData
                            storageBuffer = mp.moreData;
                            //先將多出來的那些資料(現在存在storageBuffer中的)的headinfo讀出來待迴圈判斷用
                            headInfo = HeadInfo.GetHeadInfo(storageBuffer);

                            //將已經完成裝包的mp丟進event中操作
                            if(receiveEvent!=null)
                                receiveEvent(this, new ReceiveEventArgs(client.GetStream(), mp));
                        }
                    }
                }
            }
            catch (Exception)
            {
                //主動斷線與意外斷線都在這邊
                if(listenerDisconnectEvent!=null)
                    listenerDisconnectEvent(this, new EventArgs());
                return;
            }
            //被從server主動斷開會走這邊
            Disconnect();
            if (disconnectFromListenerEvent != null)
                disconnectFromListenerEvent(this, new EventArgs());
        }

        /// <summary>
        /// 中斷連線
        /// </summary>
        public void Disconnect()
        {
            client.GetStream().Close();
            client.Close();
        }
    }
}
