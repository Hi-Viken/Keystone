<template>
  <div class="app-container">
    <!-- 顶部工具栏 -->
    <el-row :gutter="10" class="mb8">
      <el-col>
        <el-button :type="serviceInfo.isEnable ? 'danger' : 'primary'"
          :icon="serviceInfo.isEnable ? 'VideoPause' : 'VideoPlay'" @click="handleSwitchStatus"
          :loading="statusLoading">
          {{ serviceInfo.isEnable ? '停止服务' : '启动服务' }}
        </el-button>
        <el-button type="success" icon="RefreshRight" @click="refreshStatus">刷新状态</el-button>
        <el-button type="primary" icon="Check" @click="submitConfig">保存配置</el-button>
      </el-col>
    </el-row>

    <!-- 运行状态卡片 -->
    <el-row :gutter="20">
      <el-col :span="8">
        <div class="status-card">
          <div class="card-title">运行状态</div>
          <div class="status-content">
            <el-tag :type="serviceInfo.isEnable ? 'success' : 'info'" size="large">
              {{ serviceInfo.isEnable ? '运行中' : '已停止' }}
            </el-tag>
            <div class="status-desc">
              <p>监听地址：<span>{{ serviceInfo.ipAddress }}</span></p>
              <p>监听端口：<span>{{ serviceInfo.port }}</span></p>
              <p>当前连接：<span>{{ serviceInfo.onlineCount }} / {{ serviceInfo.maxConn }}</span></p>
              <p>启动时间：<span>{{ serviceInfo.startTime || '-' }}</span></p>
            </div>
          </div>
        </div>
      </el-col>
      <el-col :span="8">
        <div class="status-card">
          <div class="card-title">认证模式</div>
          <div class="status-content">
            <el-tag :type="getAuthTag(serviceInfo.authMode)" size="large">
              {{ getAuthText(serviceInfo.authMode) }}
            </el-tag>
            <div class="status-desc">
              <p>连接密钥：
                <code class="secret-code">
                  {{ showSecret ? serviceInfo.secret : '••••••••••••' }}
                </code>
                <el-button link @click="showSecret = !showSecret" size="small">
                  <el-icon>
                    <View v-if="!showSecret" />
                    <Hide v-else />
                  </el-icon>
                </el-button>
              </p>
              <p>超时时间：<span>{{ serviceInfo.timeout }} 秒</span></p>
              <p>传输模式：<span>{{ serviceInfo.transMode === 'tcp' ? '原生TCP' : '二进制流' }}</span></p>
            </div>
          </div>
        </div>
      </el-col>
      <el-col :span="8">
        <div class="status-card">
          <div class="card-title">连接统计</div>
          <div class="status-content">
            <div class="stat-item">
              <div class="stat-num">{{ serviceInfo.totalConn }}</div>
              <div class="stat-label">累计连接数</div>
            </div>
            <div class="stat-item">
              <div class="stat-num">{{ serviceInfo.totalConn }}</div>
              <div class="stat-label">累计连接数</div>
            </div>
            <div class="stat-item">
              <div class="stat-num">{{ serviceInfo.failConn }}</div>
              <div class="stat-label">失败连接数</div>
            </div>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- 配置表单区域 -->
    <div class="config-card" style="margin-top: 20px;">
      <div class="card-title">服务参数配置</div>
      <el-form ref="configRef" :model="serviceInfo" :rules="rules" label-width="120px" style="padding: 20px 0;">
        <el-row :gutter="24">
          <el-col :span="12">
            <el-form-item label="监听地址" prop="ipAddress">
              <el-input v-model="serviceInfo.ipAddress" placeholder="例：0.0.0.0 / 127.0.0.1" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="监听端口" prop="port">
              <el-input v-model.number="serviceInfo.port" placeholder="端口范围 1-65535" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="24">
          <el-col :span="12">
            <el-form-item label="最大连接数" prop="maxConn">
              <el-input v-model.number="serviceInfo.maxConn" placeholder="最大并发客户端数" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="连接超时(秒)" prop="timeout">
              <el-input v-model.number="serviceInfo.timeout" placeholder="客户端空闲超时时间" />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="24">
          <el-col :span="12">
            <el-form-item label="认证模式" prop="authMode">
              <el-select v-model="serviceInfo.authMode" placeholder="选择TCP认证方式">
                <el-option label="无认证" value="none" />
                <el-option label="单向认证" value="single" />
                <el-option label="双向认证" value="double" />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="传输模式" prop="transMode">
              <el-select v-model="serviceInfo.transMode">
                <el-option label="原生TCP" value="tcp" />
                <el-option label="二进制流" value="stream" />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="连接密钥" prop="secret">
          <div style="display: flex; gap: 8px;">
            <el-input v-model="serviceInfo.secret" style="flex: 1;" placeholder="TCP连接认证密钥" />
            <el-button type="primary" :loading="genLoading" @click="generateSecret">
              <el-icon>
                <MagicStick />
              </el-icon> 随机生成
            </el-button>
          </div>
        </el-form-item>

        <el-form-item label="运行状态">
          <el-switch v-model="serviceInfo.isEnable" inline-prompt active-text="启用" inactive-text="禁用" />
        </el-form-item>

        <el-form-item label="备注说明">
          <el-input v-model="serviceInfo.remark" type="textarea" rows="3" placeholder="填写服务备注信息" />
        </el-form-item>
      </el-form>
    </div>
  </div>
