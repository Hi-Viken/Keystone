<template>
  <div class="app-container home-page">
    <!-- 欢迎横幅 -->
    <div class="hero">
      <div class="hero-bg"></div>
      <div class="hero-content">
        <el-avatar :size="64" :src="userStore.avatar" class="hero-avatar">
          {{ userInitial }}
        </el-avatar>
        <div class="hero-text">
          <h2 class="hero-title">{{ greeting }}，{{ displayName }}</h2>
          <p class="hero-tip">{{ welcomeTip }}</p>
          <p class="hero-clock">
            <span>{{ $t('home.todayIs') }} {{ currentDate }}</span>
            <el-divider direction="vertical" />
            <span class="hero-time">{{ currentTime }}</span>
          </p>
        </div>
      </div>
    </div>

    <!-- 数据概览 -->
    <el-row :gutter="16" class="mt20">
      <el-col v-for="item in statList" :key="item.labelKey" :xs="12" :sm="12" :md="6">
        <div class="stat-card" :style="{ '--stat-color': item.color }" @click="go(item.path)">
          <div class="stat-icon">
            <svg-icon :name="item.icon" :size="'24px'" />
          </div>
          <div class="stat-body">
            <div class="stat-value">{{ item.value }}</div>
            <div class="stat-label">{{ $t(item.labelKey) }}</div>
          </div>
        </div>
      </el-col>
    </el-row>

    <el-row :gutter="16" class="mt20">
      <!-- 常用功能 -->
      <el-col :xs="24" :md="14">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span>{{ $t('home.quickAccess') }}</span>
              <span class="card-header-tip">{{ $t('home.quickAccessTip') }}</span>
            </div>
          </template>
          <div v-if="quickAccessList.length" class="quick-wrap">
            <div v-for="item in quickAccessList" :key="item.path" class="quick-item" @click="go(item.path)">
              <div class="quick-icon">
                <svg-icon :name="item.icon" :size="'20px'" />
              </div>
              <div class="quick-title">{{ item.title }}</div>
            </div>
          </div>
          <el-empty v-else :description="$t('home.emptyQuickAccess')" :image-size="80" />
        </el-card>
      </el-col>

      <!-- 系统信息 -->
      <el-col :xs="24" :md="10">
        <el-card shadow="never" class="box-card">
          <template #header>
            <div class="card-header">
              <span>{{ $t('home.systemInfo') }}</span>
            </div>
          </template>
          <el-descriptions :column="1" border size="small">
            <el-descriptions-item :label="$t('home.systemName')">{{ systemName }}</el-descriptions-item>
            <el-descriptions-item :label="$t('home.systemVersion')">{{ defaultSettings.version }}</el-descriptions-item>
            <el-descriptions-item :label="$t('home.userName')">{{ userStore.userName || '-' }}</el-descriptions-item>
            <el-descriptions-item :label="$t('home.userDept')">{{ userDept }}</el-descriptions-item>
            <el-descriptions-item :label="$t('home.userRole')">{{ userRole }}</el-descriptions-item>
          </el-descriptions>

          <div class="tech-stack">
            <div class="tech-label">{{ $t('home.techStack') }}</div>
            <div class="tech-tags">
              <el-tag v-for="tag in techStack" :key="tag" size="small" effect="plain">{{ tag }}</el-tag>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { useI18n } from 'vue-i18n'
import { useRouter } from 'vue-router'
import defaultSettings from '@/settings'
import useUserStore from '@/store/modules/user'
import usePermissionStore from '@/store/modules/permission'
import { isExternal } from '@/utils/validate'
import { getNormalPath } from '@/utils/ruoyi'

const { t, locale } = useI18n()
const router = useRouter()
const userStore = useUserStore()
const permissionStore = usePermissionStore()

// 首页与个人中心无需作为快捷入口展示
const EXCLUDE_PATHS = ['/index', '/user/profile']
const QUICK_ACCESS_LIMIT = 12

