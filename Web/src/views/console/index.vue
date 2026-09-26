<template>
  <div class="app-container console-page">
    <!-- 顶部欢迎条 -->
    <div class="console-hero">
      <div class="hero-left">
        <el-avatar :size="52" :src="userStore.avatar" class="hero-avatar">{{ userInitial }}</el-avatar>
        <div class="hero-text">
          <div class="hero-title">{{ greeting }}，{{ displayName }}</div>
          <div class="hero-desc">欢迎回到 {{ systemName }} 控制台，当前 {{ onlineStat }} 人在线</div>
        </div>
      </div>
      <div class="hero-right">
        <div class="hero-clock">
          <span class="hero-date">{{ currentDate }}</span>
          <span class="hero-time">{{ currentTime }}</span>
        </div>
        <el-tag :type="runEnvType" effect="dark" round>{{ runEnv }}</el-tag>
        <el-tooltip content="刷新数据" placement="bottom">
          <el-button :icon="Refresh" circle :loading="refreshing" @click="handleRefresh" />
        </el-tooltip>
      </div>
    </div>

    <!-- 核心指标 -->
    <el-row :gutter="16">
      <el-col v-for="item in statList" :key="item.label" :xs="12" :sm="8" :lg="4">
        <div class="stat-card" :style="{ '--stat-color': item.color }">
          <div class="stat-icon">
            <el-icon :size="22"><component :is="item.icon" /></el-icon>
          </div>
          <div class="stat-body">
            <div class="stat-value">{{ item.value }}<span class="stat-unit">{{ item.unit }}</span></div>
            <div class="stat-label">{{ item.label }}</div>
            <div class="stat-trend" :class="item.up ? 'is-up' : 'is-down'">
              <el-icon :size="12">
                <component :is="item.up ? 'CaretTop' : 'CaretBottom'" />
              </el-icon>
              <span>{{ Math.abs(item.rate) }}%</span>
              <span class="stat-tips">较昨日</span>
            </div>
          </div>
        </div>
      </el-col>
    </el-row>

    <!-- 图表区 -->
    <el-row :gutter="16">
      <el-col :xs="24" :lg="16">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">访问趋势</span>
              <el-radio-group v-model="trendRange" size="small">
                <el-radio-button value="7">近 7 天</el-radio-button>
                <el-radio-button value="30">近 30 天</el-radio-button>
              </el-radio-group>
            </div>
          </template>
          <div ref="trendRef" class="chart-box" />
        </el-card>
      </el-col>
      <el-col :xs="24" :lg="8">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">模块访问占比</span>
              <span class="card-tip">近 7 天</span>
            </div>
          </template>
          <div ref="moduleRef" class="chart-box" />
        </el-card>
      </el-col>
    </el-row>

    <!-- 在线用户 + 服务器资源 -->
    <el-row :gutter="16">
      <el-col :xs="24" :lg="16">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">
                在线用户
                <el-tag size="small" type="success" effect="plain" round>{{ onlineUsers.length }}</el-tag>
              </span>
              <el-button link type="primary" :icon="View" @click="go('/monitor/onlineuser')">查看全部</el-button>
            </div>
          </template>
          <el-table :data="onlineUsers" border stripe size="small">
            <el-table-column type="index" label="#" width="55" align="center" />
            <el-table-column prop="userName" label="用户名" min-width="90" align="center" />
            <el-table-column prop="deptName" label="部门" min-width="90" align="center" />
            <el-table-column prop="ipaddr" label="登录 IP" min-width="120" align="center" />
            <el-table-column prop="browser" label="浏览器" min-width="100" align="center" />
            <el-table-column prop="os" label="操作系统" min-width="110" align="center" />
            <el-table-column prop="loginTime" label="登录时间" min-width="150" align="center" />
            <el-table-column label="在线时长" min-width="90" align="center">
              <template #default="scope">
                <el-tag size="small" effect="plain">{{ scope.row.onlineTime }} 分钟</el-tag>
              </template>
            </el-table-column>
            <el-table-column label="操作" width="120" align="center">
              <template #default="scope">
                <el-button link type="primary" :icon="ChatDotRound" @click="mockAction(`私信 ${scope.row.userName}`)">私信</el-button>
                <el-button link type="danger" :icon="SwitchButton" @click="mockAction(`强退 ${scope.row.userName}`)">强退</el-button>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>

      <el-col :xs="24" :lg="8">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">服务器资源</span>
              <el-tag size="small" type="success" effect="plain" round>运行正常</el-tag>
            </div>
          </template>
          <div class="resource-list">
            <div v-for="item in resourceList" :key="item.label" class="resource-item">
              <div class="resource-head">
                <span class="resource-label">
                  <el-icon :size="14"><component :is="item.icon" /></el-icon>
                  {{ item.label }}
                </span>
                <span class="resource-value">{{ item.text }}</span>
              </div>
              <el-progress
                :percentage="item.percentage"
                :stroke-width="10"
                :show-text="false"
                :color="item.color"
              />
            </div>
          </div>
          <el-descriptions :column="2" border size="small" class="resource-desc">
            <el-descriptions-item label="运行天数">128 天</el-descriptions-item>
            <el-descriptions-item label="平均负载">32%</el-descriptions-item>
            <el-descriptions-item label="数据库">MySQL 8.0</el-descriptions-item>
            <el-descriptions-item label="缓存">Redis 7.2</el-descriptions-item>
          </el-descriptions>
        </el-card>
      </el-col>
    </el-row>

    <!-- 操作日志 + 实时动态 -->
    <el-row :gutter="16">
      <el-col :xs="24" :lg="16">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">最近操作日志</span>
              <el-button link type="primary" :icon="View" @click="go('/monitor/operlog')">查看全部</el-button>
            </div>
          </template>
          <el-table :data="operLogs" border stripe size="small">
            <el-table-column prop="time" label="操作时间" min-width="150" align="center" />
            <el-table-column prop="userName" label="操作人" min-width="90" align="center" />
            <el-table-column prop="module" label="所属模块" min-width="90" align="center" />
            <el-table-column prop="operType" label="操作类型" min-width="90" align="center">
              <template #default="scope">
                <el-tag size="small" :type="operTagType[scope.row.operType] || 'info'" effect="light">
                  {{ scope.row.operType }}
                </el-tag>
              </template>
            </el-table-column>
            <el-table-column prop="ipaddr" label="操作 IP" min-width="120" align="center" />
            <el-table-column label="耗时" min-width="80" align="center">
              <template #default="scope">{{ scope.row.cost }} ms</template>
            </el-table-column>
            <el-table-column label="状态" min-width="80" align="center">
              <template #default="scope">
                <el-tag size="small" :type="scope.row.success ? 'success' : 'danger'" effect="plain">
                  {{ scope.row.success ? '成功' : '失败' }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </el-card>
      </el-col>

      <el-col :xs="24" :lg="8">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">实时动态</span>
              <span class="card-tip">最近 5 条</span>
            </div>
          </template>
          <el-timeline class="activity-list">
            <el-timeline-item
              v-for="item in activityList"
              :key="item.time + item.text"
              :timestamp="item.time"
              placement="top"
              :type="item.type"
              :hollow="item.hollow"
            >
              <div class="activity-text">{{ item.text }}</div>
              <div class="activity-user">{{ item.userName }}</div>
            </el-timeline-item>
          </el-timeline>
        </el-card>
      </el-col>
    </el-row>

    <!-- 快捷入口 + 系统信息 -->
    <el-row :gutter="16">
      <el-col :xs="24" :lg="14">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">快捷入口</span>
              <span class="card-tip">点击卡片进入对应功能</span>
            </div>
          </template>
          <div class="shortcut-wrap">
            <div
              v-for="item in shortcutList"
              :key="item.path"
              class="shortcut-item"
              :style="{ '--shortcut-color': item.color }"
              @click="go(item.path)"
            >
              <div class="shortcut-icon">
                <el-icon :size="20"><component :is="item.icon" /></el-icon>
              </div>
              <div class="shortcut-title">{{ item.title }}</div>
            </div>
          </div>
        </el-card>
      </el-col>

      <el-col :xs="24" :lg="10">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span class="card-title">系统信息</span>
              <span class="card-tip">{{ runEnv }}</span>
            </div>
          </template>
          <el-descriptions :column="1" border size="small">
            <el-descriptions-item v-for="item in systemInfo" :key="item.label" :label="item.label">
              {{ item.value }}
            </el-descriptions-item>
          </el-descriptions>
          <div class="tech-tags">
            <el-tag v-for="tag in techStack" :key="tag" size="small" effect="plain">{{ tag }}</el-tag>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup name="console">
import * as echarts from 'echarts'
import { useResizeObserver } from '@vueuse/core'
import {
  Bell,
  ChatDotRound,
  Cpu,
  DataLine,
  Document as DocumentIcon,
  Histogram,
  Monitor,
  Odometer,
  PieChart,
  Position,
  Refresh,
  Setting,
  SwitchButton,
  Timer,
  TrendCharts,
  User,
  View,
  Warning
} from '@element-plus/icons-vue'
import defaultSettings from '@/settings'
import useUserStore from '@/store/modules/user'

const { proxy } = getCurrentInstance()
const router = useRouter()
const userStore = useUserStore()

const pad = (value) => String(value).padStart(2, '0')
const systemName = defaultSettings.title
const runEnv = import.meta.env.MODE === 'production' ? '生产环境' : '开发环境'
const runEnvType = import.meta.env.MODE === 'production' ? 'danger' : 'warning'
const techStack = ['Vue 3', 'Vite 6', 'Element Plus', 'Pinia', 'ECharts 5', 'ASP.NET Core', 'SqlSugar', 'SignalR']

const displayName = computed(() => userStore.name || userStore.userName || '管理员')
const userInitial = computed(() => displayName.value.charAt(0).toUpperCase())
const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6) return '凌晨好'
  if (hour < 12) return '上午好'
  if (hour < 14) return '中午好'
  if (hour < 18) return '下午好'
  return '晚上好'
})

