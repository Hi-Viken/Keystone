<template>
  <div class="app-container">
    <el-row :gutter="10" class="mb8">
      <el-col>
        <el-button :type="serverInfo.state ? 'danger' : 'primary'"
          :icon="serverInfo.state ? 'VideoPause' : 'VideoPlay'" @click="handleSwitchStatus"
          :loading="statusLoading">
          {{ serverInfo.state ? '停止服务' : '启动服务' }}
        </el-button>
        <el-button type="success" icon="RefreshRight" @click="refreshStatus">刷新状态</el-button>
        <el-button type="primary" icon="Download" @click="loadLogs">加载日志</el-button>
        <el-button type="warning" icon="Delete" @click="handleClearLogs">清空日志</el-button>
        <!-- 新增：生成随机日志按钮 -->
        <el-button type="info" icon="DocumentAdd" @click="generateRandomLog">生成随机日志</el-button>
      </el-col>
    </el-row>

    <el-row :gutter="20">
      <el-col :span="8">
        <div class="status-card">
          <div class="card-title">运行状态</div>
          <div class="status-content">
            <el-tag :type="serverInfo.state ? 'success' : 'info'" size="large">
              {{ serverInfo.state ? '运行中' : '已停止' }}
            </el-tag>
            <div class="status-desc">
              <p>监听地址：<span>{{ serverInfo.ipAddress }}</span></p>
              <p>监听端口：<span>{{ serverInfo.port }}</span></p>
              <p>在线客户端：<span>{{ serverInfo.onlineCount }}</span></p>
              <p>启动时间：<span>{{ serverInfo.startTime || '-' }}</span></p>
            </div>
          </div>
        </div>
      </el-col>
      <el-col :span="16">
        <div class="status-card">
          <div class="card-title">实时日志（终端模式）</div>
          <!-- 终端容器 -->
          <div class="terminal-container" ref="logContainer">
            <div class="terminal-line" v-for="(log, index) in logs" :key="index">
              <span class="terminal-time">[{{ formatTime(log.time) }}]</span>
              <span :class="['terminal-text', log.levelClass]">{{ log.message }}</span>
            </div>
            <!-- 模拟终端闪烁光标 -->
            <span class="terminal-cursor">&nbsp;</span>
          </div>
        </div>
      </el-col>
    </el-row>

    <div class="client-card" style="margin-top: 20px;">
      <div class="card-title">在线客户端</div>
      <el-table :data="clients" border style="width: 100%">
        <el-table-column prop="clientId" label="客户端ID" />
        <el-table-column prop="sn" label="设备SN" />
        <el-table-column prop="token" label="Token" />
        <el-table-column prop="authenticated" label="认证状态">
          <template #default="scope">
            <el-tag :type="scope.row.authenticated ? 'success' : 'warning'">
              {{ scope.row.authenticated ? '已认证' : '未认证' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="connectTime" label="连接时间" />
        <el-table-column prop="lastActivity" label="最后活跃" />
        <el-table-column label="操作">
          <template #default="scope">
            <el-button size="small" type="danger" @click="handleKick(scope.row.clientId)">踢下线</el-button>
            <el-button size="small" type="primary" @click="openCommandModal(scope.row)">下发命令</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <el-dialog v-model="commandModalVisible" title="下发命令" width="500px">
      <el-form :model="commandForm" label-width="80px">
        <el-form-item label="客户端ID">
          <el-input :value="commandForm.clientId" disabled />
        </el-form-item>
        <el-form-item label="命令参数">
          <el-input v-model="commandForm.paramText" type="textarea" :rows="4" placeholder="输入要下发的参数" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button @click="commandModalVisible = false">取消</el-button>
        <el-button type="primary" @click="handleSendCommand" :loading="commandLoading">发送</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup name="kepServer">
import { ref, reactive, onMounted, nextTick } from 'vue'
import { VideoPlay, VideoPause, RefreshRight, Download, Delete, DocumentAdd } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'

const statusLoading = ref(false)
const commandLoading = ref(false)
const commandModalVisible = ref(false)
const logContainer = ref(null)

// 服务状态假数据
const serverInfo = reactive({
  ipAddress: '0.0.0.0',
  port: 8888,
  state: false,
  onlineCount: 0,
  startTime: ''
})

// 在线客户端假数据
const clients = ref([
  {
    clientId: 'client_001',
    sn: 'SN20260703001',
    token: 'tk-abc123xyz789',
    authenticated: true,
    connectTime: new Date().toLocaleString(),
    lastActivity: new Date().toLocaleString()
  },
  {
    clientId: 'client_002',
    sn: 'SN20260703002',
    token: 'tk-def456uvw000',
    authenticated: false,
    connectTime: new Date().toLocaleString(),
    lastActivity: new Date().toLocaleString()
  }
])

// 日志列表：新增 levelClass 用来区分不同日志颜色
const logs = ref([])

const commandForm = reactive({
  clientId: '',
  paramText: ''
})

// 时间格式化
const formatTime = (time) => {
  if (!time) return ''
  const d = new Date(time)
  return `${d.getHours().toString().padStart(2, '0')}:${d.getMinutes().toString().padStart(2, '0')}:${d.getSeconds().toString().padStart(2, '0')}`
}

// 启停服务（前端模拟）
const handleSwitchStatus = () => {
  statusLoading.value = true
  setTimeout(() => {
    if (serverInfo.state) {
      // 停止服务
      serverInfo.state = false
      serverInfo.startTime = ''
      serverInfo.onlineCount = 0
      clients.value = []
      pushLog('>>> INFO 服务已手动停止，断开所有客户端连接', 'log-info')
      ElMessage.success('服务停止成功')
    } else {
      // 启动服务
      serverInfo.state = true
      serverInfo.startTime = new Date().toLocaleString()
      serverInfo.onlineCount = clients.value.length
      pushLog('>>> INFO KEP服务启动成功，监听地址 0.0.0.0:8888', 'log-info')
      pushLog('>>> INFO 等待客户端接入...', 'log-info')
      ElMessage.success('服务启动成功')
    }
    statusLoading.value = false
  }, 800)
}

// 刷新状态
const refreshStatus = () => {
  serverInfo.onlineCount = clients.value.length
  pushLog('>>> INFO 手动执行状态刷新，同步在线客户端数量', 'log-info')
  ElMessage.success('状态刷新完成')
}

// 追加一条日志（支持自定义样式类）
const pushLog = (msg, levelClass = 'log-info') => {
  logs.value.push({
    time: new Date(),
    message: msg,
    levelClass
  })
  // 最多保留500条
  if (logs.value.length > 500) {
    logs.value = logs.value.slice(-500)
  }
  nextTick(() => scrollToBottom())
}

// 【新增】生成随机日志
const generateRandomLog = () => {
  const logTypes = [
    { prefix: '>>> DEBUG', class: 'log-debug', texts: [
      '客户端心跳包校验通过',
      '配置文件加载完成',
      '缓冲区数据冲刷完毕',
      '端口监听轮询执行'
    ] },
    { prefix: '>>> INFO', class: 'log-info', texts: [
      '客户端 client_001 正常上报数据',
      '消息队列入队成功',
      '连接会话保持正常',
      '定时任务调度执行'
    ] },
    { prefix: '>>> WARN', class: 'log-warn', texts: [
      '客户端信号偏弱，延迟偏高',
      '内存占用接近阈值',
      '重复连接请求已拦截',
      '数据包轻微丢包检测'
    ] },
    { prefix: '>>> ERROR', class: 'log-error', texts: [
      '数据解析异常，丢弃非法报文',
      '客户端超时断开连接',
      '读写缓冲区溢出保护触发',
      '认证令牌校验失败'
    ] }
  ]
  // 随机类型
  const typeItem = logTypes[Math.floor(Math.random() * logTypes.length)]
  // 随机文案
  const text = typeItem.texts[Math.floor(Math.random() * typeItem.texts.length)]
  const fullMsg = `${typeItem.prefix} ${text}`
  pushLog(fullMsg, typeItem.class)
}

// 加载日志
const loadLogs = () => {
  pushLog('>>> INFO 加载历史日志记录完成', 'log-info')
  ElMessage.success('日志加载完成')
}

// 清空日志
const handleClearLogs = () => {
  logs.value = []
  ElMessage.success('日志已清空')
}

// 踢客户端下线
const handleKick = (clientId) => {
  const idx = clients.value.findIndex(item => item.clientId === clientId)
  if (idx > -1) {
    const delClient = clients.value.splice(idx, 1)[0]
    serverInfo.onlineCount = clients.value.length
    pushLog(`>>> WARN 操作：踢下线 ${clientId} | SN:${delClient.sn}`, 'log-warn')
    ElMessage.success('踢下线操作成功')
  } else {
    ElMessage.error('未找到该客户端')
  }
}

// 打开下发命令弹窗
const openCommandModal = (client) => {
  commandForm.clientId = client.clientId
  commandForm.paramText = ''
  commandModalVisible.value = true
}

// 发送命令模拟
const handleSendCommand = () => {
  if (!commandForm.paramText.trim()) {
    ElMessage.warning('请输入命令参数')
    return
  }
  commandLoading.value = true
  setTimeout(() => {
    pushLog(`>>> INFO 下发命令至 ${commandForm.clientId} : ${commandForm.paramText}`, 'log-info')
    pushLog(`>>> INFO 客户端应答：命令执行完成`, 'log-info')
    ElMessage.success('命令发送成功，响应：模拟执行完成')
    commandModalVisible.value = false
    commandLoading.value = false
  }, 600)
}

// 日志滚动到底部
const scrollToBottom = () => {
  if (logContainer.value) {
    logContainer.value.scrollTop = logContainer.value.scrollHeight
  }
}

onMounted(() => {
  pushLog('>>> INFO 系统初始化完毕，KEP服务当前处于停止状态', 'log-info')
  serverInfo.onlineCount = clients.value.length
})
</script>

<style scoped>
.status-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 10px;
  padding: 16px;
  height: 100%;
  box-sizing: border-box;
}

.card-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin-bottom: 14px;
  padding-bottom: 8px;
  border-bottom: 1px dashed var(--el-border-color-extra-light);
}

