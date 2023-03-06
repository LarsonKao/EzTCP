using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using EzTCP;
namespace ClientDemo
{
    /// <summary>
    /// TCP Client的Demo
    /// 使用前須先將
    /// 1. Method : ReceiveFile中的dirPath參數改成本機專案存放的資料夾下的target路徑
    /// </summary>
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
        }

        private TcpClientHandler tcpClient;
        string fileName;
        int fileSize;
        int fileCount;
        int fileChunkCount;
        List<MessageProtocol> fileChunks;
        public void ReceiveEvent(object receiver,ReceiveEventArgs args) 
        {
            MessageProtocol mp = args.mp;
            switch (mp.headInfo.command)
            {
                
                case 1:
                    //單純訊息
                    string message = mp.AsString();
                    if (!string.IsNullOrEmpty(message))
                    {
                        AppendMessages(message);
                    }
                    break;
                case 2:
                    switch (mp.headInfo.param)
                    {
                        case 0:
                            fileName = mp.AsString();
                            AppendMessages($"檔案名稱 : {fileName}");
                            break;
                        case 1:
                            fileSize = Convert.ToInt32(mp.AsString());
                            AppendMessages($"檔案大小 : {fileSize} KB");
                            break;
                        case 2:
                            string dirPath = textBox1.Text;

                            FileInfo fi = new FileInfo(fileName);
                            FileStream fs = new FileStream(dirPath + '/' + fi.Name, FileMode.Append, FileAccess.Write);

                            fs.Write(mp.messageData, 0, mp.headInfo.dataLength);
                            fs.Close();
                            AppendMessages("接收完成 : " + fileName);
                            break;
                    }
                    break;
                case 3:
                    switch (mp.headInfo.param)
                    {
                        case 0:
                            fileCount = Convert.ToInt32(Encoding.UTF8.GetString(mp.messageData, 0, mp.headInfo.dataLength));
                            AppendMessages($"檔案清單數量 : {fileCount}");
                            break;
                    }
                    break;
                case 4:
                    switch (mp.headInfo.param) 
                    {
                        case 0:
                            fileChunkCount = Convert.ToInt32(mp.AsString());
                            AppendMessages($"檔案切片數量 : {fileChunkCount}");
                            fileChunks = new List<MessageProtocol>();
                            break;
                        case 1:
                            fileChunks.Add(mp);
                            AppendMessages($"檔案切片大小 : {mp.headInfo.dataLength}");
                            AppendMessages($"檔案已接收切片數量 : {fileChunks.Count} / {fileChunkCount}");
                            break;
                        case 2:
                            string dirPath = textBox1.Text;

                            FileInfo fi = new FileInfo(fileName);
                            FileStream fs = new FileStream(dirPath + '/' + fi.Name, FileMode.Append, FileAccess.Write);
                            foreach (var chunk in fileChunks) 
                            {
                                fs.Write(chunk.messageData, 0, chunk.headInfo.dataLength);
                            }
                            fs.Close();
                            AppendMessages("接收完成 : " + fileName);
                            break;
                    }
                    break;
            }
        }
        public void ListenerDisconnectEvent(object sender, EventArgs args) 
        {
            AppendMessages("已與Server斷線");
        }
        public void DisconnectFromListenerEvent(object sender, EventArgs args)
        {
            AppendMessages("已被Server主動斷線");
        }
        public void ReceivingEvent(object sender, ReceivingEventArgs args)
        {
            this.Invoke((MethodInvoker)delegate () 
            {
                label4.Text = $"檔案傳輸  {args.now}  /  {args.all}";
                label5.Text = $"檔案傳輸  {(args.now*1.0/args.all).ToString("0%")} ";
            });
            
        }
        #region UI操作
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
        private void btn_Send_Click(object sender, EventArgs e)
        {
            tcpClient.SendDataWithCmd(tcpClient.client.GetStream(), 1,0,Encoding.UTF8.GetBytes("HAHA"));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tcpClient.SendDataWithCmd(tcpClient.client.GetStream(),3, 0, Encoding.UTF8.GetBytes("Update"));
        }

        private void button2_Click(object sender, EventArgs e)
        {
            tcpClient.SendDataWithCmd(tcpClient.client.GetStream(), 2,0, Encoding.UTF8.GetBytes("Update"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AppendMessages("中斷連線");
            tcpClient.Disconnect();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            tcpClient.SendDataWithCmd(tcpClient.client.GetStream(), 1,1,Encoding.UTF8.GetBytes("Hello"));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            tcpClient.SendDataWithCmd(tcpClient.client.GetStream(), 4, 0, Encoding.UTF8.GetBytes("請求切片檔案"));
        }

        private void btn_StartConnect_Click(object sender, EventArgs e)
        {   
            tcpClient = new TcpClientHandler();
            tcpClient.receiveEvent += ReceiveEvent;
            tcpClient.listenerDisconnectEvent += ListenerDisconnectEvent;
            tcpClient.disconnectFromListenerEvent += DisconnectFromListenerEvent;
            tcpClient.receivingEvent += ReceivingEvent;
            
            var result = tcpClient.StartConnect(txtb_IP.Text,txtb_Port.Text,txtb_Name.Text);
            if (result == 0)
                AppendMessages("連線成功");
            else if (result == 1)
                AppendMessages("嘗試連線失敗");
            if (result == 2)
                AppendMessages("嘗試傳送名稱失敗");
            else if (result == 3)
                AppendMessages("名稱重複，已被拒絕連線");
        }

        #endregion


    }
}
