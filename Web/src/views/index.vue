<template>
  <div class="linux-dashboard">
    <el-row :gutter="20" class="mb-4">
      <!-- 服务器信息 -->
      <el-col :span="6">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>服务器信息</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('server')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="info-content">
            <div class="hardware-item server-info-item">
              <label>Ip地址:</label>
              <span>{{ serverInfo.Ip }}</span>
            </div>
            <div class="hardware-item server-info-item">
              <label>主机名:</label>
              <span>{{ serverInfo.hostname }}</span>
            </div>
            <div class="hardware-item server-info-item">
              <label>系统版本:</label>
              <span>{{ serverInfo.osVersion }}</span>
            </div>
            <div class="hardware-item server-info-item">
              <label>内核版本:</label>
              <span>{{ serverInfo.kernelVersion }}</span>
            </div>
            <div class="hardware-item server-info-item">
              <label>启动模式:</label>
              <span>{{ serverInfo.boot_mode }}</span>
            </div>
            <div class="hardware-item server-info-item">
              <label>运行时间:</label>
              <span>{{ serverInfo.uptime }}</span>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 硬件设备列表 -->
      <el-col :span="6">
        <el-card class="info-card hardware-card">
          <template #header>
            <div class="card-header-custom">
              <span>硬件设备列表</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('hardware')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="hardware-list">
            <div v-for="(device, index) in hardwareList" :key="index" class="hardware-item">
              <div class="hardware-type">
                <el-tag :type="device.statusType">{{ device.type }}</el-tag>
              </div>
              <div class="hardware-details">
                <div class="hardware-name">{{ device.name }}</div>
                <div class="hardware-specs">{{ device.specs }}</div>
                <div class="hardware-status">
                  <el-icon :class="device.statusIconClass">
                    <component :is="device.statusIcon" />
                  </el-icon>
                  <span>{{ device.statusText }}</span>
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 系统日志 -->
      <el-col :span="12">
        <el-card class="info-card log-card">
          <template #header>
            <div class="card-header-custom">
              <span>系统日志</span>
              <div class="header-right">
                <el-select v-model="logType" size="small" style="width: 120px">
                  <el-option label="系统日志" value="syslog"></el-option>
                  <el-option label="错误日志" value="error"></el-option>
                  <el-option label="安全日志" value="security"></el-option>
                </el-select>
                <el-button type="text" size="small" class="detail-btn" @click="handleDetail('log')">
                  查看详情
                </el-button>
              </div>
            </div>
          </template>
          <div class="log-container">
            <div class="log-list">
              <div v-for="(log, index) in systemLogs" :key="index" class="log-item">
                <div class="log-time">{{ log.time }}</div>
                <div class="log-level">
                  <el-tag :type="getLogLevelTagType(log.level)">{{ log.level }}</el-tag>
                </div>
                <div class="log-content">{{ log.content }}</div>
              </div>
            </div>
            <el-button type="text" @click="loadMoreLogs" class="load-more">加载更多</el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <el-row :gutter="20" class="mb-4">
      <!-- CPU信息卡片 -->
      <el-col :span="6">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>CPU</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('cpu')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="cpu-container">
            <el-row :gutter="15" class="cpu-inner-row" type="flex" justify="center" align="stretch">
              <el-col :span="10" :xs="24" :sm="12" :md="10" class="cpu-chart-col">
                <div class="chart-wrapper">
                  <div id="cpu-total-ring-chart" class="ring-chart"></div>
                  <!-- CPU频率信息展示 -->
                  <div class="cpu-data-info">
                    <div class="cpu-data-item">
                      <label>总频率:</label>
                      <span>{{ cpuBasicInfo.totalFrequency }}</span>
                    </div>
                    <div class="cpu-data-item">
                      <label>当前频率:</label>
                      <span>{{ cpuBasicInfo.currentFrequency }}</span>
                    </div>
                  </div>
                </div>
              </el-col>
              <el-col :span="14" :xs="24" :sm="12" :md="14" class="cpu-list-col">
                <div class="cpu-list">
                  <div v-for="(cpu, index) in cpuList" :key="index" class="cpu-item">
                    <div class="cpu-name">{{ cpu.filesystem }} {{ cpu.mountPoint }}</div>
                    <el-progress :percentage="cpu.usage" :stroke-width="4" :color="cpuColorMethod"></el-progress>
                  </div>
                </div>
              </el-col>
            </el-row>
          </div>
        </el-card>
      </el-col>

      <!-- 内存信息卡片 -->
      <el-col :span="3">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>内存</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('mem')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="mem-container">
            <div class="chart-wrapper">
              <div id="mem-total-ring-chart" class="ring-chart"></div>
              <div class="mem-data-info">
                <div class="mem-data-item">
                  <label>总大小:</label>
                  <span>{{ memInfo.total }}</span>
                </div>
                <div class="mem-data-item">
                  <label>可用大小:</label>
                  <span>{{ memInfo.available }}</span>
                </div>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 磁盘分区 -->
      <el-col :span="3">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>磁盘分区</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('disk')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="disk-list-container">
            <div class="disk-list">
              <div v-for="(disk, index) in diskPartitionList" :key="index" class="disk-item">
                <div class="disk-name">{{ disk.partition }} ({{ disk.size }})</div>
                <el-progress :percentage="disk.usage" :stroke-width="3" :color="cpuColorMethod"></el-progress>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 硬盘IO -->
      <el-col :span="3">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>硬盘IO</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('io')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="io-list-container">
            <div class="io-list">
              <div v-for="(io, index) in diskIoList" :key="index" class="io-item">
                <div class="io-name">{{ io.device }}</div>
                <div class="io-read">读: <span class="value">{{ io.readSpeed }}</span></div>
                <div class="io-write">写: <span class="value">{{ io.writeSpeed }}</span></div>
                <el-progress :percentage="io.utilization" :stroke-width="3" :color="ioColorMethod"></el-progress>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 网络速率 -->
      <el-col :span="3">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>网络速率</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('net')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="net-list-container">
            <div class="net-list">
              <div v-for="(net, index) in netInterfaceList" :key="index" class="net-item">
                <div class="net-name">{{ net.interface }}</div>
                <div class="net-in">入: <span class="value">{{ net.inSpeed }}</span></div>
                <div class="net-out">出: <span class="value">{{ net.outSpeed }}</span></div>
                <el-progress :percentage="net.utilization" :stroke-width="3" :color="netColorMethod"></el-progress>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 进程监控 -->
      <el-col :span="3">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>进程监控</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('process')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="process-list-container">
            <div class="process-list">
              <div v-for="(proc, index) in processList" :key="index" class="process-item">
                <div class="process-name">{{ proc.name }}</div>
                <div class="process-pid">PID: {{ proc.pid }}</div>
                <div class="process-mem">内存: {{ proc.memUsage }}</div>
                <el-progress :percentage="proc.cpuUsage" :stroke-width="3" :color="procColorMethod"></el-progress>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>

      <!-- 系统负载 -->
      <el-col :span="3">
        <el-card class="info-card">
          <template #header>
            <div class="card-header-custom">
              <span>系统负载</span>
              <el-button type="text" size="small" class="detail-btn" @click="handleDetail('load')">
                查看详情
              </el-button>
            </div>
          </template>
          <div class="load-list-container">
            <div class="load-list">
              <div class="load-item">
                <div class="load-label">1分钟负载</div>
                <div class="load-value">{{ loadInfo.load1 }}</div>
                <el-progress :percentage="loadInfo.load1Percent" :stroke-width="3"
                  :color="loadColorMethod"></el-progress>
              </div>
              <div class="load-item">
                <div class="load-label">5分钟负载</div>
                <div class="load-value">{{ loadInfo.load5 }}</div>
                <el-progress :percentage="loadInfo.load5Percent" :stroke-width="3"
                  :color="loadColorMethod"></el-progress>
              </div>
              <div class="load-item">
                <div class="load-label">15分钟负载</div>
                <div class="load-value">{{ loadInfo.load15 }}</div>
                <el-progress :percentage="loadInfo.load15Percent" :stroke-width="3"
                  :color="loadColorMethod"></el-progress>
              </div>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- CPU + 内存 一行（两列平分） -->
    <el-row :gutter="20" class="mb-4">
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>CPU使用率监控</span>
              <el-select v-model="cpuChartTimeRange" size="small" style="width: 120px">
                <el-option label="最近1小时" value="1h"></el-option>
                <el-option label="最近6小时" value="6h"></el-option>
                <el-option label="最近24小时" value="24h"></el-option>
              </el-select>
            </div>
          </template>
          <div class="chart-container">
            <div id="cpu-chart" style="width: 100%; height: 400px"></div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>内存使用率监控</span>
              <el-select v-model="memChartTimeRange" size="small" style="width: 120px">
                <el-option label="最近1小时" value="1h"></el-option>
                <el-option label="最近6小时" value="6h"></el-option>
                <el-option label="最近24小时" value="24h"></el-option>
              </el-select>
            </div>
          </template>
          <div class="chart-container">
            <div id="mem-chart" style="width: 100%; height: 400px"></div>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- 磁盘IO + 网络流量 一行（两列平分） -->
    <el-row :gutter="20" class="mb-4">
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>磁盘IO监控</span>
              <el-select v-model="diskIoChartTimeRange" size="small" style="width: 120px">
                <el-option label="最近1小时" value="1h"></el-option>
                <el-option label="最近6小时" value="6h"></el-option>
                <el-option label="最近24小时" value="24h"></el-option>
              </el-select>
            </div>
          </template>
          <div class="chart-container">
            <div id="disk-io-chart" style="width: 100%; height: 400px"></div>
          </div>
        </el-card>
      </el-col>
      <el-col :span="12">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>网络流量监控</span>
              <el-select v-model="networkChartTimeRange" size="small" style="width: 120px">
                <el-option label="最近1小时" value="1h"></el-option>
                <el-option label="最近6小时" value="6h"></el-option>
                <el-option label="最近24小时" value="24h"></el-option>
              </el-select>
            </div>
          </template>
          <div class="chart-container">
            <div id="network-chart" style="width: 100%; height: 400px"></div>
          </div>
        </el-card>
      </el-col>
    </el-row>


  </div>
