namespace KepServerHub
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnSwitch = new Button();
            rTxtLog = new TextBox();
            label1 = new Label();
            label2 = new Label();
            txtIpAddress = new TextBox();
            txtPort = new TextBox();
            lblClients = new Label();
            cmbClients = new ComboBox();
            txtParam = new TextBox();
            btnSendParam = new Button();
            btnSendFile = new Button();
            lblReplyHint = new Label();
            lstClientRequests = new ListBox();
            txtReplyContent = new TextBox();
            btnReplyToClient = new Button();
            btnKickClient = new Button();
            btnCleanLog = new Button();
            dgvClientList = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvClientList).BeginInit();
            SuspendLayout();
            // 
            // btnSwitch
            // 
            btnSwitch.Location = new Point(500, 64);
            btnSwitch.Margin = new Padding(4);
            btnSwitch.Name = "btnSwitch";
            btnSwitch.Size = new Size(142, 44);
            btnSwitch.TabIndex = 1;
            btnSwitch.Text = "启动";
            btnSwitch.UseVisualStyleBackColor = true;
            btnSwitch.Click += btnSwitch_Click;
            // 
            // rTxtLog
            // 
            rTxtLog.Location = new Point(26, 142);
            rTxtLog.Margin = new Padding(4);
            rTxtLog.Multiline = true;
            rTxtLog.Name = "rTxtLog";
            rTxtLog.Size = new Size(790, 739);
            rTxtLog.TabIndex = 2;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(26, 69);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(84, 31);
            label1.TabIndex = 3;
            label1.Text = "Ip地址";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(294, 69);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(62, 31);
            label2.TabIndex = 4;
            label2.Text = "端口";
            // 
            // txtIpAddress
            // 
            txtIpAddress.Location = new Point(110, 66);
            txtIpAddress.Margin = new Padding(4);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(140, 38);
            txtIpAddress.TabIndex = 5;
            txtIpAddress.Text = "0.0.0.0";
            // 
            // txtPort
            // 
            txtPort.Location = new Point(358, 66);
            txtPort.Margin = new Padding(4);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(96, 38);
            txtPort.TabIndex = 6;
            txtPort.Text = "8803";
            // 
            // lblClients
            // 
            lblClients.AutoSize = true;
            lblClients.Location = new Point(827, 73);
            lblClients.Margin = new Padding(4, 0, 4, 0);
            lblClients.Name = "lblClients";
            lblClients.Size = new Size(86, 31);
            lblClients.TabIndex = 7;
            lblClients.Text = "客户端";
            // 
            // cmbClients
            // 
            cmbClients.FormattingEnabled = true;
            cmbClients.Location = new Point(972, 68);
            cmbClients.Margin = new Padding(4);
            cmbClients.Name = "cmbClients";
            cmbClients.Size = new Size(228, 39);
            cmbClients.TabIndex = 8;
            // 
            // txtParam
            // 
            txtParam.Location = new Point(834, 142);
            txtParam.Margin = new Padding(4);
            txtParam.Name = "txtParam";
            txtParam.Size = new Size(188, 38);
            txtParam.TabIndex = 9;
            txtParam.Text = "示例参数";
            // 
            // btnSendParam
            // 
            btnSendParam.Location = new Point(1058, 136);
            btnSendParam.Margin = new Padding(4);
            btnSendParam.Name = "btnSendParam";
            btnSendParam.Size = new Size(142, 44);
            btnSendParam.TabIndex = 10;
            btnSendParam.Text = "下发参数";
            btnSendParam.UseVisualStyleBackColor = true;
            btnSendParam.Click += btnSendParam_Click;
            // 
            // btnSendFile
            // 
            btnSendFile.Location = new Point(1239, 136);
            btnSendFile.Margin = new Padding(4);
            btnSendFile.Name = "btnSendFile";
            btnSendFile.Size = new Size(142, 44);
            btnSendFile.TabIndex = 11;
            btnSendFile.Text = "下发文件";
            btnSendFile.UseVisualStyleBackColor = true;
            btnSendFile.Click += btnSendFile_Click;
            // 
            // lblReplyHint
            // 
            lblReplyHint.AutoSize = true;
            lblReplyHint.Location = new Point(834, 195);
            lblReplyHint.Margin = new Padding(4, 0, 4, 0);
            lblReplyHint.Name = "lblReplyHint";
            lblReplyHint.Size = new Size(620, 31);
            lblReplyHint.TabIndex = 12;
            lblReplyHint.Text = "客户端上报请求（双击选择，输入回复内容后点击回复）:";
            // 
            // lstClientRequests
            // 
            lstClientRequests.FormattingEnabled = true;
            lstClientRequests.Location = new Point(834, 244);
            lstClientRequests.Margin = new Padding(4);
            lstClientRequests.Name = "lstClientRequests";
            lstClientRequests.Size = new Size(442, 314);
            lstClientRequests.TabIndex = 13;
            // 
            // txtReplyContent
            // 
            txtReplyContent.Location = new Point(834, 580);
            txtReplyContent.Margin = new Padding(4);
            txtReplyContent.Name = "txtReplyContent";
            txtReplyContent.Size = new Size(368, 38);
            txtReplyContent.TabIndex = 14;
            txtReplyContent.Text = "服务端已处理";
            // 
            // btnReplyToClient
            // 
            btnReplyToClient.Location = new Point(834, 645);
            btnReplyToClient.Margin = new Padding(4);
            btnReplyToClient.Name = "btnReplyToClient";
            btnReplyToClient.Size = new Size(142, 44);
            btnReplyToClient.TabIndex = 15;
            btnReplyToClient.Text = "回复客户端";
            btnReplyToClient.UseVisualStyleBackColor = true;
            btnReplyToClient.Click += btnReplyToClient_Click;
            // 
            // btnKickClient
            // 
            btnKickClient.Location = new Point(1239, 69);
            btnKickClient.Margin = new Padding(4);
            btnKickClient.Name = "btnKickClient";
            btnKickClient.Size = new Size(142, 44);
            btnKickClient.TabIndex = 16;
            btnKickClient.Text = "踢下线";
            btnKickClient.UseVisualStyleBackColor = true;
            btnKickClient.Click += btnKickClient_Click;
            // 
            // btnCleanLog
            // 
            btnCleanLog.Location = new Point(834, 837);
            btnCleanLog.Margin = new Padding(4);
            btnCleanLog.Name = "btnCleanLog";
            btnCleanLog.Size = new Size(142, 44);
            btnCleanLog.TabIndex = 30;
            btnCleanLog.Text = "清理日志";
            btnCleanLog.UseVisualStyleBackColor = true;
            btnCleanLog.Click += btnCleanLog_Click;
            // 
            // dgvClientList
            // 
            dgvClientList.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvClientList.Location = new Point(1522, 95);
            dgvClientList.Name = "dgvClientList";
            dgvClientList.RowHeadersWidth = 82;
            dgvClientList.Size = new Size(732, 300);
            dgvClientList.TabIndex = 32;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(2314, 917);
            Controls.Add(dgvClientList);
            Controls.Add(btnCleanLog);
            Controls.Add(btnKickClient);
            Controls.Add(btnReplyToClient);
            Controls.Add(txtReplyContent);
            Controls.Add(lstClientRequests);
            Controls.Add(lblReplyHint);
            Controls.Add(btnSendFile);
            Controls.Add(btnSendParam);
            Controls.Add(txtParam);
            Controls.Add(cmbClients);
            Controls.Add(lblClients);
            Controls.Add(txtPort);
            Controls.Add(txtIpAddress);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(rTxtLog);
            Controls.Add(btnSwitch);
            Margin = new Padding(4);
            Name = "MainForm";
            Text = "Server";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)dgvClientList).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private Button btnSwitch;
        private TextBox rTxtLog;
        private Label label1;
        private Label label2;
        private TextBox txtIpAddress;
        private TextBox txtPort;
        private Label lblClients;
        private ComboBox cmbClients;
        private TextBox txtParam;
        private Button btnSendParam;
        private Button btnSendFile;
        private Label lblReplyHint;
        private ListBox lstClientRequests;
        private TextBox txtReplyContent;
        private Button btnReplyToClient;
        private Button btnKickClient;
        private Button btnCleanLog;
        private DataGridView dgvClientList;
    }
}
