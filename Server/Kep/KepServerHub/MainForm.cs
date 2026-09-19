using KepServerCore;
using System.Collections;
using System.ComponentModel;

namespace KepServerHub
{
    public partial class MainForm : Form
    {
        private KepServer kepServer;
        private bool ServerState = false;
        private System.Windows.Forms.Timer refreshTimer;


        // 存储待回复的客户端请求 (clientId, requestId, requestData)
        private readonly System.Collections.Concurrent.ConcurrentDictionary<uint, (string ClientId, string RequestData)> _pendingClientRequests = new();

        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.btnSwitch.Text = "启动";
            dgvClientList.DataSource = ClientList;

            rTxtLog.Multiline = true;
            rTxtLog.ReadOnly = true;
            rTxtLog.ScrollBars = ScrollBars.Vertical;
            rTxtLog.BackColor = Color.Black;
            rTxtLog.ForeColor = Color.LightGreen;
            rTxtLog.Font = new Font("Consolas", 9f);




            // 定时刷新在线客户端列表
            refreshTimer = new System.Windows.Forms.Timer { Interval = 2000 };
            refreshTimer.Tick += (s, ev) => RefreshClientList();
            refreshTimer.Start();

            // 初始化客户端上报请求手动回复控件
            InitializeClientRequestReplyControls();
        }

        private void InitializeClientRequestReplyControls()
        {
            int ctrlY = rTxtLog.Bottom + 50;
            int ctrlX = rTxtLog.Left;


        }

        private void RefreshClientList()
        {
            if (kepServer == null) return;
            var ids = kepServer.GetOnlineClientIds();
            var current = cmbClients.SelectedItem?.ToString();
            cmbClients.Items.Clear();
            foreach (var id in ids)
                cmbClients.Items.Add(id);
            if (current != null && cmbClients.Items.Contains(current))
                cmbClients.SelectedItem = current;
            btnSendParam.Enabled = ServerState && cmbClients.Items.Count > 0;
        }

        private async void btnSendParam_Click(object sender, EventArgs e)
        {
            if (kepServer == null || cmbClients.SelectedItem == null)
            {
                AppendLog("没有可用的在线客户端");
                return;
            }

            string clientId = cmbClients.SelectedItem.ToString()!;
            string paramText = txtParam.Text;
            btnSendParam.Enabled = false;

            try
            {
                string reply = await kepServer.SendCommandToClientAsync(clientId, paramText);
                AppendLog($"[{clientId}] 参数回复: {reply}");
            }
            catch (TimeoutException ex)
            {
                AppendLog($"[{clientId}] {ex.Message}");
            }
            catch (Exception ex)
            {
                AppendLog($"[{clientId}] 下发失败: {ex.Message}");
            }
            finally
            {
                btnSendParam.Enabled = true;
            }
        }

        private void btnSwitch_Click(object sender, EventArgs e)
        {
            if (ServerState)
            {
                ServerState = false;
                btnSwitch.Text = "启动";
                _ = kepServer.StopAsync();
                kepServer.OnLog -= msg => Invoke(() => AppendLog(msg));
            }
            else
            {
                ServerState = true;
                btnSwitch.Text = "停止";
                kepServer = new KepServer(this.txtIpAddress.Text, Convert.ToInt32(this.txtPort.Text));
                kepServer.OnLog += msg => Invoke(() => AppendLog(msg));
                kepServer.OnAuthentication += KepServer_OnAuthentication;//客户端认证
                kepServer.OnClientOnline += KepServer_OnClientOnline;
                kepServer.OnClientOffline += KepServer_OnClientOffline;

                kepServer.OnClientRequestReceived += OnClientRequestReceivedHandler;
                kepServer.Start();
            }
        }



        BindingList<ClientDev> ClientList=new BindingList<ClientDev>();

        private void KepServer_OnClientOnline(KepConversation obj)
        {
            if (dgvClientList.InvokeRequired)
            {
                dgvClientList.Invoke(new Action(() =>
                {
                    ClientList.Add(new ClientDev { Id = obj.Id, SN = obj.SN, Token = obj.Token });
                }));
            }
            else
            {
                ClientList.Add(new ClientDev { Id = obj.Id, SN = obj.SN, Token = obj.Token });
            }
            
        }

        private void KepServer_OnClientOffline(string obj)
        {

            ClientList.Remove(ClientList.Where(_ => _.Id == obj).First());
            ;
        }