</template>

<script setup>
import { reactive, ref, onMounted } from 'vue'
import * as echarts from 'echarts'
import { Cpu, DataBoard, Check, Warning, InfoFilled, Message } from '@element-plus/icons-vue'
import { ElNotification } from 'element-plus'
// 各图表独立的时间范围
const cpuChartTimeRange = ref('1h')
const memChartTimeRange = ref('1h')
const diskIoChartTimeRange = ref('1h')
const networkChartTimeRange = ref('1h')


// 服务器信息数据
const serverInfo = reactive({
  hostname: 'linux-server-01',
  Ip: "10.0.0.134",
  osVersion: 'CentOS Linux release 7.9.2009 (Core)',
  kernelVersion: '3.10.0-1160.11.1.el7.x86_64',
  boot_mode: 'BIOS',
  uptime: '15天8小时23分钟',
})

// 内存详细信息数据
const memInfo = reactive({
  total: '64 GB',
  used: '6.4 GB',
  available: '57.6 GB',
  free: '48.2 GB',
  shared: '1.2 GB',
  buffCache: '8.5 GB',
  allocatable: '52.8 GB'
})

// CPU基础信息
const cpuBasicInfo = reactive({
  model: 'Intel(R) Xeon(R) CPU E5-2680 v4',
  physicalCpuCount: 2,
  physicalCoreCount: 16,
  logicalCoreCount: 32,
  frequency: '2.40GHz (Turbo up to 3.30GHz)',
  cache: '40MB L3 Cache',
  totalFrequency: '76.8 GHz',
  currentFrequency: '2.65 GHz'
})