</template>
<script setup name="tcpConfig">
import { ref, reactive } from 'vue'
import {
  View, Hide, VideoPlay, VideoPause,
  RefreshRight, Check, MagicStick
} from '@element-plus/icons-vue'
import { startServer, stopServer, GetInfo } from '@/api/frp/frpServer'
import { ElMessage } from 'element-plus'

const configRef = ref(null)
const statusLoading = ref(false)
const genLoading = ref(false)
const showSecret = ref(false)

// ===================== 模拟TCP服务全局数据（唯一服务） =====================
const serviceInfo = ref({
  // 基础网络配置
  ipAddress: '0.0.0.0',
  port: 8899,
  state: false,
  maxConn: 200,
  timeout: 60,
  // 认证与传输
  authMode: 'double',   // none 无认证 / single 单向 / double 双向认证
  transMode: 'tcp',
  secret: 'TCP_AUTH_2026_6688',
  // 运行状态
  isEnable: false,
  startTime: '2026-06-12 10:20:30',
  // 连接统计
  onlineCount: 16,
  totalConn: 1286,
  failConn: 23,
  // 备注
  remark: 'TCP双向认证服务，用于内网设备通信，端口8899，开启双向证书验证'
})

// ===================== 文本/标签转换 =====================
const getAuthText = (val) => {
  const map = { none: '无认证', single: '单向认证', double: '双向认证' }
  return map[val] || '未知'
}
const getAuthTag = (val) => {
  const map = { none: 'info', single: 'warning', double: 'success' }
  return map[val] || ''
}

// ===================== 表单校验规则 =====================
const rules = reactive({
  listenAddr: [
    { required: true, message: '监听地址不能为空', trigger: 'blur' }
  ],
  port: [
    { required: true, message: '监听端口不能为空', trigger: 'blur' },
    { pattern: /^([1-9]\d{0,4})$/, message: '端口必须在 1 ~ 65535 之间', trigger: 'blur' }
  ],
  maxConn: [
    { required: true, message: '最大连接数不能为空', trigger: 'blur' },
    { min: 1, message: '连接数不能小于1', trigger: 'blur' }
  ],
  timeout: [
    { required: true, message: '超时时间不能为空', trigger: 'blur' },
    { min: 1, message: '超时时间不能小于1秒', trigger: 'blur' }
  ],
  authMode: [
    { required: true, message: '请选择认证模式', trigger: 'change' }
  ],
  secret: [
    { required: true, message: '连接密钥不能为空', trigger: 'blur' }
  ]
})

// ===================== 事件方法 =====================
// 启停服务
const handleSwitchStatus = () => {
  statusLoading.value = true
  if (serviceInfo.value.isEnable) {
    stopServer().then(res => {
      if (res.code === 200) {
        ElMessage.success('服务已停止')
        serviceInfo.value.isEnable = false
      }
    })
  } else {
    startServer().then(res => {
      if (res.code === 200) {
        ElMessage.success('服务已启动')
        serviceInfo.value.isEnable = true
        serviceInfo.value.startTime = new Date().toLocaleString()


      }
    })
    getServiceInfo();
  }
  //     serviceInfo.onlineCount = 0
  //   } else {
  //     serviceInfo.onlineCount = 0
  //   }
  statusLoading.value = false
  //   ElMessage.success(serviceInfo.isEnable ? '服务启动成功' : '服务已停止')
  // }, 800)
}

// 刷新运行状态（模拟拉取实时数据）
const refreshStatus = () => {
  // 模拟随机在线连接数
  // if (serviceInfo.value.isEnable) {
  //   serviceInfo.value.onlineCount = Math.floor(Math.random() * serviceInfo.value.maxConn)
  //   serviceInfo.value.totalConn += Math.floor(Math.random() * 5)
  // }
  getServiceInfo();
  ElMessage.success('状态刷新完成')
}

// 随机生成密钥
const generateSecret = () => {
  genLoading.value = true
  setTimeout(() => {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_-'
    let str = ''
    for (let i = 0; i < 18; i++) {
      str += chars.charAt(Math.floor(Math.random() * chars.length))
    }
    serviceInfo.value.secret = str
    genLoading.value = false
    ElMessage.success('密钥已自动生成')
  }, 600)
}

// 保存配置
const submitConfig = () => {
  configRef.value.validate(valid => {
    if (!valid) return
    // 模拟保存配置
    setTimeout(() => {
      ElMessage.success('配置保存成功，参数已生效')
    }, 500)
  })
}

// 获取服务信息
const getServiceInfo = () => {
  GetInfo().then(res => {
    if (res.code === 200) {
      serviceInfo.value = res.data
      serviceInfo.value.isEnable = res.data.state;
      serviceInfo.value.maxConn = 100;
      console.log(serviceInfo.value)
    }
  })
}


onMounted(() => {
  getServiceInfo()
})

</script>
<style scoped>
/* 顶部状态卡片 */
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

/* 统计项 */
.stat-item {
  display: inline-block;
  width: 48%;
  text-align: center;
  margin-top: 10px;
}

.stat-num {
  font-size: 24px;
  font-weight: bold;
  color: var(--el-color-primary);
}

.stat-label {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

/* 配置卡片 */
.config-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 10px;
  padding: 16px;
}

/* 密钥样式 */
.secret-code {
  font-family: 'JetBrains Mono', Consolas, monospace;
  background: var(--el-fill-color-light);
  padding: 2px 6px;
  border-radius: 3px;
  font-size: 12px;
}
</style>