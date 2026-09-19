<template>
  <div class="app-container">
    <!-- 顶部操作按钮栏 -->
    <div class="btn-bar">
      <el-button
        :type="serverInfo.state ? 'danger' : 'primary'"
        :icon="serverInfo.state ? 'VideoPause' : 'VideoPlay'"
        @click="handleSwitchStatus"
        :loading="statusLoading"
      >
        {{ serverInfo.state ? '停止服务' : '启动服务' }}
      </el-button>
      <el-button type="success" icon="RefreshRight" @click="refreshStatus">刷新状态</el-button>
      <el-button type="primary" icon="Download" @click="loadLogs">加载日志</el-button>
      <el-button type="warning" icon="Delete" @click="handleClearLogs">清空日志</el-button>
      <el-button type="info" icon="DocumentAdd" @click="generateRandomLog">生成随机日志</el-button>
    </div>

    <el-row :gutter="20" class="row-gap">
      <el-col :span="8">
        <div class="card-wrap">
          <div class="card-title">运行状态</div>
          <div class="status-content">
            <el-tag :type="serverInfo.state ? 'success' : 'info'" size="large" effect="dark">
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
        <div class="card-wrap">
          <div class="card-title">实时日志 · 终端模式</div>
          <div class="terminal-container" ref="logContainer">
            <div class="terminal-line" v-for="(log, index) in logs" :key="index">
              <span class="terminal-time">[{{ formatTime(log.time) }}]</span>
              <span :class="['terminal-text', log.levelClass]">{{ log.message }}</span>
            </div>
            <span class="terminal-cursor">&nbsp;</span>
          </div>
        </div>
      </el-col>
    </el-row>

    <div class="card-wrap client-card-wrap">
      <div class="card-title">在线客户端列表</div>
      <el-table :data="clients" border stripe style="width: 100%" size="small">
        <el-table-column prop="clientId" label="客户端ID" />
        <el-table-column prop="sn" label="设备SN" />
        <el-table-column prop="token" label="Token" show-overflow-tooltip />
        <el-table-column prop="authenticated" label="认证状态">
          <template #default="scope">
            <el-tag :type="scope.row.authenticated ? 'success' : 'warning'" size="small">
              {{ scope.row.authenticated ? '已认证' : '未认证' }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="connectTime" label="连接时间" />
        <el-table-column prop="lastActivity" label="最后活跃" />
        <el-table-column label="操作" width="180">
          <template #default="scope">
            <el-button size="small" type="danger" text @click="handleKick(scope.row.clientId)">踢下线</el-button>
            <el-button size="small" type="primary" text @click="openCommandModal(scope.row)">下发命令</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <el-dialog v-model="commandModalVisible" title="下发命令" width="520px" draggable>
      <el-form :model="commandForm" label-width="90px">
        <el-form-item label="客户端ID">
          <el-input :value="commandForm.clientId" disabled />
        </el-form-item>
        <el-form-item label="命令参数">
          <el-input
            v-model="commandForm.paramText"
            type="textarea"
            :rows="5"
            placeholder="请输入需要下发的指令参数"
          />
        </el-form-item>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="commandModalVisible = false">取消</el-button>
          <el-button type="primary" @click="handleSendCommand" :loading="commandLoading">确认发送</el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script setup name="kepServer">
import { ref, reactive, onMounted, nextTick } from 'vue'
import { VideoPlay, VideoPause, RefreshRight, Download, Delete, DocumentAdd } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import kepHub from '@/signalr/KepHub.js'
import { GetServerStatus } from '@/api/Kep/KepServer.js'



const handleSwitchStatus = async () => {
  statusLoading.value = true
  try {
    if (!await kepHub.ensureConnected()) {
      ElMessage.error('连接失败，请检查网络')
      return
    }
    const result = await kepHub.SR.invoke('StateSwitch')
    serverInfo.state = result.state
    serverInfo.ipAddress = result.ipAddress
    serverInfo.port = result.port
    serverInfo.startTime = result.startTime
    serverInfo.onlineCount = result.onlineCount
    ElMessage.success(serverInfo.state ? '服务启动成功' : '服务停止成功')
  } catch (error) {
    ElMessage.error('操作失败: ' + error.message)
  } finally {
    statusLoading.value = false
  }
}




// 获取服务信息
const getServerStatus = () => {
  GetServerStatus().then(res => {
    if (res.code === 200) {
    serverInfo.state = res.data.state
    serverInfo.ipAddress = res.data.ipAddress
    serverInfo.port = res.data.port
    serverInfo.startTime = res.data.startTime
    serverInfo.onlineCount = res.data.onlineCount


      // serviceInfo.value = res.data
      // serviceInfo.value.isEnable = res.data.state;
      // serviceInfo.value.maxConn = 100;
      //console.log(serviceInfo.value)
    }
  })
}

let logHandler = null
let statusHandler = null
let OnlineHandler = null
let OfflineHandler = null




onMounted(async () => {
  getServerStatus()
  if (!await kepHub.ensureConnected()) {
    logs.value = [`[${new Date().toLocaleTimeString()}] 连接失败`]
    return
  }
 // 加入日志分组
  await kepHub.SR.invoke("SubscribeKepStatusPage")


  // 赋值保存回调
  logHandler = (msg) => {
    console.log('收到日志:', msg)
    pushLog(msg)
  }
  statusHandler = (result) => {
    serverInfo.state = result.state
    serverInfo.ipAddress = result.ipAddress
    serverInfo.port = result.port
    serverInfo.startTime = result.startTime
    serverInfo.onlineCount = result.onlineCount
  }
  
  OnlineHandler = (result) => {
    console.log('收到在线客户端sss:', result)
    //clients.value.push(result)
  }
  OfflineHandler = (result) => {
    console.log('收到下线客户端sss:', result)
    //clients.value = clients.value.filter(client => client.clientId !== result.clientId)
  }



  kepHub.SR.off('KepLog', logHandler)
  kepHub.SR.off('ServerStatus', statusHandler)
    kepHub.SR.off('ClientOnline', OnlineHandler)
      kepHub.SR.off('ClientOffline', OfflineHandler)
  // 订阅
  kepHub.SR.on('KepLog', logHandler)
  kepHub.SR.on('ServerStatus', statusHandler)
    kepHub.SR.on('ClientOnline', OnlineHandler)
      kepHub.SR.on('ClientOffline', OfflineHandler)
})

// 组件销毁解绑，关键！
onUnmounted(async () => {
  // 退出分组
  // if(kepHub.SR.state === signalR.HubConnectionState.Connected){
  //   await kepHub.SR.invoke("UnSubscribeKepStatusPage")
  // }
//   if (kepHub.SR.state === 1) {
  
// }
await kepHub.SR.invoke("UnSubscribeKepStatusPage")
  if (logHandler) {
    kepHub.SR.off('KepLog', logHandler)
  }
  if (statusHandler) {
    kepHub.SR.off('ServerStatus', statusHandler)
  }
  if (OnlineHandler) {
    kepHub.SR.off('ClientOnline', OnlineHandler)
  }
  if (OfflineHandler) {
    kepHub.SR.off('ClientOffline', OfflineHandler)
  }
})




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

// 日志列表
const logs = ref([])

const commandForm = reactive({
  clientId: '',
  paramText: ''
})

// 时间格式化 HH:mm:ss
const formatTime = (time) => {
  if (!time) return ''
  const d = new Date(time)
  const pad = (n) => n.toString().padStart(2, '0')
  return `${pad(d.getHours())}:${pad(d.getMinutes())}:${pad(d.getSeconds())}`
}

// // 启停服务模拟
// const handleSwitchStatus = () => {
//   statusLoading.value = true
//   setTimeout(() => {
//     if (serverInfo.state) {
//       serverInfo.state = false
//       serverInfo.startTime = ''
//       serverInfo.onlineCount = 0
//       clients.value = []
//       pushLog('>>> INFO 服务已手动停止，断开所有客户端连接', 'log-info')
//       ElMessage.success('服务停止成功')
//     } else {
//       serverInfo.state = true
//       serverInfo.startTime = new Date().toLocaleString()
//       serverInfo.onlineCount = clients.value.length
//       pushLog('>>> INFO KEP服务启动成功，监听地址 0.0.0.0:8888', 'log-info')
//       pushLog('>>> INFO 等待客户端接入...', 'log-info')
//       ElMessage.success('服务启动成功')
//     }
//     statusLoading.value = false
//   }, 800)
// }

// // 刷新状态
// const refreshStatus = () => {
//   serverInfo.onlineCount = clients.value.length
//   pushLog('>>> INFO 手动执行状态刷新，同步在线客户端数量', 'log-info')
//   ElMessage.success('状态刷新完成')
// }

// // 统一写入日志
 const pushLog = (msg, levelClass = 'log-info') => {
   logs.value.push({ time: new Date(), message: msg, levelClass })
   if (logs.value.length > 500) logs.value = logs.value.slice(-500)
   nextTick(() => scrollToBottom())
}

// // 随机日志生成
// const generateRandomLog = () => {
//   const logTypes = [
//     {
//       prefix: '>>> DEBUG',
//       class: 'log-debug',
//       texts: ['客户端心跳包校验通过', '配置文件加载完成', '缓冲区数据冲刷完毕', '端口监听轮询执行']
//     },
//     {
//       prefix: '>>> INFO',
//       class: 'log-info',
//       texts: ['客户端 client_001 正常上报数据', '消息队列入队成功', '连接会话保持正常', '定时任务调度执行']
//     },
//     {
//       prefix: '>>> WARN',
//       class: 'log-warn',
//       texts: ['客户端信号偏弱，延迟偏高', '内存占用接近阈值', '重复连接请求已拦截', '数据包轻微丢包检测']
//     },
//     {
//       prefix: '>>> ERROR',
//       class: 'log-error',
//       texts: ['数据解析异常，丢弃非法报文', '客户端超时断开连接', '读写缓冲区溢出保护触发', '认证令牌校验失败']
//     }
//   ]
//   const typeItem = logTypes[Math.floor(Math.random() * logTypes.length)]
//   const text = typeItem.texts[Math.floor(Math.random() * typeItem.texts.length)]
//   pushLog(`${typeItem.prefix} ${text}`, typeItem.class)
// }

// const loadLogs = () => {
//   pushLog('>>> INFO 加载历史日志记录完成', 'log-info')
//   ElMessage.success('日志加载完成')
// }

// const handleClearLogs = () => {
//   logs.value = []
//   ElMessage.success('日志已清空')
// }

// const handleKick = (clientId) => {
//   const idx = clients.value.findIndex(item => item.clientId === clientId)
//   if (idx > -1) {
//     const delClient = clients.value.splice(idx, 1)[0]
//     serverInfo.onlineCount = clients.value.length
//     pushLog(`>>> WARN 操作：踢下线 ${clientId} | SN:${delClient.sn}`, 'log-warn')
//     ElMessage.success('踢下线操作成功')
//   } else {
//     ElMessage.error('未找到该客户端')
//   }
// }

// const openCommandModal = (client) => {
//   commandForm.clientId = client.clientId
//   commandForm.paramText = ''
//   commandModalVisible.value = true
// }

// const handleSendCommand = () => {
//   if (!commandForm.paramText.trim()) {
//     ElMessage.warning('请输入命令参数')
//     return
//   }
//   commandLoading.value = true
//   setTimeout(() => {
//     pushLog(`>>> INFO 下发命令至 ${commandForm.clientId} : ${commandForm.paramText}`, 'log-info')
//     pushLog(`>>> INFO 客户端应答：命令执行完成`, 'log-info')
//     ElMessage.success('命令发送成功，响应：模拟执行完成')
//     commandModalVisible.value = false
//     commandLoading.value = false
//   }, 600)
// }

 const scrollToBottom = () => {
   if (logContainer.value) logContainer.value.scrollTop = logContainer.value.scrollHeight
 }







</script>

<style scoped>
.app-container {
  padding: 16px;
}

.btn-bar {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  margin-bottom: 16px;
}

.row-gap {
  margin-bottom: 16px;
}

.card-wrap {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 12px;
  padding: 18px;
  box-shadow: 0 2px 12px rgba(0, 0, 0, 0.06);
  height: 100%;
  box-sizing: border-box;
}

.client-card-wrap {
  margin-top: 4px;
}

.card-title {
  font-size: 15px;
  font-weight: 600;
  color: var(--el-text-color-primary);
  margin-bottom: 16px;
  padding-bottom: 10px;
  border-bottom: 1px dashed var(--el-border-color-extra-light);
}

.status-content {
  line-height: 1.85;
}

.status-desc {
  margin-top: 14px;
  font-size: 13px;
  color: var(--el-text-color-regular);
}

.status-desc span {
  font-family: 'Consolas', monospace;
  color: var(--el-color-primary);
}

/* 终端深度美化 */
.terminal-container {
  background-color: #0a0a0a;
  color: #33ff33;
  height: 220px;
  overflow: auto;
  padding: 12px 14px;
  border-radius: 8px;
  font-family: 'Consolas', 'Monaco', monospace;
  font-size: 13px;
  white-space: pre;
  line-height: 1.55;
  box-shadow: inset 0 0 14px rgba(0, 0, 0, 0.75);
  border: 1px solid #222;
}

/* 滚动条美化 */
.terminal-container::-webkit-scrollbar {
  width: 9px;
  height: 9px;
}
.terminal-container::-webkit-scrollbar-thumb {
  background: #444;
  border-radius: 5px;
}
.terminal-container::-webkit-scrollbar-track {
  background: #191919;
}

.terminal-line {
  word-break: break-all;
  white-space: pre-wrap;
}

.terminal-time {
  color: #88ff88;
  margin-right: 9px;
  user-select: none;
}

.terminal-text {
  color: #33ff33;
}

/* 日志级别配色 */
.log-debug { color: #80b8ff; }
.log-info  { color: #39ff39; }
.log-warn  { color: #ffdd44; }
.log-error { color: #ff4d4f; }

/* 终端闪烁光标 */
.terminal-cursor {
  display: inline-block;
  width: 9px;
  background-color: #39ff39;
  animation: blink 1s infinite;
  margin-left: 5px;
  vertical-align: bottom;
}
@keyframes blink {
  0%, 50% { opacity: 1; }
  51%, 100% { opacity: 0; }
}

.dialog-footer {
  text-align: right;
}
</style>