// CPU占用Top5进程数据
const cpuTopProcesses = ref([
  { name: 'java', pid: 3456, cpuPercent: '22.5%', operation: '查看详情/终止进程' },
  { name: 'mysql', pid: 5678, cpuPercent: '15.7%', operation: '查看详情/终止进程' },
  { name: 'nginx', pid: 1234, cpuPercent: '8.2%', operation: '查看详情/终止进程' },
  { name: 'redis', pid: 9012, cpuPercent: '3.1%', operation: '查看详情/终止进程' },
  { name: 'sshd', pid: 7890, cpuPercent: '0.5%', operation: '查看详情/终止进程' }
])

// 内存占用Top5进程数据
const memTopProcesses = ref([
  { name: 'java', pid: 3456, memSize: '26.9 GB', memPercent: '42.1%', operation: '查看详情/终止进程' },
  { name: 'mysql', pid: 5678, memSize: '18.1 GB', memPercent: '28.3%', operation: '查看详情/终止进程' },
  { name: 'nginx', pid: 1234, memSize: '7.9 GB', memPercent: '12.5%', operation: '查看详情/终止进程' },
  { name: 'redis', pid: 9012, memSize: '3.7 GB', memPercent: '5.8%', operation: '查看详情/终止进程' },
  { name: 'sshd', pid: 7890, memSize: '0.8 GB', memPercent: '1.2%', operation: '查看详情/终止进程' }
])

// 其他基础数据
const diskPartitionList = ref([
  { partition: '/', size: '200G', usage: 75.2 },
  { partition: '/data', size: '200G', usage: 58.5 },
  { partition: '/boot', size: '1G', usage: 30.1 },
  { partition: '/tmp', size: '50G', usage: 45.8 },
  { partition: '/home', size: '80G', usage: 62.3 },
  { partition: '/var', size: '60G', usage: 78.9 }
])
const diskIoList = ref([
  { device: 'sda', readSpeed: '12.3 MB/s', writeSpeed: '8.5 MB/s', utilization: 35.2 },
  { device: 'sdb', readSpeed: '0.8 MB/s', writeSpeed: '2.1 MB/s', utilization: 12.8 },
  { device: 'sdc', readSpeed: '45.6 MB/s', writeSpeed: '32.4 MB/s', utilization: 68.5 },
  { device: 'sdd', readSpeed: '0.0 MB/s', writeSpeed: '0.5 MB/s', utilization: 5.1 },
  { device: 'md0', readSpeed: '28.7 MB/s', writeSpeed: '18.2 MB/s', utilization: 48.9 }
])
const netInterfaceList = ref([
  { interface: 'eth0', inSpeed: '5.2 MB/s', outSpeed: '3.8 MB/s', utilization: 22.5 },
  { interface: 'eth1', inSpeed: '120.5 MB/s', outSpeed: '98.3 MB/s', utilization: 75.8 },
  { interface: 'lo', inSpeed: '0.1 MB/s', outSpeed: '0.1 MB/s', utilization: 0.5 },
  { interface: 'docker0', inSpeed: '8.3 MB/s', outSpeed: '6.5 MB/s', utilization: 15.2 }
])
const processList = ref([
  { name: 'nginx', pid: 1234, memUsage: '12.5%', cpuUsage: 8.2 },
  { name: 'mysql', pid: 5678, memUsage: '28.3%', cpuUsage: 15.7 },
  { name: 'redis', pid: 9012, memUsage: '5.8%', cpuUsage: 3.1 },
  { name: 'java', pid: 3456, memUsage: '42.1%', cpuUsage: 22.5 },
  { name: 'sshd', pid: 7890, memUsage: '1.2%', cpuUsage: 0.5 }
])
const loadInfo = reactive({
  load1: '0.85',
  load1Percent: 42.5,
  load5: '0.62',
  load5Percent: 31.0,
  load15: '0.48',
  load15Percent: 24.0
})
const hardwareList = ref([
  {
    type: 'CPU',
    statusType: 'success',
    name: 'Intel(R) Xeon(R) CPU E5-2680 v4',
    specs: '16核心 / 32线程 / 2.40GHz',
    statusIcon: 'Check',
    statusIconClass: 'success',
    statusText: '正常运行'
  },
  {
    type: '内存',
    statusType: 'success',
    name: 'DDR4 ECC 内存条',
    specs: '64GB (4×16GB) / 2666MHz',
    statusIcon: 'Check',
    statusIconClass: 'success',
    statusText: '正常'
  },
  {
    type: '磁盘',
    statusType: 'warning',
    name: 'Samsung SSD 870 EVO',
    specs: '500GB / SATA III / 读取560MB/s',
    statusIcon: 'Warning',
    statusIconClass: 'warning',
    statusText: '使用率偏高'
  },
  {
    type: '网卡',
    statusType: 'success',
    name: 'Intel I350-T4',
    specs: '4×1GbE / RJ45 / PCIe 2.1',
    statusIcon: 'Check',
    statusIconClass: 'success',
    statusText: '链路正常'
  }
])
const logType = ref('syslog')
const systemLogs = ref([
  {
    time: '2025-11-25 15:45:22',
    level: 'INFO',
    content: 'systemd[1]: Started Session 1234 of user root.'
  },
  {
    time: '2025-11-25 15:30:10',
    level: 'WARNING',
    content: 'kernel: [12345.67890] TCP: request_sock_TCP: Possible SYN flooding on port 80. Sending cookies.'
  },
  {
    time: '2025-11-25 15:15:05',
    level: 'ERROR',
    content: 'sshd[12345]: error: PAM: Authentication failure for root from 192.168.1.100'
  }
])
const cpuList = ref([
  { filesystem: 'Sum', mountPoint: '(总占用)', usage: 90.345 },
  { filesystem: 'Core-1', mountPoint: '(CPU核心1)', usage: 80.821 },
  { filesystem: 'Core-2', mountPoint: '(CPU核心2)', usage: 80.12 },
  { filesystem: 'Core-3', mountPoint: '(CPU核心3)', usage: 70.123 },
  { filesystem: 'Core-4', mountPoint: '(CPU核心4)', usage: 60.123 }
])

