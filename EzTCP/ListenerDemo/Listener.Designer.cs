namespace ListenerDemo
{
    partial class Listener
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.txtb_IP = new System.Windows.Forms.TextBox();
            this.txtb_Port = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_StartListen = new System.Windows.Forms.Button();
            this.txtb_Messages = new System.Windows.Forms.TextBox();
            this.cklist_Clients = new System.Windows.Forms.CheckedListBox();
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(21, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP :";
            // 
            // txtb_IP
            // 
            this.txtb_IP.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtb_IP.Location = new System.Drawing.Point(26, 38);
            this.txtb_IP.Name = "txtb_IP";
            this.txtb_IP.Size = new System.Drawing.Size(100, 21);
            this.txtb_IP.TabIndex = 1;
            // 
            // txtb_Port
            // 
            this.txtb_Port.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtb_Port.Location = new System.Drawing.Point(26, 92);
            this.txtb_Port.Name = "txtb_Port";
            this.txtb_Port.Size = new System.Drawing.Size(100, 21);
            this.txtb_Port.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 77);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "PORT :";
            // 
            // btn_StartListen
            // 
            this.btn_StartListen.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btn_StartListen.Location = new System.Drawing.Point(179, 409);
            this.btn_StartListen.Name = "btn_StartListen";
            this.btn_StartListen.Size = new System.Drawing.Size(100, 33);
            this.btn_StartListen.TabIndex = 4;
            this.btn_StartListen.Text = "開始監聽";
            this.btn_StartListen.UseVisualStyleBackColor = true;
            this.btn_StartListen.Click += new System.EventHandler(this.btn_StartListen_Click);
            // 
            // txtb_Messages
            // 
            this.txtb_Messages.Location = new System.Drawing.Point(179, 38);
            this.txtb_Messages.Multiline = true;
            this.txtb_Messages.Name = "txtb_Messages";
            this.txtb_Messages.ReadOnly = true;
            this.txtb_Messages.Size = new System.Drawing.Size(245, 349);
            this.txtb_Messages.TabIndex = 5;
            // 
            // cklist_Clients
            // 
            this.cklist_Clients.FormattingEnabled = true;
            this.cklist_Clients.Location = new System.Drawing.Point(26, 136);
            this.cklist_Clients.Name = "cklist_Clients";
            this.cklist_Clients.Size = new System.Drawing.Size(100, 191);
            this.cklist_Clients.TabIndex = 6;
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button1.Location = new System.Drawing.Point(324, 409);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 33);
            this.button1.TabIndex = 7;
            this.button1.Text = "停止監聽";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("新細明體", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button2.Location = new System.Drawing.Point(26, 354);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(100, 33);
            this.button2.TabIndex = 8;
            this.button2.Text = "移除連線";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // Listener
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(436, 463);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.cklist_Clients);
            this.Controls.Add(this.txtb_Messages);
            this.Controls.Add(this.btn_StartListen);
            this.Controls.Add(this.txtb_Port);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtb_IP);
            this.Controls.Add(this.label1);
            this.Name = "Listener";
            this.Text = "Listener";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtb_IP;
        private System.Windows.Forms.TextBox txtb_Port;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btn_StartListen;
        private System.Windows.Forms.TextBox txtb_Messages;
        private System.Windows.Forms.CheckedListBox cklist_Clients;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
    }
}

