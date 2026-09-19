namespace KepDevSmartMeter
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
            txtPort = new TextBox();
            txtIpAddress = new TextBox();
            label2 = new Label();
            label1 = new Label();
            btnSwitch = new Button();
            rTxtLog = new TextBox();
            btnHeartbeat = new Button();
            btnTimeSync = new Button();
            txtTemperature = new TextBox();
            btnSendTemp = new Button();
            btnSendAndWait = new Button();
            btnUploadFile = new Button();
            txtRequestData = new TextBox();
            btnCleanLog = new Button();
            txtToken = new TextBox();
            txtSn = new TextBox();
            labToken = new Label();
            labSn = new Label();
            SuspendLayout();
            // 
            // txtPort
            // 
            txtPort.Location = new Point(336, 29);
            txtPort.Margin = new Padding(4, 4, 4, 4);
            txtPort.Name = "txtPort";
            txtPort.Size = new Size(96, 38);
            txtPort.TabIndex = 11;
            txtPort.Text = "8803";
            // 
            // txtIpAddress
            // 
            txtIpAddress.Location = new Point(106, 29);
            txtIpAddress.Margin = new Padding(4, 4, 4, 4);
            txtIpAddress.Name = "txtIpAddress";
            txtIpAddress.Size = new Size(140, 38);
            txtIpAddress.TabIndex = 10;
            txtIpAddress.Text = "127.0.0.1";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(272, 33);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(62, 31);
            label2.TabIndex = 9;
            label2.Text = "端口";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(22, 33);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(86, 31);
            label1.TabIndex = 8;
            label1.Text = "服务器";
            // 
            // btnSwitch
            // 
            btnSwitch.Location = new Point(1008, 20);
            btnSwitch.Margin = new Padding(4, 4, 4, 4);
            btnSwitch.Name = "btnSwitch";
            btnSwitch.Size = new Size(142, 44);
            btnSwitch.TabIndex = 7;
            btnSwitch.Text = "连接";
            btnSwitch.UseVisualStyleBackColor = true;
            btnSwitch.Click += btnSwitch_Click;
            // 
            // rTxtLog
            // 
            rTxtLog.Location = new Point(22, 78);
            rTxtLog.Margin = new Padding(4, 4, 4, 4);
            rTxtLog.Multiline = true;
            rTxtLog.Name = "rTxtLog";
            rTxtLog.Size = new Size(790, 739);
            rTxtLog.TabIndex = 12;
            // 
            // btnHeartbeat
            // 
            btnHeartbeat.Location = new Point(858, 101);
            btnHeartbeat.Margin = new Padding(4, 4, 4, 4);
            btnHeartbeat.Name = "btnHeartbeat";
            btnHeartbeat.Size = new Size(142, 42);
            btnHeartbeat.TabIndex = 22;
            btnHeartbeat.Text = "心跳";
            btnHeartbeat.UseVisualStyleBackColor = true;
            btnHeartbeat.Click += btnHeartbeat_Click;
            // 
            // btnTimeSync
            // 
            btnTimeSync.Location = new Point(1008, 101);
            btnTimeSync.Margin = new Padding(4, 4, 4, 4);
            btnTimeSync.Name = "btnTimeSync";
            btnTimeSync.Size = new Size(142, 44);
            btnTimeSync.TabIndex = 23;
            btnTimeSync.Text = "对时";
            btnTimeSync.UseVisualStyleBackColor = true;
            btnTimeSync.Click += btnTimeSync_Click;
            // 
            // txtTemperature
            // 
            txtTemperature.Location = new Point(858, 176);
            txtTemperature.Margin = new Padding(4, 4, 4, 4);
            txtTemperature.Name = "txtTemperature";
            txtTemperature.Size = new Size(138, 38);
            txtTemperature.TabIndex = 24;
            txtTemperature.Text = "25.5";
            // 
            // btnSendTemp
            // 
            btnSendTemp.Location = new Point(1008, 174);
            btnSendTemp.Margin = new Padding(4, 4, 4, 4);
            btnSendTemp.Name = "btnSendTemp";
            btnSendTemp.Size = new Size(142, 44);
            btnSendTemp.TabIndex = 25;
            btnSendTemp.Text = "上报温度";
            btnSendTemp.UseVisualStyleBackColor = true;
            btnSendTemp.Click += btnSendTemp_Click;
            // 
            // btnSendAndWait
            // 
            btnSendAndWait.Location = new Point(1008, 245);
            btnSendAndWait.Margin = new Padding(4, 4, 4, 4);
            btnSendAndWait.Name = "btnSendAndWait";
            btnSendAndWait.Size = new Size(142, 44);
            btnSendAndWait.TabIndex = 26;
            btnSendAndWait.Text = "上报并等待响应";
            btnSendAndWait.UseVisualStyleBackColor = true;
            btnSendAndWait.Click += btnSendAndWait_Click;
            // 
            // btnUploadFile
            // 
            btnUploadFile.Location = new Point(858, 315);
            btnUploadFile.Margin = new Padding(4, 4, 4, 4);
            btnUploadFile.Name = "btnUploadFile";
            btnUploadFile.Size = new Size(142, 44);
            btnUploadFile.TabIndex = 27;
            btnUploadFile.Text = "上传文件";
            btnUploadFile.UseVisualStyleBackColor = true;
            btnUploadFile.Click += btnUploadFile_Click;
            // 
            // txtRequestData
            // 
            txtRequestData.Location = new Point(858, 245);
            txtRequestData.Margin = new Padding(4, 4, 4, 4);
            txtRequestData.Name = "txtRequestData";
            txtRequestData.Size = new Size(138, 38);
            txtRequestData.TabIndex = 28;
            txtRequestData.Text = "查询设备状态";
            // 
            // btnCleanLog
            // 
            btnCleanLog.Location = new Point(858, 777);
            btnCleanLog.Margin = new Padding(4, 4, 4, 4);
            btnCleanLog.Name = "btnCleanLog";
            btnCleanLog.Size = new Size(142, 44);
            btnCleanLog.TabIndex = 29;
            btnCleanLog.Text = "清理日志";
            btnCleanLog.UseVisualStyleBackColor = true;
            btnCleanLog.Click += btnCleanLog_Click;
            // 
            // txtToken
            // 
            txtToken.Location = new Point(795, 29);
            txtToken.Margin = new Padding(4);
            txtToken.Name = "txtToken";
            txtToken.Size = new Size(187, 38);
            txtToken.TabIndex = 33;
            txtToken.Text = "7929S82ASD892340SD9897234";
            // 
            // txtSn
            // 
            txtSn.Location = new Point(503, 29);
            txtSn.Margin = new Padding(4);
            txtSn.Name = "txtSn";
            txtSn.Size = new Size(180, 38);
            txtSn.TabIndex = 32;
            txtSn.Text = "72ZE126125192";
            // 
            // labToken
            // 
            labToken.AutoSize = true;
            labToken.Location = new Point(701, 33);
            labToken.Margin = new Padding(4, 0, 4, 0);
            labToken.Name = "labToken";
            labToken.Size = new Size(85, 31);
            labToken.TabIndex = 31;
            labToken.Text = "Token";
            // 
            // labSn
            // 
            labSn.AutoSize = true;
            labSn.Location = new Point(450, 33);
            labSn.Margin = new Padding(4, 0, 4, 0);
            labSn.Name = "labSn";
            labSn.Size = new Size(48, 31);
            labSn.TabIndex = 30;
            labSn.Text = "SN";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(14F, 31F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1190, 844);
            Controls.Add(txtToken);
            Controls.Add(txtSn);
            Controls.Add(labToken);
            Controls.Add(labSn);
            Controls.Add(btnCleanLog);
            Controls.Add(txtRequestData);
            Controls.Add(btnUploadFile);
            Controls.Add(btnSendAndWait);
            Controls.Add(btnSendTemp);
            Controls.Add(txtTemperature);
            Controls.Add(btnTimeSync);
            Controls.Add(btnHeartbeat);
            Controls.Add(rTxtLog);
            Controls.Add(txtPort);
            Controls.Add(txtIpAddress);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(btnSwitch);
            Margin = new Padding(4, 4, 4, 4);
            Name = "MainForm";
            Text = "Client";
            Load += MainForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtPort;
        private TextBox txtIpAddress;
        private Label label2;
        private Label label1;
        private Button btnSwitch;
        private TextBox rTxtLog;
        private Button btnHeartbeat;
        private Button btnTimeSync;
        private TextBox txtTemperature;
        private Button btnSendTemp;
        private Button btnSendAndWait;
        private Button btnUploadFile;
        private TextBox txtRequestData;
        private Button btnCleanLog;
        private TextBox txtToken;
        private TextBox txtSn;
        private Label labToken;
        private Label labSn;
    }
}