// 查看详情按钮点击事件
const handleDetail = (type) => {
  // 这里可以根据不同类型跳转到对应的详情页面，或打开详情弹窗
  console.log(`查看 ${type} 详情`)
  // 示例：可结合路由跳转
  // router.push(`/dashboard/detail/${type}`)
}

const loadMoreLogs = () => {
  systemLogs.value.push(
    {
      time: '2025-11-25 13:15:00',
      level: 'INFO',
      content: 'systemd[1]: Starting Cleanup of Temporary Directories...'
    }
  )
}

const getLogLevelTagType = (level) => {
  switch (level) {
    case 'INFO': return 'info'
    case 'WARNING': return 'warning'
    case 'ERROR': return 'danger'
    default: return 'info'
  }
}

const cpuColorMethod = (value) => {
  if (value <= 65) return '#67c23a'
  if (value <= 75) return '#409EFF'
  if (value <= 85) return '#e6a23c'
  return '#f56c6c'
}
const ioColorMethod = (value) => {
  if (value <= 40) return '#67c23a'
  if (value <= 70) return '#e6a23c'
  return '#f56c6c'
}
const netColorMethod = (value) => {
  if (value <= 30) return '#67c23a'
  if (value <= 60) return '#409EFF'
  return '#f56c6c'
}
const procColorMethod = (value) => {
  if (value <= 10) return '#67c23a'
  if (value <= 20) return '#409EFF'
  return '#f56c6c'
}
const loadColorMethod = (value) => {
  if (value <= 30) return '#67c23a'
  if (value <= 50) return '#409EFF'
  return '#f56c6c'
}

// ECharts实例
let cpuTotalRingChart = null
let memTotalRingChart = null