const pad = (value) => String(value).padStart(2, '0')

/** 拼接父子路由为完整路径，与侧边栏 resolvePath 保持一致 */
const resolvePath = (basePath, routePath) => {
  if (isExternal(routePath) || isExternal(basePath)) {
    return routePath
  }
  return getNormalPath(basePath + '/' + routePath)
}

/** 将路由树拍平为叶子节点，用于快捷入口 */
const flattenRoutes = (routes, basePath = '') => {
  const result = []
  routes.forEach((route) => {
    if (!route || route.hidden) return
    const fullPath = resolvePath(basePath, route.path)
    if (route.children && route.children.length) {
      result.push(...flattenRoutes(route.children, fullPath))
    } else if (fullPath) {
      result.push({ path: fullPath, meta: route.meta || {} })
    }
  })
  return result
}

const routeTitle = (meta) => (meta.titleKey ? t(meta.titleKey) : meta.title || '')

/** 所有可访问的叶子路由 */
const leafRoutes = computed(() => flattenRoutes(permissionStore.sidebarRouters))

/** 常用功能：取叶子路由中带标题的前若干项 */
const quickAccessList = computed(() =>
  leafRoutes.value
    .filter((item) => !EXCLUDE_PATHS.includes(item.path) && routeTitle(item.meta) && !isExternal(item.path))
    .slice(0, QUICK_ACCESS_LIMIT)
    .map((item) => ({
      path: item.path,
      title: routeTitle(item.meta),
      icon: item.meta.icon || 'menu'
    }))
)

/** 统计数据全部由本地 store / 静态配置推导，不请求任何接口 */
const statList = computed(() => [
  {
    labelKey: 'home.statMenu',
    value: leafRoutes.value.length,
    icon: 'menu',
    color: '#409eff',
    path: findPathByTitleKeys(['menu.systemMenu'])
  },
  {
    labelKey: 'home.statRole',
    value: userStore.roles.length,
    icon: 'role',
    color: '#67c23a',
    path: findPathByTitleKeys(['menu.systemRole'])
  },
  {
    labelKey: 'home.statPermission',
    value: userStore.permissions.length,
    icon: 'permission',
    color: '#e6a23c',
    path: findPathByTitleKeys(['menu.systemUser'])
  },
  {
    labelKey: 'home.statModule',
    value: permissionStore.defaultRoutes.length,
    icon: 'component',
    color: '#f56c6c',
    path: ''
  }
])

/** 按 titleKey 找到对应菜单路径，用于统计卡片跳转 */
function findPathByTitleKeys(titleKeys) {
  const target = leafRoutes.value.find((item) => titleKeys.includes(item.meta.titleKey) && !isExternal(item.path))
  return target ? target.path : ''
}

const techStack = ['Vue 3', 'Vite', 'Element Plus', 'Pinia', 'ECharts', 'ASP.NET Core', 'SqlSugar', 'SignalR']

const systemName = defaultSettings.title
const displayName = computed(() => userStore.name || userStore.userName || t('common.unknow'))
const userInitial = computed(() => displayName.value.charAt(0).toUpperCase())
// 欢迎语优先取后端 getInfo 已下发的 welcomeContent，取不到时回退到本地文案
const welcomeTip = computed(() => userStore.userInfo?.welcomeContent || t('home.welcomeTip'))
const userDept = computed(() => userStore.userInfo?.deptName || '-')
const userRole = computed(() => userStore.roles.join('、') || '-')

const greeting = computed(() => {
  const hour = new Date().getHours()
  if (hour < 6) return t('home.greetingEvening')
  if (hour < 12) return t('home.greetingMorning')
  if (hour < 18) return t('home.greetingAfternoon')
  return t('home.greetingEvening')
})

const currentDate = ref('')
const currentTime = ref('')
let clockTimer = null

