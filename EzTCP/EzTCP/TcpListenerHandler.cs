using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace EzTCP
{
    /// <summary>
    /// 基於帶HeadInfo的標準data傳輸的Listener
    /// </summary>
    public class TcpListenerHandler : TcpCommonBase
    {
        /// <summary>
        /// 使用中的TcpListener
        /// </summary>
        public TcpListener _listener;
        private Thread _listeningThread;

        private bool stoppingListen = false;

        /// <summary>
        /// 用來記錄連線中的Client清單  Key : 用於辨識用的Client名稱  Value : TcpClient
        /// </summary>
        private Dictionary<string, TcpClient> clients = new Dictionary<string, TcpClient>();

        /// <summary>
        /// 接收到data後的事件
        /// </summary>
        /// <param name="sender">指的是TcpListenHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void ReceiveEvent(object sender, ReceiveClientEventArgs args);
        /// <summary>
        /// 接收到data後的事件
        /// </summary>
        public event ReceiveEvent receiveEvent;

        /// <summary>
        /// 成功與新的client連線時的事件
        /// </summary>
        /// <param name="client">傳送者，指的是TcpClient</param>
        /// <param name="args">事件參數</param>
        public delegate void NewClientJoinEvent(object client, NewClientJoinEventArgs args);
        /// <summary>
        /// 成功與新的client連線時的事件
        /// </summary>
        public event NewClientJoinEvent newClientJoinEvent;

        /// <summary>
        /// 新的Client名稱重複事件的事件
        /// </summary>
        /// <param name="client">傳送者，指的是TcpClient</param>
        /// <param name="args">事件參數</param>
        public delegate bool NewClientNameRepeatedEvent(object client, NewClientNameRepeatedEventArgs args);
        /// <summary>
        /// 成功與新的client連線時的事件
        /// </summary>
        public event NewClientNameRepeatedEvent newClientNameRepeatedEvent;

        /// <summary>
        /// 主動將Client斷開連線時的事件
        /// </summary>
        /// <param name="sender">接收者，指的是TcpListenHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void LetClientDisconnectEvent(object sender, LetClientDisconnectEventArgs args);
        /// <summary>
        /// 主動將Client斷開連線時的事件
        /// </summary>
        public event LetClientDisconnectEvent letClientDisconnectEvent;

        /// <summary>
        /// Client意外斷開連線時的事件
        /// </summary>
        /// <param name="sender">接收者，指的是TcpListenHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void ClientUnExceptedDisconnectEvent(object sender, ClientUnExceptedDisconnectEventArgs args);
        /// <summary>
        /// Client意外斷開連線時的事件
        /// </summary>
        public event ClientUnExceptedDisconnectEvent clientUnExceptedDisconnectEvent;

        /// <summary>
        /// Client主動斷開連線時的事件
        /// </summary>
        /// <param name="sender">接收者，指的是TcpListenHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void ClientDisconnectEvent(object sender, ClientDisconnectEventArgs args);
        /// <summary>
        /// Client主動斷開連線時的事件
        /// </summary>
        public event ClientDisconnectEvent clientDisconnectEvent;

        /// <summary>
        /// 接收data中的事件
        /// </summary>
        /// <param name="sender">指的是TcpListenerHandler本身</param>
        /// <param name="args">事件參數</param>
        public delegate void ReceivingEvent(object sender, ReceivingEventArgs args);
        /// <summary>
        /// 接收data中的事件
        /// </summary>
        public event ReceivingEvent receivingEvent;

        /// <summary>
        /// 初始化Listener 並且開始監聽是否有Client建立請求
        /// </summary>
        /// <param name="IP"></param>
        /// <param name="PORT"></param>
        /// <returns></returns>
        public bool StartListen(string IP, string PORT)
        {            
            //Listener必要的兩個參數 : IP跟PORT
            IPAddress ip = IPAddress.Parse(IP);
            int port = int.Parse(PORT);

            try
            {
                //建立實體
                _listener = new TcpListener(ip, port);
                _listener.Start();

                //開始異步監聽
                _listener.BeginAcceptTcpClient(ListenClients, _listener);
            }
            catch (Exception)
            {
                return false;
            }
            return true;

        }

        /// <summary>
        /// 持續監聽Client的請求
        /// 收到後會新增進Clients清單中並且另開Thread持續接收訊息
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ListenClients(IAsyncResult asyncResult)
        {
            //成功收到Client請求並且建立連線
            TcpClient client = null;
            try
            {
                client = _listener.EndAcceptTcpClient(asyncResult);
            }
            catch (Exception ex)
            {
                return;
            }
            

            

            //獲取Client第一次傳入的自訂名稱(必須不重複)            
            NetworkStream stream = client.GetStream();
            byte[] nameByte = new byte[1024];
            int length = stream.Read(nameByte, 0, nameByte.Length);
            string name = Encoding.UTF8.GetString(nameByte, 0, length);

            //判斷是否重名
            if (clients.ContainsKey(name)) 
            {
                //判斷是否有實作重名的事件  沒有或著事件回傳false則代表拒絕連線
                if (newClientNameRepeatedEvent == null || !newClientNameRepeatedEvent(client, new NewClientNameRepeatedEventArgs(name, client,clients)))
                {
                    //試傳連線結果
                    stream.WriteByte(1);
                    stream.Close();
                    client.Close();
                    _listener.BeginAcceptTcpClient(ListenClients, _listener);
                    return;
                }
            }
            //試傳連線結果
            stream.WriteByte(0);


            //加入Clients中
            AddClientToDict(client, name);

            if (newClientJoinEvent != null)
                newClientJoinEvent(client, new NewClientJoinEventArgs(name,client));
            
            

            //另開Thread用來監聽這個Client的訊息
            Thread thread = new Thread(ReceiveMessageWithCmd);
            thread.Start(name);

            // 繼續監聽等待下一個連線請求
            _listener.BeginAcceptTcpClient(ListenClients, _listener);
        }

        /// <summary>
        /// 將Client加入Dictionary中，並且更新UI
        /// </summary>
        /// <param name="client"></param>
        /// <param name="name"></param>
        private void AddClientToDict(TcpClient client, string name)
        {

            clients.Add(name, client);
        }

        /// <summary>
        /// 讀取stream持續接收data
        /// </summary>
        /// <param name="name">指定的client name</param>
        private void ReceiveMessageWithCmd(object name)
        {
            var client = clients[name as string];
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
                    if (storageBuffer.Length <= HeadInfo.HEADLENGTH)
                    {
                        continue;
                    }
                    else
                    {
                        //解析headinfo
                        HeadInfo headInfo = HeadInfo.GetHeadInfo(storageBuffer);

                        if (receivingEvent != null)
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
                            if (receiveEvent != null)
                                receiveEvent(this, new ReceiveClientEventArgs(client.GetStream(), mp, name.ToString(),client)) ;
                        }
                    }
                }
            }
            catch (Exception)
            {
                bool isInDict = clients.Remove(name as string);

                //如果這個client已經不dictionary中，代表我們已經在停止監聽時就把他斷連了
                if (!isInDict) 
                {
                    if (letClientDisconnectEvent != null)
                        letClientDisconnectEvent(this, new LetClientDisconnectEventArgs(name as string));
                }
                else
                {
                    if (clientUnExceptedDisconnectEvent != null)
                        clientUnExceptedDisconnectEvent(this, new ClientUnExceptedDisconnectEventArgs(name as string));
                }
                return;
            }
            clients.Remove(name as string);
            if (clientDisconnectEvent != null)
                clientDisconnectEvent(this, new ClientDisconnectEventArgs(name as string));
            return;
        }

        /// <summary>
        /// 停止Listener的監聽，同時會將所有Client主動斷連
        /// </summary>
        /// <returns></returns>
        public bool StopListening() 
        {
            try
            {
                List<string> removeNames = new List<string>();
                foreach (var pair in clients)
                {
                    var client = pair.Value;
                    client.GetStream().Close();
                    client.Close();
                    removeNames.Add(pair.Key);
                }
                for (int i = 0; i < removeNames.Count; i++)
                {
                    clients.Remove(removeNames[i]);
                }
                _listener.Stop();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 將Client主動斷連
        /// </summary>
        /// <param name="name">Client的名稱</param>
        /// <returns></returns>
        public bool BreakClientConnect(string name)
        {
            try
            {
                TcpClient client = clients[name];
                client.GetStream().Close();
                client.Close();
                clients.Remove(name);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 依照unique name獲取tcp client
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TcpClient GetClient(string name) 
        {
            if (clients.ContainsKey(name))
                return clients[name];
            else
                return null;
        }
    }
}