// 环形图初始化
const initRingChart = (domId, title, value, type = 'default') => {
  const chartDom = document.getElementById(domId)
  if (!chartDom) return null
  const chart = echarts.init(chartDom)

  const getCpuTooltipContent = () => {
    const coreUsageRows = cpuList.value.slice(1).map(cpu => `
      <tr><td>${cpu.filesystem}</td><td>${cpu.usage.toFixed(1)}%</td></tr>
    `).join('');
    const cpuTopProcessRows = cpuTopProcesses.value.map(proc => `
      <tr><td>${proc.name}</td><td>${proc.pid}</td><td>${proc.cpuPercent}</td><td><a href="javascript:;" class="op-link">${proc.operation}</a></td></tr>
    `).join('');

    return `
      <div class="cpu-tooltip-collapse">
        <div class="collapse-header active"><span>CPU基础信息</span><i class="el-icon-arrow-down"></i></div>
        <div class="collapse-content show">
          <table class="cpu-info-table">
            <tr><td>CPU型号:</td><td>${cpuBasicInfo.model}</td></tr>
            <tr><td>物理CPU个数:</td><td>${cpuBasicInfo.physicalCpuCount}</td></tr>
            <tr><td>总频率:</td><td>${cpuBasicInfo.totalFrequency}</td></tr>
            <tr><td>当前频率:</td><td>${cpuBasicInfo.currentFrequency}</td></tr>
          </table>
        </div>
      </div>
      <div class="cpu-tooltip-collapse">
        <div class="collapse-header"><span>核心使用率</span><i class="el-icon-arrow-right"></i></div>
        <div class="collapse-content"><table class="cpu-core-table"><thead><tr><th>核心名称</th><th>使用率</th></tr></thead><tbody>${coreUsageRows}</tbody></table></div>
      </div>
      <div class="cpu-tooltip-collapse">
        <div class="collapse-header"><span>CPU占用Top5进程</span><i class="el-icon-arrow-right"></i></div>
        <div class="collapse-content"><table class="cpu-process-table"><thead><tr><th>进程名</th><th>ID</th><th>占比</th><th>操作</th></tr></thead><tbody>${cpuTopProcessRows}</tbody></table></div>
      </div>
    `;
  }

  const getMemTooltipContent = () => {
    const memTopProcessRows = memTopProcesses.value.map(proc => `
      <tr><td>${proc.name}</td><td>${proc.pid}</td><td>${proc.memSize}</td><td>${proc.memPercent}</td><td><a href="javascript:;" class="op-link">${proc.operation}</a></td></tr>
    `).join('');

    return `
      <div class="mem-tooltip-collapse">
        <div class="collapse-header active"><span>内存详细信息</span><i class="el-icon-arrow-down"></i></div>
        <div class="collapse-content show">
          <table class="mem-info-table">
            <tr><td>总内存:</td><td>${memInfo.total}</td></tr>
            <tr><td>空闲内存:</td><td>${memInfo.free}</td></tr>
            <tr><td>可用大小:</td><td>${memInfo.available}</td></tr>
          </table>
        </div>
      </div>
      <div class="mem-tooltip-collapse">
        <div class="collapse-header"><span>内存占用Top5进程</span><i class="el-icon-arrow-right"></i></div>
        <div class="collapse-content"><table class="mem-process-table"><thead><tr><th>进程名</th><th>ID</th><th>内存大小</th><th>占比</th><th>操作</th></tr></thead><tbody>${memTopProcessRows}</tbody></table></div>
      </div>
    `;
  }

  let formatter = `${title} <br/>{b}: {c}% ({d}%)`;
  let isCustomTooltip = false;
  if (type === 'cpu') {
    formatter = () => getCpuTooltipContent();
    isCustomTooltip = true;
  } else if (type === 'mem') {
    formatter = () => getMemTooltipContent();
    isCustomTooltip = true;
  }

  const option = {
    tooltip: {
      trigger: 'item',
      formatter: formatter,
      appendToBody: true,
      position: ['50%', '50%'],
      textStyle: { fontSize: 12 },
      padding: isCustomTooltip ? [10, 10] : [8, 12],
      triggerOn: 'mousemove',
      backgroundColor: 'rgba(255, 255, 255, 0.95)',
      borderColor: '#e6e6e6',
      borderWidth: 1,
      boxShadow: '0 2px 12px 0 rgba(0, 0, 0, 0.1)',
      enterable: isCustomTooltip,
      extraCssText: isCustomTooltip ? 'width: 450px !important; height: auto !important;' : ''
    },
    series: [
      {
        name: title,
        type: 'pie',
        radius: ['40%', '70%'],
        center: ['50%', '50%'],
        avoidLabelOverlap: false,
        label: { show: true, position: 'center', fontSize: 16, fontWeight: 'bold', formatter: `${value.toFixed(1)}%` },
        labelLine: { show: false },
        data: [
          { value: value, name: '已占用' },
          { value: 100 - value, name: '空闲', itemStyle: { color: '#f5f5f5' } }
        ],
        itemStyle: { color: cpuColorMethod(value) }
      }
    ]
  }

  chart.setOption(option)

  if (isCustomTooltip) {
    chart.on('tooltipShow', () => {
      setTimeout(() => {
        const collapseHeaders = document.querySelectorAll(`.${type}-tooltip-collapse .collapse-header`);
        collapseHeaders.forEach(header => {
          header.addEventListener('click', () => {
            const parent = header.parentElement;
            const content = parent.querySelector('.collapse-content');
            const icon = header.querySelector('i');
            if (header.classList.contains('active')) {
              header.classList.remove('active');
              content.classList.remove('show');
              icon.classList.replace('el-icon-arrow-down', 'el-icon-arrow-right');
            } else {
              document.querySelectorAll(`.${type}-tooltip-collapse .collapse-header`).forEach(h => {
                if (h !== header) {
                  h.classList.remove('active');
                  h.querySelector('i').classList.replace('el-icon-arrow-down', 'el-icon-arrow-right');
                  h.parentElement.querySelector('.collapse-content').classList.remove('show');
                }
              });
              header.classList.add('active');
              content.classList.add('show');
              icon.classList.replace('el-icon-arrow-right', 'el-icon-arrow-down');
            }
          });
        });
      }, 0);
    });
  }

  return chart
}


// 图表实例
let cpuChart = null
let memChart = null
let diskIoChart = null
let networkChart = null
let dataUpdateTimer = null
// 生成模拟图表数据
const generateChartData = (timeRange) => {
  const hours = timeRange === '1h' ? 1 : timeRange === '6h' ? 6 : 24
  const points = hours * 12 // 每5分钟一个数据点
  const cpuData = []
  const memData = []
  const ioReadData = []
  const ioWriteData = []
  const netInData = []
  const netOutData = []
  const xAxis = []

  const baseTime = new Date()
  baseTime.setMinutes(0)
  baseTime.setSeconds(0)

  for (let i = 0; i < points; i++) {
    const time = new Date(baseTime)
    time.setMinutes(time.getMinutes() - (points - i - 1) * 5)
    xAxis.push(time.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }))

    // 生成模拟数据
    const cpuBase = 60 + Math.random() * 20
    cpuData.push(Math.round((cpuBase + Math.sin(i / 6) * 10) * 100) / 100)

    const memBase = 75 + Math.random() * 5
    memData.push(Math.round((memBase + Math.sin(i / 8) * 5) * 100) / 100)

    ioReadData.push(Math.round((50 + Math.random() * 30 + Math.sin(i / 4) * 20) * 100) / 100)
    ioWriteData.push(Math.round((80 + Math.random() * 40 + Math.cos(i / 5) * 30) * 100) / 100)

    netInData.push(Math.round((100 + Math.random() * 50 + Math.sin(i / 7) * 40) * 100) / 100)
    netOutData.push(Math.round((80 + Math.random() * 60 + Math.cos(i / 9) * 50) * 100) / 100)
  }



  return {
    xAxis,
    cpuData,
    memData,
    ioReadData,
    ioWriteData,
    netInData,
    netOutData
  }
}