const currentDate = ref('')
const currentTime = ref('')
let clockTimer = null
const updateClock = () => {
  const now = new Date()
  currentDate.value = `${now.getFullYear()}年${pad(now.getMonth() + 1)}月${pad(now.getDate())}日`
  currentTime.value = `${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`
}

const statList = ref([
  { label: '用户总数', value: 1286, unit: ' 人', icon: markRaw(User), color: '#409eff', rate: 12.5, up: true },
  { label: '今日新增', value: 46, unit: ' 人', icon: markRaw(TrendCharts), color: '#67c23a', rate: 8.2, up: true },
  { label: '在线用户', value: 128, unit: ' 人', icon: markRaw(Monitor), color: '#36cfc9', rate: 3.4, up: true },
  { label: '接口调用', value: 52840, unit: ' 次', icon: markRaw(DataLine), color: '#e6a23c', rate: 18.9, up: true },
  { label: '异常请求', value: 37, unit: ' 次', icon: markRaw(Warning), color: '#f56c6c', rate: 6.4, up: false },
  { label: '平均耗时', value: 128, unit: ' ms', icon: markRaw(Odometer), color: '#909399', rate: 4.1, up: false }
])
const onlineStat = computed(() => statList.value.find((item) => item.label === '在线用户')?.value || 0)