/** 按当前语言格式化日期，locale 非法时回退到纯数字日期 */
const formatDate = (now) => {
  try {
    return new Intl.DateTimeFormat(locale.value, {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
      weekday: 'long'
    }).format(now)
  } catch {
    return `${now.getFullYear()}-${pad(now.getMonth() + 1)}-${pad(now.getDate())}`
  }
}

const updateClock = () => {
  const now = new Date()
  currentDate.value = formatDate(now)
  currentTime.value = `${pad(now.getHours())}:${pad(now.getMinutes())}:${pad(now.getSeconds())}`
}

/** 统一跳转：内部路由走 router，外部链接新窗口打开 */
const go = (path) => {
  if (!path) return
  if (isExternal(path)) {
    window.open(path, '_blank')
    return
  }
  if (path !== router.currentRoute.value.path) {
    router.push(path)
  }
}

onMounted(() => {
  updateClock()
  clockTimer = setInterval(updateClock, 1000)
})

onUnmounted(() => {
  if (clockTimer) clearInterval(clockTimer)
})
</script>

<style lang="scss" scoped>
.home-page {
  --home-border: var(--el-border-color-lighter);
  --home-text: var(--base-text-color-rgba);
}

/* 欢迎横幅 */
.hero {
  position: relative;
  padding: 24px;
  border-radius: 8px;
  overflow: hidden;
  background: var(--el-color-primary);
  color: #fff;
}

.hero-bg {
  position: absolute;
  inset: 0;
  background:
    radial-gradient(circle at 85% 20%, rgba(255, 255, 255, 0.25), transparent 45%),
    radial-gradient(circle at 70% 90%, rgba(255, 255, 255, 0.15), transparent 40%);
  pointer-events: none;
}

.hero-content {
  position: relative;
  display: flex;
  align-items: center;
  gap: 18px;
}

.hero-avatar {
  flex-shrink: 0;
  border: 2px solid rgba(255, 255, 255, 0.6);
  background: rgba(255, 255, 255, 0.2);
  color: #fff;
  font-size: 24px;
}

.hero-text {
  min-width: 0;
}

.hero-title {
  margin: 0 0 6px;
  font-size: 22px;
  font-weight: 600;
}

.hero-tip {
  margin: 0 0 8px;
  font-size: 13px;
  opacity: 0.9;
}

.hero-clock {
  margin: 0;
  font-size: 13px;
  opacity: 0.85;
}

.hero-time {
  font-variant-numeric: tabular-nums;
  font-weight: 600;
}

/* 统计卡片 */
.stat-card {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 16px;
  padding: 18px;
  border: 1px solid var(--home-border);
  border-radius: 8px;
  background: var(--el-bg-color);
  cursor: pointer;
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

.stat-value {
  font-size: 22px;
  font-weight: 600;
  line-height: 1.2;
}

.stat-label {
  margin-top: 2px;
  font-size: 13px;
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
  font-weight: 600;
}

.card-header-tip {
  font-size: 12px;
  font-weight: 400;
  color: var(--el-text-color-secondary);
}

/* 常用功能 */
.quick-wrap {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(96px, 1fr));
  gap: 12px;
}

.quick-item {
  padding: 14px 8px;
  border: 1px solid var(--home-border);
  border-radius: 8px;
  text-align: center;
  cursor: pointer;
  transition:
    border-color 0.2s,
    background-color 0.2s;

  &:hover {
    border-color: var(--el-color-primary);
    background: color-mix(in srgb, var(--el-color-primary) 8%, transparent);
  }
}

.quick-icon {
  display: flex;
  align-items: center;
  justify-content: center;
  margin-bottom: 8px;
  color: var(--el-color-primary);
}

.quick-title {
  font-size: 13px;
  line-height: 1.3;
  word-break: break-all;
}

/* 技术栈 */
.tech-stack {
  margin-top: 16px;
}

.tech-label {
  margin-bottom: 8px;
  font-size: 13px;
  font-weight: 600;
  color: var(--home-text);
}

.tech-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

@media (max-width: 768px) {
  .hero-content {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .hero-title {
    font-size: 18px;
  }
}
</style>