// 更新图表
const updateCharts = () => {
  const cpuData = generateChartData(cpuChartTimeRange.value)
  const memData = generateChartData(memChartTimeRange.value)
  const diskIoData = generateChartData(diskIoChartTimeRange.value)
  const networkData = generateChartData(networkChartTimeRange.value)

  // CPU图表配置
  const cpuOption = {
    tooltip: { trigger: 'axis', axisPointer: { type: 'cross' } },
    legend: { data: ['CPU使用率(%)'], top: 0 },
    grid: { left: '3%', right: '4%', bottom: '3%', top: '15%', containLabel: true },
    xAxis: { type: 'category', boundaryGap: false, data: cpuData.xAxis },
    yAxis: { type: 'value', min: 0, max: 100, name: '百分比 (%)' },
    series: [{
      name: 'CPU使用率(%)',
      type: 'line',
      data: cpuData.cpuData,
      smooth: true,
      lineStyle: { width: 2 },
      itemStyle: { color: '#f56c6c' },
      areaStyle: {
        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: 'rgba(245, 108, 108, 0.3)' },
          { offset: 1, color: 'rgba(245, 108, 108, 0.05)' }
        ])
      }
    }]
  }

  // 内存图表配置
  const memOption = {
    tooltip: { trigger: 'axis', axisPointer: { type: 'cross' } },
    legend: { data: ['内存使用率(%)'], top: 0 },
    grid: { left: '3%', right: '4%', bottom: '3%', top: '15%', containLabel: true },
    xAxis: { type: 'category', boundaryGap: false, data: memData.xAxis },
    yAxis: { type: 'value', min: 0, max: 100, name: '百分比 (%)' },
    series: [{
      name: '内存使用率(%)',
      type: 'line',
      data: memData.memData,
      smooth: true,
      lineStyle: { width: 2 },
      itemStyle: { color: '#409eff' },
      areaStyle: {
        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [
          { offset: 0, color: 'rgba(64, 158, 255, 0.3)' },
          { offset: 1, color: 'rgba(64, 158, 255, 0.05)' }
        ])
      }
    }]
  }

  // 磁盘IO图表配置
  const diskIoOption = {
    tooltip: { trigger: 'axis', axisPointer: { type: 'cross' } },
    legend: { data: ['磁盘读(MB/s)', '磁盘写(MB/s)'], top: 0 },
    grid: { left: '3%', right: '4%', bottom: '3%', top: '15%', containLabel: true },
    xAxis: { type: 'category', boundaryGap: false, data: diskIoData.xAxis },
    yAxis: { type: 'value', min: 0, name: '速度 (MB/s)' },
    series: [
      {
        name: '磁盘读(MB/s)',
        type: 'line',
        data: diskIoData.ioReadData,
        smooth: true,
        lineStyle: { width: 2 },
        itemStyle: { color: '#67c23a' }
      },
      {
        name: '磁盘写(MB/s)',
        type: 'line',
        data: diskIoData.ioWriteData,
        smooth: true,
        lineStyle: { width: 2 },
        itemStyle: { color: '#e6a23c' }
      }
    ]
  }

  // 网络流量图表配置
  const networkOption = {
    tooltip: { trigger: 'axis', axisPointer: { type: 'cross' } },
    legend: { data: ['网络入(MB/s)', '网络出(MB/s)'], top: 0 },
    grid: { left: '3%', right: '4%', bottom: '3%', top: '15%', containLabel: true },
    xAxis: { type: 'category', boundaryGap: false, data: networkData.xAxis },
    yAxis: { type: 'value', min: 0, name: '速度 (MB/s)' },
    series: [
      {
        name: '网络入(MB/s)',
        type: 'line',
        data: networkData.netInData,
        smooth: true,
        lineStyle: { width: 2, type: 'dashed' },
        itemStyle: { color: '#909399' }
      },
      {
        name: '网络出(MB/s)',
        type: 'line',
        data: networkData.netOutData,
        smooth: true,
        lineStyle: { width: 2, type: 'dashed' },
        itemStyle: { color: '#f56c6c' }
      }
    ]
  }

  cpuChart.setOption(cpuOption)
  memChart.setOption(memOption)
  diskIoChart.setOption(diskIoOption)
  networkChart.setOption(networkOption)


}

// 窗口大小变化时调整图表
const resizeCharts = () => {
  if (cpuChart) cpuChart.resize()
  if (memChart) memChart.resize()
  if (diskIoChart) diskIoChart.resize()
  if (networkChart) networkChart.resize()
  if (cpuTotalRingChart) cpuTotalRingChart.resize()
  if (memTotalRingChart) memTotalRingChart.resize()
}
// 初始化图表
const initCharts = () => {
  // 初始化原有图表
  cpuChart = echarts.init(document.getElementById('cpu-chart'))
  memChart = echarts.init(document.getElementById('mem-chart'))
  diskIoChart = echarts.init(document.getElementById('disk-io-chart'))
  networkChart = echarts.init(document.getElementById('network-chart'))




  updateCharts()
}

onMounted(() => {
  const cpuTotalUsage = cpuList.value[0].usage
  cpuTotalRingChart = initRingChart('cpu-total-ring-chart', 'CPU总占用', cpuTotalUsage, 'cpu')
  const memTotalUsage = 10
  memTotalRingChart = initRingChart('mem-total-ring-chart', '内存总占用', memTotalUsage, 'mem')
  clearInterval(dataUpdateTimer)

  initCharts();

  window.addEventListener('resize', () => {
    cpuTotalRingChart?.resize()
    memTotalRingChart?.resize()
  })
  window.addEventListener('resize', resizeCharts)
  dataUpdateTimer = setInterval(() => {
    updateCharts()
  }, 30000)

})