const trendRange = ref('7')
const trendRef = ref(null)
const moduleRef = ref(null)
let trendChart = null
let moduleChart = null
let themeObserver = null

const cssVar = (name, fallback) => {
  const value = getComputedStyle(document.documentElement).getPropertyValue(name).trim()
  return value || fallback
}
const palette = () => [cssVar('--el-color-primary', '#409eff'), '#67c23a', '#e6a23c', '#f56c6c', '#36cfc9', '#909399']

const WAVE = [0.82, 0.95, 1.08, 0.9, 1.15, 1.02, 0.88, 1.12, 0.96, 1.05, 0.86, 1.18, 1, 0.93]
const buildTrend = (days, base) => {
  const today = new Date()
  const labels = []
  const visit = []
  const visitor = []
  const api = []
  for (let i = days - 1; i >= 0; i--) {
    const day = new Date(today.getFullYear(), today.getMonth(), today.getDate() - i)
    labels.push(`${pad(day.getMonth() + 1)}-${pad(day.getDate())}`)
    const wave = WAVE[(days - 1 - i) % WAVE.length]
    visit.push(Math.round(base * wave))
    visitor.push(Math.round(base * wave * 0.42))
    api.push(Math.round(base * wave * 6.4))
  }
  return { labels, visit, visitor, api }
}
const trendData = computed(() => buildTrend(Number(trendRange.value), trendRange.value === '7' ? 3200 : 2900))