        private bool KepServer_OnAuthentication(ZepCommon.Identity arg)
        {
            if (arg.SN.Equals("72ZE126125192") && arg.Token.Equals("7929S82ASD892340SD9897234"))
            { 
                return true;
            }
            return false;
        }



        /// <summary>处理客户端上报请求事件（从后台线程调用）</summary>
        private void OnClientRequestReceivedHandler(string clientId, uint requestId, string requestData)
        {
            // 存储请求信息
            _pendingClientRequests[requestId] = (clientId, requestData);

            // 在UI线程更新ListBox
            Invoke(() =>
            {
                string displayText = $"[{clientId}] Id={requestId}: {requestData}";
                lstClientRequests.Items.Add(displayText);
                AppendLog($"收到客户端上报请求: {displayText}");
                btnReplyToClient.Enabled = lstClientRequests.Items.Count > 0;
            });
        }

        /// <summary>回复客户端按钮点击事件</summary>
        private async void btnReplyToClient_Click(object sender, EventArgs e)
        {
            if (lstClientRequests.SelectedIndex < 0)
            {
                AppendLog("请先选择一个客户端请求");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtReplyContent.Text))
            {
                AppendLog("请输入回复内容");
                return;
            }

            // 获取选中的请求信息
            string selectedItem = lstClientRequests.SelectedItem.ToString()!;
            // 从显示文本中提取requestId: 格式为 "[clientId] Id=xxx: requestData"
            int idStart = selectedItem.IndexOf("Id=") + 3;
            int idEnd = selectedItem.IndexOf(":", idStart);
            if (!uint.TryParse(selectedItem.Substring(idStart, idEnd - idStart), out uint requestId))
            {
                AppendLog("无法解析请求ID");
                return;
            }

            if (!_pendingClientRequests.TryGetValue(requestId, out var requestInfo))
            {
                AppendLog("请求信息已失效");
                lstClientRequests.Items.RemoveAt(lstClientRequests.SelectedIndex);
                btnReplyToClient.Enabled = lstClientRequests.Items.Count > 0;
                return;
            }

            string clientId = requestInfo.ClientId;
            string replyText = txtReplyContent.Text;

            btnReplyToClient.Enabled = false;
            try
            {
                await kepServer.ReplyToClientRequestAsync(clientId, requestId, replyText);
                AppendLog($"已回复客户端 [{clientId}]: {replyText}");

                // 移除已回复的请求
                _pendingClientRequests.TryRemove(requestId, out _);
                lstClientRequests.Items.RemoveAt(lstClientRequests.SelectedIndex);
                btnReplyToClient.Enabled = lstClientRequests.Items.Count > 0;
            }
            catch (Exception ex)
            {
                AppendLog($"回复失败: {ex.Message}");
                btnReplyToClient.Enabled = true;
            }
        }

        private void AppendLog(string message)
        {
            var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
            rTxtLog.AppendText(line);
        }

        private async void btnSendFile_Click(object sender, EventArgs e)
        {
            if (kepServer == null || cmbClients.SelectedItem == null)
            {
                AppendLog("没有可用的在线客户端");
                return;
            }

            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "选择要下发的文件";
            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            string clientId = cmbClients.SelectedItem.ToString()!;
            string filePath = dialog.FileName;
            btnSendFile.Enabled = false;

            try
            {
                await kepServer.SendFileToClientAsync(clientId, filePath);
                AppendLog($"[{clientId}] 文件下发成功: {Path.GetFileName(filePath)}");
            }
            catch (Exception ex)
            {
                AppendLog($"[{clientId}] 文件下发失败: {ex.Message}");
            }
            finally
            {
                btnSendFile.Enabled = true;
            }
        }

        private void btnKickClient_Click(object sender, EventArgs e)
        {
            if (kepServer == null || cmbClients.SelectedItem == null)
            {
                AppendLog("没有可用的在线客户端");
                return;
            }

            string clientId = cmbClients.SelectedItem.ToString()!;
            var result = MessageBox.Show($"确定要踢客户端 [{clientId}] 下线吗？", "确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes)
                return;

            bool success = kepServer.KickClient(clientId);
            if (success)
                AppendLog($"已踢客户端 [{clientId}] 下线");
            else
                AppendLog($"踢客户端 [{clientId}] 失败，可能已离线");
        }

        private void btnCleanLog_Click(object sender, EventArgs e)
        {
            rTxtLog.Text = string.Empty;
        }


    }


    public class ClientDev
    {
        public string Id { get; set; }
        public string SN { get; set; }
        public string Token { get; set; }
    }
}
