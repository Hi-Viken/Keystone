<template>
  <logo v-show="showLogo" :collapse="false"></logo>
  <el-menu class="topbar-menu" :ellipsis="false" :default-active="activeMenu" :active-text-color="theme"
    mode="horizontal">
    <sidebar-item :key="route.path + index" v-for="(route, index) in showRoutes" :item="route"
      :base-path="route.path" />

    <!-- 顶部菜单超出数量折叠 -->
    <el-sub-menu index="more" class="el-sub-menu__hide-arrow" v-if="moreRoutes.length > 0">
      <template #title>
        <svg-icon name="ele-more"></svg-icon>
      </template>
      <sidebar-item :key="route.path + index" v-for="(route, index) in moreRoutes" :item="route"
        :base-path="route.path" />
    </el-sub-menu>
  </el-menu>
</template>

<script setup>
import Logo from '../Sidebar/Logo'
import SidebarItem from '../Sidebar/SidebarItem'
import useAppStore from '@/store/modules/app'
import useSettingsStore from '@/store/modules/settings'
import usePermissionStore from '@/store/modules/permission'
import { ref, computed, onMounted, onBeforeUnmount, watch } from 'vue'

const route = useRoute()
const appStore = useAppStore()
const settingsStore = useSettingsStore()
const permissionStore = usePermissionStore()

const sidebarRouters = computed(() => permissionStore.sidebarRouters)
const showLogo = computed(() => settingsStore.sidebarLogo)
const theme = computed(() => settingsStore.theme)
const device = computed(() => appStore.device)
const activeMenu = computed(() => {
  const { meta, path } = route
  // if set path, the sidebar will highlight the path you set
  if (meta.activeMenu) {
    return meta.activeMenu
  }
  return path
})

const visibleNumber = ref(5)
const showRoutes = computed(() => {
  return permissionStore.sidebarRouters.filter((f) => !f.hidden).slice(0, visibleNumber.value)
})
const moreRoutes = computed(() => {
  return permissionStore.sidebarRouters.filter((f) => !f.hidden).slice(visibleNumber.value)
})

// 【核心优化：彻底消除强制重排】
function setVisibleNumber() {
  requestAnimationFrame(() => {
    // 替换为不会触发重排的 API
    const width = window.innerWidth / 3
    visibleNumber.value = parseInt(width / 85)
  })
}

onMounted(() => {
  window.addEventListener('resize', setVisibleNumber)
  setVisibleNumber()
})
onBeforeUnmount(() => {
  window.removeEventListener('resize', setVisibleNumber)
})

watch(route, (val) => {
  if (device.value === 'mobile') {
    appStore.closeSideBar()
  }
})
</script>
<style lang="scss" scoped>
.el-menu--horizontal.el-menu {
  border-bottom: unset !important;
}

.el-menu--horizontal {
  height: var(--base-header-height) !important;
}

.topbar-menu {
  width: 100% !important;
  flex: 1;
  background-color: transparent;
  border: none;
}
</style>