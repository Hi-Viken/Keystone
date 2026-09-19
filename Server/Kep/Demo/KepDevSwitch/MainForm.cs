using KepClientCore;
using KepDevSwitch;

namespace KepDevSwitch
{
    public partial class MainForm : Form
    {
        private KepClient kepClient;
        private bool ServerState = false;



        //private Button btnHeartbeat;
        //private Button btnTimeSync;
        //private Button btnSendTemp;
        //private TextBox txtTemperature;
        //private Button btnSendAndWait;
        //private TextBox txtRequestData;
        //private Button btnUploadFile;

        public MainForm()
        {
            InitializeComponent();
        }


        private async void btnSwitch_Click(object sender, EventArgs e)
        {
            if (ServerState)
            {
                ServerState = false;
                kepClient?.Disconnect();
                btnSwitch.Text = "连接";
            }
            else
            {
                ServerState = true;
                btnSwitch.Text = "断开";

                kepClient = new KepClient();
                kepClient.OnLog += msg => Invoke(() => AppendLog(msg));
                kepClient.OnDisconnect += KepClient_OnDisconnect;

                _ = kepClient.ConnectAsync(this.txtIpAddress.Text, Convert.ToInt32(this.txtPort.Text),this.txtSn.Text,this.txtToken.Text);
            }
        }

        private void KepClient_OnDisconnect()
        {
            ServerState = false;
            kepClient?.Disconnect();
            btnSwitch.Text = "连接";
        }

        private void AppendLog(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            rTxtLog.AppendText(line);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            this.btnSwitch.Text = "连接";

            rTxtLog.Multiline = true;
            rTxtLog.ReadOnly = true;
            rTxtLog.ScrollBars = ScrollBars.Vertical;
            rTxtLog.BackColor = Color.Black;
            rTxtLog.ForeColor = Color.LightGreen;
            rTxtLog.Font = new Font("Consolas", 9f);

        }

        private async void btnTimeSync_Click(object sender, EventArgs e)
        {
            if (kepClient?.IsAuthenticated == true)
                await kepClient.SendTimeSyncAsync();
            else
                AppendLog("未连接或未认证");
        }


        private async void btnHeartbeat_Click(object sender, EventArgs e)
        {
            if (kepClient?.IsAuthenticated == true)
                await kepClient.SendHeartbeatAsync();
            else
                AppendLog("未连接或未认证");
        }

        private async void btnSendTemp_Click(object sender, EventArgs e)
        {
            if (kepClient?.IsAuthenticated == true)
                await kepClient.SendTemperatureAsync(txtTemperature.Text);
            else
                AppendLog("未连接或未认证");
        }

        private async void btnSendAndWait_Click(object sender, EventArgs e)
        {

            if (kepClient?.IsAuthenticated != true)
            {
                AppendLog("未连接或未认证");
                return;
            }

            btnSendAndWait.Enabled = false;
            try
            {
                var requestData = System.Text.Encoding.UTF8.GetBytes(txtRequestData.Text);
                AppendLog($"发送请求: {txtRequestData.Text}");
                var response = await kepClient.SendRequestAndWaitAsync(requestData, timeoutMs: 10_000);
                AppendLog($"收到响应: {response.GetStringData()}");
            }
            catch (TimeoutException ex)
            {
                AppendLog($"等待超时: {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendLog($"请求失败: {ex.Message}");
            }
            finally
            {
                btnSendAndWait.Enabled = true;
            }

        }

        private async void btnUploadFile_Click(object sender, EventArgs e)
        {

            if (kepClient?.IsAuthenticated != true)
            {
                AppendLog("未连接或未认证");
                return;
            }

            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "选择要上传的文件";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            btnUploadFile.Enabled = false;
            try
            {
                string filePath = dialog.FileName;
                AppendLog($"开始上传文件: {Path.GetFileName(filePath)}");
                await kepClient.SendFileToServerAsync(filePath);
                AppendLog($"文件上传成功: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                AppendLog($"文件上传失败: {ex.Message}");
            }
            finally
            {
                btnUploadFile.Enabled = true;
            }

        }

        private void btnCleanLog_Click(object sender, EventArgs e)
        {
            rTxtLog.Text=string.Empty;
        }
    }
}