const moduleData = [
  { name: '系统管理', value: 4210 },
  { name: '在线监控', value: 2860 },
  { name: '代码生成', value: 1740 },
  { name: '文件管理', value: 1320 },
  { name: '邮件服务', value: 860 },
  { name: '其他', value: 520 }
]

const buildTrendOption = () => {
  const { labels, visit, visitor, api } = trendData.value
  const colors = palette()
  const textColor = cssVar('--el-text-color-regular', '#606266')
  const subTextColor = cssVar('--el-text-color-secondary', '#909399')
  const splitColor = cssVar('--el-border-color-lighter', '#ebeef5')
  return {
    tooltip: { trigger: 'axis', axisPointer: { type: 'shadow' } },
    legend: {
      top: 0,
      right: 0,
      itemWidth: 10,
      itemHeight: 10,
      textStyle: { color: textColor, fontSize: 12 },
      data: ['访问量', '独立访客', '接口调用']
    },
    grid: { top: 40, left: 6, right: 6, bottom: 4, containLabel: true },
    xAxis: {
      type: 'category',
      data: labels,
      axisTick: { show: false },
      axisLine: { lineStyle: { color: splitColor } },
      axisLabel: { color: subTextColor, fontSize: 12, interval: labels.length > 10 ? 4 : 0 }
    },
    yAxis: [
      {
        type: 'value',
        name: '人次',
        nameTextStyle: { color: subTextColor, fontSize: 12 },
        axisLabel: { color: subTextColor, fontSize: 12 },
        splitLine: { lineStyle: { color: splitColor, type: 'dashed' } }
      },
      {
        type: 'value',
        name: '接口',
        nameTextStyle: { color: subTextColor, fontSize: 12 },
        axisLabel: { color: subTextColor, fontSize: 12 },
        splitLine: { show: false }
      }
    ],
    series: [
      {
        name: '访问量',
        type: 'bar',
        data: visit,
        barMaxWidth: 26,
        itemStyle: { color: colors[0], borderRadius: [4, 4, 0, 0] }
      },
      {
        name: '独立访客',
        type: 'line',
        smooth: true,
        showSymbol: false,
        data: visitor,
        lineStyle: { width: 2, color: colors[1] },
        itemStyle: { color: colors[1] }
      },
      {
        name: '接口调用',
        type: 'line',
        smooth: true,
        showSymbol: false,
        yAxisIndex: 1,
        data: api,
        lineStyle: { width: 2, color: colors[2], type: 'dashed' },
        itemStyle: { color: colors[2] }
      }
    ]
  }
}

const buildModuleOption = () => {
  const textColor = cssVar('--el-text-color-regular', '#606266')
  const subTextColor = cssVar('--el-text-color-secondary', '#909399')
  const bgColor = cssVar('--el-bg-color', '#ffffff')
  return {
    color: palette(),
    tooltip: { trigger: 'item', formatter: '{b}: {c} 次 ({d}%)' },
    legend: {
      bottom: 0,
      itemWidth: 10,
      itemHeight: 10,
      itemGap: 12,
      textStyle: { color: textColor, fontSize: 12 }
    },
    series: [
      {
        name: '模块访问占比',
        type: 'pie',
        radius: ['42%', '66%'],
        center: ['50%', '44%'],
        avoidLabelOverlap: true,
        itemStyle: { borderRadius: 4, borderColor: bgColor, borderWidth: 2 },
        label: { show: false },
        emphasis: {
          scaleSize: 6,
          label: { show: true, color: textColor, fontSize: 13, fontWeight: 600, formatter: '{b}\n{d}%' }
        },
        data: moduleData
      }
    ],
    graphic: {
      type: 'text',
      left: 'center',
      top: '38%',
      style: {
        text: `${moduleData.reduce((total, item) => total + item.value, 0)}\n访问总数`,
        textAlign: 'center',
        fill: subTextColor,
        fontSize: 12,
        lineHeight: 18
      }
    }
  }
}