.status-content {
  line-height: 1.8;
}

.status-desc {
  margin-top: 12px;
  font-size: 13px;
  color: var(--el-text-color-regular);
}

.status-desc span {
  font-family: 'Consolas', monospace;
  color: var(--el-color-primary);
}

/* ========== 终端样式 ========== */
.terminal-container {
  background-color: #000;
  height: 200px;
  overflow: auto;
  padding: 10px 12px;
  border-radius: 6px;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 13px;
  white-space: pre;
  line-height: 1.5;
}
/* 自定义滚动条 */
.terminal-container::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}
.terminal-container::-webkit-scrollbar-thumb {
  background: #555;
  border-radius: 4px;
}
.terminal-container::-webkit-scrollbar-track {
  background: #222;
}

.terminal-line {
  word-break: break-all;
  white-space: pre-wrap;
}
.terminal-time {
  color: #88ff88;
  margin-right: 8px;
  user-select: none;
}
.terminal-text {
  color: #33ff33;
}
/* 不同日志级别颜色区分 */
.log-debug {
  color: #88aaff;
}
.log-info {
  color: #33ff33;
}
.log-warn {
  color: #ffdd33;
}
.log-error {
  color: #ff4444;
}

/* 闪烁光标动画 */
.terminal-cursor {
  display: inline-block;
  width: 8px;
  background-color: #33ff33;
  animation: blink 1s infinite;
  margin-left: 4px;
  vertical-align: bottom;
}
@keyframes blink {
  0%, 50% { opacity: 1; }
  51%, 100% { opacity: 0; }
}

.client-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 10px;
  padding: 16px;
}
</style>