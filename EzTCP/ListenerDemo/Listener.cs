using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Schema;
using EzTCP;
using static System.Net.WebRequestMethods;


namespace ListenerDemo
{
    /// <summary>
    /// TCP Listener的Demo
    /// 支持Listener 一對多 Client操作
    /// 
    /// 使用前須先將
    /// 1. Method : ReceiveClientMessage中 case Update底下的路徑調整成任意一個想要傳送的檔案路徑
    /// 2.Method : ReceiveClientMessage中 case Updates底下的路徑調整成本機專案資料夾下的res資料夾路徑
    /// </summary>
    public partial class Listener : Form
    {
        public Listener()
        {
            InitializeComponent();
        }   

        private TcpListenerHandler tcpListener;

        /// <summary>
        /// 處理接收到的data
        /// </summary>
        /// <param name="receiver"></param>
        /// <param name="args"></param>
        public void ReceiveEvent(object receiver, ReceiveClientEventArgs args)
        {
            MessageProtocol mp = args.mp;
            switch (mp.headInfo.command)
            {
                case 1:
                    switch (mp.headInfo.param)
                    {
                        case 0:
                            string message = mp.AsString();
                            if (!string.IsNullOrEmpty(message))
                            {
                                AppendMessages(message);
                            }
                            break;
                        case 1:
                            AppendMessages(" : Hello");
                            (receiver as TcpListenerHandler).SendDataWithCmd(args.senderStream, 1, 1, Encoding.UTF8.GetBytes("server send back"));
                            break;
                    }
                    break;
                case 2:
                    switch (mp.headInfo.param)
                    {
                        case 0:
                            FileInfo file = new FileInfo(@"D:\code\TCP_Demo2.0\res\200MB.exe");
                            (receiver as TcpListenerHandler).SendFileName(args.senderStream, file.Name,2,0);
                            (receiver as TcpListenerHandler).SendFileSize(args.senderStream, file, 2, 1, SizeUnit.KB);
                            (receiver as TcpListenerHandler).SendFile(args.senderStream, file, 2, 2);
                            break;
                        case 1:

                            break;
                    }
                    break;
                case 3:
                    switch (mp.headInfo.param)
                    {
                        case 0:
                            string dir = @"D:\code\TCP_Demo2.0\res\";                            
                            DirectoryInfo directory = new DirectoryInfo(dir);
                            FileInfo[] files = directory.GetFiles();
                            (receiver as TcpListenerHandler).SendFileCount(args.senderStream, files.Length, 3, 0);
                            foreach (FileInfo file in files)
                            {
                                (receiver as TcpListenerHandler).SendFileName(args.senderStream, file.Name, 2, 0);
                                (receiver as TcpListenerHandler).SendFileSize(args.senderStream, file, 2, 1, SizeUnit.KB);
                                (receiver as TcpListenerHandler).SendFile(args.senderStream, file, 2, 2);
                            }
                            break;
                    }
                    break;
                case 4:
                    switch (mp.headInfo.param) 
                    {
                        case 0:
                            int chunkCount = 10;
                            
                            FileInfo file = new FileInfo(@"D:\GIT\★程式範例專區\Source Code\TCP_Demo2.0\res\200MB.exe");
                            (receiver as TcpListenerHandler).SendFileName(args.senderStream, file.Name, 2, 0);
                            (receiver as TcpListenerHandler).SendDataWithCmd(args.senderStream, 4, 0, Encoding.UTF8.GetBytes(chunkCount.ToString()));
                            (receiver as TcpListenerHandler).SendFileByChunks(args.senderStream, file, 4, 1, chunkCount);
                            (receiver as TcpListenerHandler).SendDataWithCmd(args.senderStream, 4, 2, Encoding.UTF8.GetBytes("0"));
                            break;
                    }
                    break;
            }
        }

        public void NewClientJoinEvent(object sender, NewClientJoinEventArgs args) 
        {
            AppendMessages(args.clientName + "已加入");
            this.Invoke((MethodInvoker)delegate (){
                cklist_Clients.Items.Add(args.clientName);
            });            
        }

        public bool NewClientNameRepeatedEvent(object sender, NewClientNameRepeatedEventArgs args)
        {
            AppendMessages(args.clientName + "已重複");
            return false;
        }

        public void ClientDisconnectEvent(object sender, ClientDisconnectEventArgs args) 
        {
            AppendMessages(args.clientName + "已斷線");
            this.Invoke((MethodInvoker)delegate () {
                cklist_Clients.Items.Remove(args.clientName);
            });            
        }
        public void LetClientDisconnectEvent(object sender, LetClientDisconnectEventArgs args)
        {
            AppendMessages(args.clientName + "已被斷線");
            this.Invoke((MethodInvoker)delegate () {
                cklist_Clients.Items.Remove(args.clientName);
            });
        }
        public void clientUnExceptedDisconnectEvent(object sender, ClientUnExceptedDisconnectEventArgs args)
        {
            AppendMessages(args.clientName + "已意外斷線");
            this.Invoke((MethodInvoker)delegate () {
                cklist_Clients.Items.Remove(args.clientName);
            });
        }

        public void ReceivingEvent(object sender, ReceivingEventArgs args) 
        {
            AppendMessages($"檔案傳輸  {args.now}  /  {args.all}");
        }
        #region UI操作

        private void btn_StartListen_Click(object sender, EventArgs e)
        {
            tcpListener = new TcpListenerHandler();
            tcpListener.receiveEvent += ReceiveEvent;
            tcpListener.newClientJoinEvent += NewClientJoinEvent;
            tcpListener.newClientNameRepeatedEvent += NewClientNameRepeatedEvent;
            tcpListener.letClientDisconnectEvent += LetClientDisconnectEvent;
            tcpListener.clientDisconnectEvent += ClientDisconnectEvent;
            tcpListener.clientUnExceptedDisconnectEvent += clientUnExceptedDisconnectEvent;
            tcpListener.receivingEvent += ReceivingEvent;
            bool successed = tcpListener.StartListen(txtb_IP.Text,txtb_Port.Text);
            if (successed)
            {
                AppendMessages("開始監聽");
            }
            else 
            {
                AppendMessages("監聽失敗");
            }
        }

        /// <summary>
        /// 顯示訊息在UI上
        /// </summary>
        /// <param name="message"></param>
        private void AppendMessages(string message)
        {
            txtb_Messages.Invoke((MethodInvoker)delegate ()
            {
                txtb_Messages.Text = txtb_Messages.Text + message + Environment.NewLine;
            });
        }

        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            bool result = tcpListener.StopListening();
            if (result) 
            {
                AppendMessages("已停止監聽");
            }
            else
            {
                AppendMessages("停止監聽失敗");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            for(int i =0;i< cklist_Clients.CheckedItems.Count; i++) 
            {
                tcpListener.BreakClientConnect(cklist_Clients.CheckedItems[i].ToString());
            }
        }
    }
}