</script>

<style scoped>
.linux-dashboard {
  /* padding: 20px; */
  min-height: 100vh;
  overflow: visible;
}

.mb-4 {
  margin-bottom: 20px;
  display: flex;
  flex-wrap: wrap;
  align-items: stretch;
  overflow: visible;
}

.info-card {
  height: 100%;
  box-sizing: border-box;
  overflow: hidden !important;
  max-width: 100%;
}

/* 卡片头部自定义样式 */
.card-header-custom {
  display: flex;
  justify-content: space-between;
  align-items: center;
  width: 100%;
}

/* 查看详情按钮样式 */
.detail-btn {
  color: #409eff;
  padding: 0;
  font-size: 12px;
}

.detail-btn:hover {
  color: #66b1ff;
}

/* 系统日志头部右侧布局 */
.header-right {
  display: flex;
  align-items: center;
  gap: 8px;
}

.info-header {
  display: flex;
  align-items: center;
  margin-bottom: 15px;
  font-size: 16px;
  font-weight: 600;
  color: #1989fa;
}

.info-content {
  font-size: 14px;
}

.server-info-item {
  display: flex;
  align-items: center;
  justify-content: flex-start;
  padding: 8px 0;
  border-bottom: 1px solid #f5f5f5;
}

.server-info-item label {
  min-width: 70px;
  font-weight: 500;
}

.server-info-item span {
  text-align: center;
}

.hardware-list {
  max-height: 300px;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 5px;
}

.hardware-list::-webkit-scrollbar {
  width: 6px;
}

.hardware-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.hardware-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.hardware-item {
  display: flex;
  align-items: flex-start;
  padding: 10px 0;
  border-bottom: 1px solid #f5f5f5;
}

.hardware-type {
  min-width: 60px;
  margin-right: 10px;
}

.hardware-details {
  flex: 1;
}

.hardware-name {
  font-weight: 600;
  margin-bottom: 4px;
}

.hardware-specs {
  font-size: 12px;
  color: #666;
  margin-bottom: 4px;
}

.hardware-status {
  display: flex;
  align-items: center;
  font-size: 12px;
  color: #333;
}

.hardware-status .el-icon {
  margin-right: 4px;
  font-size: 14px;
}

.hardware-status .success {
  color: #67c23a;
}

.hardware-status .warning {
  color: #e6a23c;
}

.hardware-status .info {
  color: #409eff;
}

.log-card .log-container {
  padding: 0 5px;
  height: 300px;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
}

.log-list {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  margin-bottom: 8px;
  padding-right: 5px;
}

.log-list::-webkit-scrollbar {
  width: 6px;
}

.log-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.log-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.log-item {
  display: flex;
  padding: 8px 0;
  border-bottom: 1px solid #f0f0f0;
  font-size: 13px;
}

.log-time {
  width: 180px;
  color: #999;
  flex-shrink: 0;
}

.log-level {
  width: 80px;
  flex-shrink: 0;
  text-align: center;
}

.log-content {
  flex: 1;
  word-break: break-all;
  padding-left: 10px;
}

.load-more {
  display: block;
  margin: 0 auto;
  color: #409eff;
}

.cpu-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 10px 0;
  width: 100%;
  height: calc(100% - 40px);
  box-sizing: border-box;
  overflow: visible;
}

.cpu-inner-row {
  display: flex;
  flex-wrap: wrap;
  align-items: stretch;
  justify-content: center;
  padding: 5px;
  width: 100%;
  max-width: 100%;
  box-sizing: border-box;
  overflow-x: hidden;
}

.cpu-chart-col {
  display: flex;
  flex-direction: column;
  min-width: 120px;
  flex-shrink: 0;
}

.cpu-list-col {
  height: 260px;
  overflow-y: auto;
  overflow-x: hidden;
  display: flex;
  flex-direction: column;
  min-width: 150px;
  max-width: calc(100% - 130px);
  flex-shrink: 0;
}

.cpu-list {
  flex: 1;
  overflow-y: auto;
  overflow-x: hidden;
  padding: 2px 10px;
  box-sizing: border-box;
}

.cpu-list::-webkit-scrollbar {
  width: 6px;
}

.cpu-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.cpu-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.cpu-item {
  margin-bottom: 8px;
  width: 100%;
  word-break: break-all;
}

.cpu-name {
  font-weight: 500;
  margin-bottom: 5px;
  font-size: 13px;
}

/* CPU频率信息样式 */
.cpu-data-info {
  width: 100%;
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid #f5f5f5;
}

.cpu-data-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 0;
  font-size: 12px;
}

.cpu-data-item label {
  color: #666;
  min-width: 60px;
}

.cpu-data-item span {
  color: #333;
  font-weight: 500;
}

.mem-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 10px 0;
  width: 100%;
  height: calc(100% - 40px);
  box-sizing: border-box;
  overflow: visible;
}

.mem-data-info {
  width: 100%;
  margin-top: 10px;
  padding-top: 10px;
  border-top: 1px solid #f5f5f5;
}

.mem-data-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 0;
  font-size: 12px;
}

.mem-data-item label {
  color: #666;
  min-width: 50px;
}

.mem-data-item span {
  color: #333;
  font-weight: 500;
}

.disk-list-container {
  padding: 10px;
  height: 290px;
  box-sizing: border-box;
}

.disk-list {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 2px;
}

.disk-list::-webkit-scrollbar {
  width: 6px;
}

.disk-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.disk-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.disk-item {
  margin-bottom: 8px;
  width: 100%;
}