const renderCharts = () => {
  if (!trendRef.value || !moduleRef.value) return
  trendChart = trendChart || echarts.init(trendRef.value)
  moduleChart = moduleChart || echarts.init(moduleRef.value)
  trendChart.setOption(buildTrendOption())
  moduleChart.setOption(buildModuleOption())
}

const onlineUsers = [
  { userName: 'admin', deptName: '研发部', ipaddr: '192.168.1.108', browser: 'Chrome 131', os: 'Windows 11', loginTime: '2026-09-26 09:12:31', onlineTime: 186 },
  { userName: 'zhangsan', deptName: '市场部', ipaddr: '192.168.1.132', browser: 'Edge 130', os: 'Windows 11', loginTime: '2026-09-26 09:35:08', onlineTime: 143 },
  { userName: 'lisi', deptName: '运营部', ipaddr: '192.168.1.155', browser: 'Firefox 133', os: 'macOS 15', loginTime: '2026-09-26 10:02:47', onlineTime: 96 },
  { userName: 'wangwu', deptName: '研发部', ipaddr: '192.168.1.167', browser: 'Chrome 131', os: 'Windows 10', loginTime: '2026-09-26 10:21:15', onlineTime: 78 },
  { userName: 'zhaoliu', deptName: '财务部', ipaddr: '192.168.1.183', browser: 'Safari 18', os: 'macOS 15', loginTime: '2026-09-26 11:08:52', onlineTime: 52 },
  { userName: 'sunqi', deptName: '运营部', ipaddr: '192.168.1.196', browser: 'Chrome 130', os: 'Windows 10', loginTime: '2026-09-26 13:26:03', onlineTime: 34 },
  { userName: 'zhouba', deptName: '研发部', ipaddr: '192.168.1.201', browser: 'Edge 130', os: 'Windows 11', loginTime: '2026-09-26 14:47:29', onlineTime: 21 },
  { userName: 'wujiu', deptName: '市场部', ipaddr: '192.168.1.215', browser: 'Chrome 131', os: 'Ubuntu 24', loginTime: '2026-09-26 15:31:10', onlineTime: 8 }
]

const operTagType = { 新增: 'success', 修改: 'warning', 删除: 'danger', 查询: 'info', 导入: '', 导出: '' }
const operLogs = [
  { time: '2026-09-26 15:42:08', userName: 'admin', module: '用户管理', operType: '新增', ipaddr: '192.168.1.108', cost: 86, success: true },
  { time: '2026-09-26 15:38:51', userName: 'zhangsan', module: '角色管理', operType: '修改', ipaddr: '192.168.1.132', cost: 42, success: true },
  { time: '2026-09-26 15:21:36', userName: 'lisi', module: '菜单管理', operType: '查询', ipaddr: '192.168.1.155', cost: 18, success: true },
  { time: '2026-09-26 15:09:12', userName: 'admin', module: '部门管理', operType: '删除', ipaddr: '192.168.1.108', cost: 73, success: true },
  { time: '2026-09-26 14:55:47', userName: 'wangwu', module: '代码生成', operType: '导入', ipaddr: '192.168.1.167', cost: 1264, success: true },
  { time: '2026-09-26 14:32:19', userName: 'zhaoliu', module: '文件管理', operType: '导出', ipaddr: '192.168.1.183', cost: 986, success: true },
  { time: '2026-09-26 14:10:03', userName: 'sunqi', module: '岗位管理', operType: '新增', ipaddr: '192.168.1.196', cost: 51, success: true },
  { time: '2026-09-26 13:58:44', userName: 'zhouba', module: '字典管理', operType: '修改', ipaddr: '192.168.1.201', cost: 37, success: false }
]