.disk-name {
  font-size: 12px;
  font-weight: 500;
  margin-bottom: 4px;
  color: #333;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.io-list-container {
  padding: 10px;
  height: 290px;
  box-sizing: border-box;
}

.io-list {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 2px;
}

.io-list::-webkit-scrollbar {
  width: 6px;
}

.io-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.io-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.io-item {
  margin-bottom: 10px;
  width: 100%;
}

.io-name {
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 3px;
  color: #333;
}

.io-read,
.io-write {
  font-size: 11px;
  color: #666;
  line-height: 1.4;
}

.io-read .value,
.io-write .value {
  color: #409eff;
  font-weight: 500;
}

.net-list-container {
  padding: 10px;
  height: 290px;
  box-sizing: border-box;
}

.net-list {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 2px;
}

.net-list::-webkit-scrollbar {
  width: 6px;
}

.net-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.net-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.net-item {
  margin-bottom: 10px;
  width: 100%;
}

.net-name {
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 3px;
  color: #333;
}

.net-in,
.net-out {
  font-size: 11px;
  color: #666;
  line-height: 1.4;
}

.net-in .value {
  color: #67c23a;
  font-weight: 500;
}

.net-out .value {
  color: #f56c6c;
  font-weight: 500;
}

.process-list-container {
  padding: 10px;
  height: 290px;
  box-sizing: border-box;
}

.process-list {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 2px;
}

.process-list::-webkit-scrollbar {
  width: 6px;
}

.process-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.process-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.process-item {
  margin-bottom: 10px;
  width: 100%;
}

.process-name {
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 2px;
  color: #333;
}

.process-pid,
.process-mem {
  font-size: 11px;
  color: #666;
  line-height: 1.3;
}

.load-list-container {
  padding: 10px;
  height: 290px;
  box-sizing: border-box;
}

.load-list {
  height: 100%;
  overflow-y: auto;
  overflow-x: hidden;
  padding-right: 2px;
}

.load-list::-webkit-scrollbar {
  width: 6px;
}

.load-list::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.load-list::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.load-item {
  margin-bottom: 15px;
  width: 100%;
}

.load-label {
  font-size: 12px;
  font-weight: 600;
  margin-bottom: 2px;
  color: #333;
}

.load-value {
  font-size: 14px;
  font-weight: 500;
  color: #409eff;
  margin-bottom: 4px;
}

:deep(.el-progress) {
  width: 100% !important;
  margin-top: 4px;
}

:deep(.echarts-tooltip) {
  z-index: 9999 !important;
  pointer-events: auto !important;
}

.chart-wrapper {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
}

.ring-chart {
  width: 180px !important;
  height: 180px !important;
}

:deep(.cpu-tooltip-collapse),
:deep(.mem-tooltip-collapse) {
  margin-bottom: 8px;
  border: 1px solid #e5e5e5;
  border-radius: 4px;
}

:deep(.cpu-info-table),
:deep(.mem-info-table) {
  width: 100%;
  border-collapse: collapse;
  font-size: 12px;
}

:deep(.cpu-info-table tr),
:deep(.mem-info-table tr) {
  border-bottom: 1px solid #f0f0f0;
}

:deep(.cpu-info-table td),
:deep(.mem-info-table td) {
  padding: 4px 8px;
}

:deep(.cpu-info-table td:first-child),
:deep(.mem-info-table td:first-child) {
  color: #666;
  width: 100px;
}

:deep(.cpu-info-table td:last-child),
:deep(.mem-info-table td:last-child) {
  color: #333;
  font-weight: 500;
}

:deep(.cpu-core-table),
:deep(.cpu-process-table),
:deep(.mem-process-table) {
  width: 100%;
  border-collapse: collapse;
  font-size: 11px;
}

:deep(.cpu-core-table th),
:deep(.cpu-process-table th),
:deep(.mem-process-table th) {
  padding: 4px 4px;
  background: #f5f5f5;
  text-align: center;
  font-weight: 500;
}

:deep(.cpu-core-table td),
:deep(.cpu-process-table td),
:deep(.mem-process-table td) {
  padding: 4px 4px;
  text-align: center;
  border-bottom: 1px solid #f0f0f0;
}

:deep(.collapse-header) {
  padding: 8px 12px;
  background: #f8f8f8;
  cursor: pointer;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-weight: 500;
  font-size: 13px;
}

:deep(.collapse-header.active) {
  background: #e8f4ff;
}

:deep(.collapse-content) {
  padding: 0;
  max-height: 0;
  overflow: hidden;
  transition: max-height 0.3s ease, padding 0.3s ease;
}

:deep(.collapse-content.show) {
  padding: 8px 12px;
  max-height: 300px;
}

:deep(.op-link) {
  color: #409eff;
  text-decoration: none;
  cursor: pointer;
}

:deep(.op-link:hover) {
  color: #66b1ff;
  text-decoration: underline;
}

/* 响应式适配 */
@media (max-width: 768px) {

  .cpu-chart-col,
  .cpu-list-col {
    max-width: 100%;
    flex-shrink: 1;
  }

  .cpu-list-col {
    margin-top: 15px;
  }

  .ring-chart {
    width: 150px !important;
    height: 150px !important;
  }

  .disk-list-container,
  .io-list-container,
  .net-list-container,
  .process-list-container,
  .load-list-container {
    height: 200px;
  }

  .log-time {
    width: 120px;
  }

  .log-level {
    width: 70px;
  }

  .log-card .log-container {
    height: 250px;
  }

  :deep(.cpu-tooltip-collapse),
  :deep(.mem-tooltip-collapse) {
    width: 300px !important;
  }
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}
</style>