const activityList = [
  { time: '15:42:08', text: '新增用户「运营小助手」', userName: 'admin', type: 'primary', hollow: false },
  { time: '15:38:51', text: '调整了「市场部」的数据权限', userName: 'zhangsan', type: 'warning', hollow: true },
  { time: '15:21:36', text: '导出了一份用户清单', userName: 'lisi', type: 'success', hollow: true },
  { time: '15:09:12', text: '删除了已停用的部门节点', userName: 'admin', type: 'danger', hollow: true },
  { time: '14:55:47', text: '生成了订单表实体代码', userName: 'wangwu', type: 'info', hollow: true }
]

const resourceList = [
  { label: 'CPU 使用率', text: '38%', percentage: 38, color: '#67c23a', icon: Cpu },
  { label: '内存使用率', text: '62%', percentage: 62, color: '#409eff', icon: Monitor },
  { label: '磁盘占用', text: '74%', percentage: 74, color: '#e6a23c', icon: Histogram },
  { label: '带宽占用', text: '45%', percentage: 45, color: '#36cfc9', icon: PieChart }
]

const shortcutList = [
  { title: '用户管理', path: '/system/user', icon: User, color: '#409eff' },
  { title: '角色管理', path: '/system/role', icon: Setting, color: '#67c23a' },
  { title: '菜单管理', path: '/system/menu', icon: Position, color: '#e6a23c' },
  { title: '在线用户', path: '/monitor/onlineuser', icon: Monitor, color: '#f56c6c' },
  { title: '操作日志', path: '/monitor/operlog', icon: DocumentIcon, color: '#36cfc9' },
  { title: '定时任务', path: '/monitor/job', icon: Timer, color: '#909399' },
  { title: '代码生成', path: '/tool/gen', icon: DataLine, color: '#409eff' },
  { title: '通知公告', path: '/system/notice', icon: Bell, color: '#e6a23c' }
]

const systemInfo = computed(() => [
  { label: '系统名称', value: systemName },
  { label: '框架版本', value: defaultSettings.version },
  { label: '运行环境', value: runEnv },
  { label: '访问地址', value: window.location.origin },
  { label: '当前账号', value: userStore.userName || '-' },
  { label: '拥有角色', value: userStore.roles.join('、') || '-' },
  { label: '权限数量', value: `${userStore.permissions.length} 项` }
])

const refreshing = ref(false)
const handleRefresh = () => {
  refreshing.value = true
  statList.value = statList.value.map((item) => ({
    ...item,
    value: Math.max(0, item.value + Math.round((Math.random() - 0.4) * 20))
  }))
  renderCharts()
  setTimeout(() => {
    refreshing.value = false
  }, 500)
  proxy.$modal.msgSuccess('控制台数据已刷新')
}

const mockAction = (text) => {
  proxy.$modal.msg(`${text}：演示页面暂未接入接口`)
}

const go = (path) => {
  if (!router.resolve(path).matched.some((item) => item.meta?.title)) {
    proxy.$modal.msgWarning(`菜单 ${path} 尚未配置，请先在菜单管理中维护`)
    return
  }
  if (path !== router.currentRoute.value.path) {
    router.push(path)
  }
}

watch(trendRange, () => renderCharts())

useResizeObserver(trendRef, () => trendChart && trendChart.resize())
useResizeObserver(moduleRef, () => moduleChart && moduleChart.resize())

onMounted(() => {
  updateClock()
  clockTimer = setInterval(updateClock, 1000)
  nextTick(renderCharts)
  themeObserver = new MutationObserver(renderCharts)
  themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ['class', 'style'] })
})

onBeforeUnmount(() => {
  if (clockTimer) clearInterval(clockTimer)
  themeObserver && themeObserver.disconnect()
  trendChart && trendChart.dispose()
  moduleChart && moduleChart.dispose()
})
</script>

<style lang="scss" scoped>
.console-page {
  --console-border: var(--el-border-color-lighter);
}

/* 栅格列间距 */
.el-col {
  margin-bottom: 16px;
}

/* 顶部欢迎条 */
.console-hero {
  display: flex;
  align-items: center;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
  margin-bottom: 16px;
  padding: 20px 24px;
  border-radius: 8px;
  color: #fff;
  background: linear-gradient(120deg, var(--el-color-primary) 0%, color-mix(in srgb, var(--el-color-primary) 55%, #000) 100%);
}

.hero-left,
.hero-right {
  display: flex;
  align-items: center;
  gap: 14px;
}

.hero-avatar {
  flex-shrink: 0;
  border: 2px solid rgb(255 255 255 / 60%);
  background: rgb(255 255 255 / 20%);
  color: #fff;
  font-size: 22px;
}

.hero-title {
  font-size: 20px;
  font-weight: 600;
  line-height: 1.4;
}

.hero-desc {
  margin-top: 4px;
  font-size: 13px;
  opacity: 0.9;
}

.hero-clock {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  font-size: 12px;
  line-height: 1.5;
  opacity: 0.92;
}

.hero-time {
  font-size: 16px;
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

/* 指标卡片 */
.stat-card {
  display: flex;
  align-items: center;
  gap: 14px;
  height: 100%;
  padding: 18px;
  border: 1px solid var(--console-border);
  border-radius: 8px;
  background: var(--el-bg-color);
  transition:
    box-shadow 0.2s,
    transform 0.2s;

  &:hover {
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgb(0 0 0 / 8%);
  }
}

.stat-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 46px;
  height: 46px;
  flex-shrink: 0;
  border-radius: 10px;
  color: var(--stat-color);
  background: color-mix(in srgb, var(--stat-color) 12%, transparent);
}

.stat-body {
  min-width: 0;
}

.stat-value {
  font-size: 22px;
  font-weight: 600;
  line-height: 1.2;
  font-variant-numeric: tabular-nums;
}

.stat-unit {
  font-size: 12px;
  font-weight: 400;
  color: var(--el-text-color-secondary);
}

.stat-label {
  margin-top: 2px;
  font-size: 13px;
  color: var(--el-text-color-secondary);
}

.stat-trend {
  display: flex;
  align-items: center;
  gap: 2px;
  margin-top: 6px;
  font-size: 12px;

  &.is-up {
    color: var(--el-color-danger);
  }

  &.is-down {
    color: var(--el-color-success);
  }
}

.stat-tips {
  margin-left: 4px;
  color: var(--el-text-color-secondary);
}

/* 卡片 */
.box-card {
  border-radius: 8px;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
}

.card-title {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 600;
}

.card-tip {
  font-size: 12px;
  font-weight: 400;
  color: var(--el-text-color-secondary);
}

.chart-box {
  width: 100%;
  height: 320px;
}

/* 服务器资源 */
.resource-list {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.resource-head {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 6px;
  font-size: 13px;
}

.resource-label {
  display: flex;
  align-items: center;
  gap: 4px;
  color: var(--el-text-color-regular);
}

.resource-value {
  font-weight: 600;
  font-variant-numeric: tabular-nums;
}

.resource-desc {
  margin-top: 16px;
}

/* 实时动态 */
.activity-list {
  padding-left: 4px;
}

.activity-text {
  font-size: 13px;
  line-height: 1.5;
}

.activity-user {
  margin-top: 2px;
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

/* 快捷入口 */
.shortcut-wrap {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(110px, 1fr));
  gap: 12px;
}

.shortcut-item {
  padding: 16px 8px;
  border: 1px solid var(--console-border);
  border-radius: 8px;
  text-align: center;
  cursor: pointer;
  transition:
    border-color 0.2s,
    background-color 0.2s;

  &:hover {
    border-color: var(--shortcut-color);
    background: color-mix(in srgb, var(--shortcut-color) 8%, transparent);
  }
}

.shortcut-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
  color: var(--shortcut-color);
}

.shortcut-title {
  font-size: 13px;
}

/* 技术栈 */
.tech-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  margin-top: 16px;
}

@media (max-width: 768px) {
  .hero-right {
    width: 100%;
    justify-content: space-between;
  }

  .stat-value {
    font-size: 18px;
  }
}
</style>
