<template>
  <el-drawer v-model="showSettings" :title="$t('layout.systemSettings')" :with-header="false" direction="rtl" size="330px">
    <!-- <div class="setting-drawer-title">
      <h3 class="drawer-title">导航模式</h3>
    </div> -->
    <el-divider>{{ $t('layout.themePresets') }}</el-divider>
    <div class="preset-wrap">
      <div
        v-for="preset in presetThemes"
        :key="preset.name"
        class="preset-card"
        :class="{ activePreset: currentPreset === preset.name }"
        @click="applyPreset(preset)">
        <div class="preset-preview" :style="{ background: preset.previewBg }">
          <div class="preset-sidebar" :style="{ background: preset.sidebarColor }"></div>
          <div class="preset-content">
            <div class="preset-header" :style="{ background: preset.primaryColor }"></div>
          </div>
        </div>
        <span class="preset-label">{{ preset.label }}</span>
      </div>
    </div>
    <el-divider>{{ $t('layout.navMode') }}</el-divider>
    <div class="nav-wrap">
      <el-tooltip :content="$t('layout.leftMenu')" placement="bottom">
        <div class="item left" @click="handleNavType(1)" :class="{ activeItem: navType == 1 }">
          <b></b>
          <b></b>
        </div>
      </el-tooltip>

      <el-tooltip :content="$t('layout.mixMenu')" placement="bottom">
        <div class="item mix" @click="handleNavType(2)" :class="{ activeItem: navType == 2 }">
          <b></b>
          <b></b>
        </div>
      </el-tooltip>
      <el-tooltip :content="$t('layout.topMenu')" placement="bottom">
        <div class="item top" @click="handleNavType(3)" :class="{ activeItem: navType == 3 }">
          <b></b>
          <b></b>
        </div>
      </el-tooltip>
    </div>
    <div class="drawer-item" style="text-align: center">
      <!-- <el-radio-group v-model="mode" size="small">
        <el-radio value="dark">{{ $t('layout.darkMode') }}</el-radio>
        <el-radio value="light">{{ $t('layout.lightMode') }}</el-radio>
      </el-radio-group> -->
      <el-divider> {{ $t('layout.themeStyleSet') }} </el-divider>
      <el-switch v-model="mode" inactive-icon="Sunny" active-icon="Moon" active-value="dark" inactive-value="light"></el-switch>
    </div>
    <div class="drawer-item">
      <!-- <div>侧边栏颜色</div> -->
      <el-divider> {{ $t('layout.sideColor') }} </el-divider>

      <div class="mt10">
        <span
          class="color-item"
          :class="{ sideActive: item.name == sideTheme }"
          :style="{ 'background-color': item.color }"
          v-for="item in sideColors"
          @click="handleSideTheme(item.name)">
          <svg-icon name="ele-check"></svg-icon>
        </span>
      </div>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.themeColor') }}</span>
      <span class="comp-style quick-color-wrap">
        <el-color-picker v-model="theme" :predefine="predefineColors" @change="themeChange" />
      </span>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.componentStyle') }}</span>
    </div>
    <div class="style-preset-wrap">
      <div
        v-for="style in stylePresets"
        :key="style.name"
        class="style-preset-card"
        :class="{ activeStyle: componentStyle === style.name }"
        @click="applyComponentStyle(style)">
        <div class="style-preview">
          <div class="preview-btn" :style="{ borderRadius: style.radius }"></div>
          <div class="preview-input" :style="{ borderRadius: style.radius }"></div>
        </div>
        <span class="style-label">{{ style.label }}</span>
      </div>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.layoutDensity') }}</span>
    </div>
    <div class="style-preset-wrap">
      <div
        v-for="item in densityPresets"
        :key="item.name"
        class="style-preset-card"
        :class="{ activeStyle: layoutDensity === item.name }"
        @click="applyDensity(item)">
        <div class="style-preview">
          <div class="preview-row" v-for="n in 3" :key="n" :style="{ height: item.rowHeight, marginBottom: item.gap }"></div>
        </div>
        <span class="style-label">{{ item.label }}</span>
      </div>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.shadowStyle') }}</span>
    </div>
    <div class="style-preset-wrap">
      <div
        v-for="item in shadowPresets"
        :key="item.name"
        class="style-preset-card"
        :class="{ activeStyle: shadowStyle === item.name }"
        @click="applyShadow(item)">
        <div class="style-preview">
          <div class="preview-shadow-box" :style="{ boxShadow: item.preview }"></div>
        </div>
        <span class="style-label">{{ item.label }}</span>
      </div>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.fontSize') }}</span>
    </div>
    <div class="style-preset-wrap">
      <div
        v-for="item in fontSizePresets"
        :key="item.name"
        class="style-preset-card"
        :class="{ activeStyle: fontSize === item.name }"
        @click="applyFontSize(item)">
        <div class="style-preview">
          <span class="preview-font" :style="{ fontSize: item.previewSize }">Aa</span>
        </div>
        <span class="style-label">{{ item.label }}</span>
      </div>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.animationSpeed') }}</span>
    </div>
    <div class="style-preset-wrap">
      <div
        v-for="item in animationPresets"
        :key="item.name"
        class="style-preset-card"
        :class="{ activeStyle: animationSpeed === item.name }"
        @click="applyAnimation(item)">
        <div class="style-preview">
          <div class="preview-anim-bar" :style="{ animationDuration: item.preview }"></div>
        </div>
        <span class="style-label">{{ item.label }}</span>
      </div>
    </div>
    <el-divider />

    <div class="drawer-item">
      <span>{{ $t('layout.open') }} {{ $t('layout.tagsView') }}</span>
      <span class="comp-style">
        <el-switch v-model="tagsView" class="drawer-switch" />
      </span>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.open') }} {{ $t('layout.bottomBar') }}</span>
      <span class="comp-style">
        <el-switch v-model="showFooter" class="drawer-switch" />
      </span>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.openWatermark') }}</span>
      <span class="comp-style">
        <el-switch v-model="showWatermark" class="drawer-switch" />
      </span>
    </div>
    <!-- <div class="drawer-item">
      <span>{{ $t('layout.fixed') }} Header</span>
      <span class="comp-style">
        <el-switch v-model="fixedHeader" class="drawer-switch" />
      </span>
    </div> -->

    <div class="drawer-item">
      <span>{{ $t('layout.show') }} Logo</span>
      <span class="comp-style">
        <el-switch v-model="sidebarLogo" class="drawer-switch" />
      </span>
    </div>

    <div class="drawer-item">
      <span>{{ $t('layout.dynamicTitle') }}</span>
      <span class="comp-style">
        <el-switch v-model="dynamicTitle" class="drawer-switch" />
      </span>
    </div>

    <div class="drawer-item">
      <span>{{ $t('layout.tagsPersist') }}</span>
      <span class="comp-style">
        <el-switch v-model="tabsPersist" class="drawer-switch" />
      </span>
    </div>
    <div class="drawer-item">
      <span>{{ $t('layout.tagsShowIcon') }}</span>
      <span class="comp-style">
        <el-switch v-model="tabsShowIcon" class="drawer-switch" />
      </span>
    </div>
    <el-divider />

    <!-- <el-button type="primary" plain icon="DocumentAdd" @click="saveSetting">{{ $t('layout.saveConfig') }}</el-button> -->
    <el-button plain icon="Refresh" @click="resetSetting">{{ $t('layout.resetConfig') }}</el-button>
  </el-drawer>
</template>

<script setup>
import 'element-plus/theme-chalk/index.css'
import 'element-plus/theme-chalk/dark/css-vars.css'
import { useColorMode } from '@vueuse/core'
import { useDynamicTitle } from '@/utils/dynamicTitle'
import { getLightColor } from '@/utils/index'
import { getmark } from '@/utils/wartermark'
import useAppStore from '@/store/modules/app'
import useSettingsStore from '@/store/modules/settings'
import usePermissionStore from '@/store/modules/permission'
import useUserStore from '@/store/modules/user'
const { proxy } = getCurrentInstance()
const appStore = useAppStore()
const settingsStore = useSettingsStore()
const permissionStore = usePermissionStore()
const showSettings = ref(false)
const theme = ref(settingsStore.theme)
const sideTheme = ref(settingsStore.sideTheme)
const storeSettings = computed(() => settingsStore)
const predefineColors = ref(['#409EFF', '#ff4500', '#ff8c00', '#00ced1', '#1e90ff', '#c71585'])
const currentPreset = ref('')
const componentStyle = ref(settingsStore.componentStyle || 'default')
const stylePresets = [
  { name: 'sharp', label: '锐利', radius: '2px', borderRadiusBase: '2px', borderRadiusSmall: '1px', borderRadiusRound: '10px' },
  { name: 'default', label: '默认', radius: '4px', borderRadiusBase: '4px', borderRadiusSmall: '2px', borderRadiusRound: '20px' },
  { name: 'rounded', label: '圆润', radius: '8px', borderRadiusBase: '8px', borderRadiusSmall: '4px', borderRadiusRound: '20px' },
  { name: 'capsule', label: '胶囊', radius: '20px', borderRadiusBase: '20px', borderRadiusSmall: '12px', borderRadiusRound: '20px' },
]
function applyComponentStyle(style) {
  componentStyle.value = style.name
  settingsStore.changeSetting({ key: 'componentStyle', value: style.name })
  const root = document.documentElement
  root.style.setProperty('--el-border-radius-base', style.borderRadiusBase)
  root.style.setProperty('--el-border-radius-small', style.borderRadiusSmall)
  root.style.setProperty('--el-border-radius-round', style.borderRadiusRound)
  root.style.setProperty('--el-input-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-tag-border-radius', style.borderRadiusSmall)
  root.style.setProperty('--el-dialog-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-card-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-messagebox-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-popover-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-tooltip-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-dropdown-menuItem-border-radius', style.borderRadiusSmall)
  root.style.setProperty('--el-table-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-pagination-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-select-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-cascader-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-color-picker-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-date-picker-border-radius', style.borderRadiusBase)
  root.style.setProperty('--el-switch-border-radius', style.borderRadiusRound)
}

const layoutDensity = ref(settingsStore.layoutDensity || 'default')
const densityPresets = [
  { name: 'compact', label: '紧凑', rowHeight: '6px', gap: '2px', size: 'small' },
  { name: 'default', label: '默认', rowHeight: '8px', gap: '3px', size: 'default' },
  { name: 'comfortable', label: '宽松', rowHeight: '12px', gap: '5px', size: 'large' },
]
function applyDensity(item) {
  layoutDensity.value = item.name
  settingsStore.changeSetting({ key: 'layoutDensity', value: item.name })
  const root = document.documentElement
  root.style.setProperty('--el-component-size', item.size === 'small' ? '24px' : item.size === 'large' ? '40px' : '32px')
  root.style.setProperty('--el-form-item-margin-bottom', item.name === 'compact' ? '12px' : item.name === 'comfortable' ? '26px' : '18px')
}

const shadowStyle = ref(settingsStore.shadowStyle || 'light')
const shadowPresets = [
  { name: 'none', label: '无阴影', preview: 'none' },
  { name: 'light', label: '轻阴影', preview: '0 2px 12px 0 rgba(0,0,0,0.06)' },
  { name: 'heavy', label: '重阴影', preview: '0 4px 20px 0 rgba(0,0,0,0.15)' },
]
function applyShadow(item) {
  shadowStyle.value = item.name
  settingsStore.changeSetting({ key: 'shadowStyle', value: item.name })
  const root = document.documentElement
  if (item.name === 'none') {
    root.style.setProperty('--el-box-shadow', 'none')
    root.style.setProperty('--el-box-shadow-light', 'none')
    root.style.setProperty('--el-box-shadow-lighter', 'none')
    root.style.setProperty('--el-box-shadow-dark', 'none')
  } else if (item.name === 'heavy') {
    root.style.setProperty('--el-box-shadow', '0 4px 20px 0 rgba(0,0,0,0.15)')
    root.style.setProperty('--el-box-shadow-light', '0 2px 12px 0 rgba(0,0,0,0.12)')
    root.style.setProperty('--el-box-shadow-lighter', '0 1px 6px 0 rgba(0,0,0,0.08)')
    root.style.setProperty('--el-box-shadow-dark', '0 6px 24px 0 rgba(0,0,0,0.22)')
  } else {
    root.style.removeProperty('--el-box-shadow')
    root.style.removeProperty('--el-box-shadow-light')
    root.style.removeProperty('--el-box-shadow-lighter')
    root.style.removeProperty('--el-box-shadow-dark')
  }
}

const fontSize = ref(settingsStore.fontSize || 'medium')
const fontSizePresets = [
  { name: 'small', label: '小', previewSize: '11px', value: '12px' },
  { name: 'medium', label: '中', previewSize: '14px', value: '14px' },
  { name: 'large', label: '大', previewSize: '17px', value: '16px' },
]
function applyFontSize(item) {
  fontSize.value = item.name
  settingsStore.changeSetting({ key: 'fontSize', value: item.name })
  document.documentElement.style.setProperty('--el-font-size-base', item.value)
}

const animationSpeed = ref(settingsStore.animationSpeed || 'normal')
const animationPresets = [
  { name: 'fast', label: '快速', preview: '0.6s', duration: '0.15s', fast: '0.1s' },
  { name: 'normal', label: '正常', preview: '1.2s', duration: '0.3s', fast: '0.2s' },
  { name: 'slow', label: '缓慢', preview: '2.4s', duration: '0.6s', fast: '0.4s' },
]
function applyAnimation(item) {
  animationSpeed.value = item.name
  settingsStore.changeSetting({ key: 'animationSpeed', value: item.name })
  const root = document.documentElement
  root.style.setProperty('--el-transition-duration', item.duration)
  root.style.setProperty('--el-transition-duration-fast', item.fast)
}

const presetThemes = [
  { name: 'default', label: '默认蓝', primaryColor: '#409EFF', mode: 'light', sideTheme: '', componentStyle: 'default', density: 'default', shadow: 'light', font: 'medium', anim: 'normal', previewBg: '#f0f2f5', sidebarColor: '#fff' },
  { name: 'dark', label: '暗夜黑', primaryColor: '#409EFF', mode: 'dark', sideTheme: '', componentStyle: 'sharp', density: 'compact', shadow: 'heavy', font: 'medium', anim: 'fast', previewBg: '#141414', sidebarColor: '#000' },
  { name: 'techBlue', label: '科技蓝', primaryColor: '#2F54EB', mode: 'light', sideTheme: 'theme-black', componentStyle: 'sharp', density: 'compact', shadow: 'light', font: 'small', anim: 'fast', previewBg: '#f0f2f5', sidebarColor: '#001529' },
  { name: 'freshGreen', label: '清新绿', primaryColor: '#52C41A', mode: 'light', sideTheme: '', componentStyle: 'rounded', density: 'comfortable', shadow: 'light', font: 'medium', anim: 'normal', previewBg: '#f0f2f5', sidebarColor: '#fff' },
  { name: 'elegantPurple', label: '优雅紫', primaryColor: '#722ED1', mode: 'light', sideTheme: 'theme-black', componentStyle: 'rounded', density: 'default', shadow: 'heavy', font: 'large', anim: 'slow', previewBg: '#f0f2f5', sidebarColor: '#1f1f2e' },
  { name: 'warmOrange', label: '活力橙', primaryColor: '#FA541C', mode: 'light', sideTheme: '', componentStyle: 'capsule', density: 'comfortable', shadow: 'none', font: 'large', anim: 'normal', previewBg: '#f0f2f5', sidebarColor: '#fff' },
]
function applyPreset(preset) {
  currentPreset.value = preset.name
  mode.value = preset.mode
  nextTick(() => {
    themeChange(preset.primaryColor)
    handleSideTheme(preset.sideTheme)
    const style = stylePresets.find((s) => s.name === preset.componentStyle)
    if (style) applyComponentStyle(style)
    const density = densityPresets.find((d) => d.name === preset.density)
    if (density) applyDensity(density)
    const shadow = shadowPresets.find((s) => s.name === preset.shadow)
    if (shadow) applyShadow(shadow)
    const font = fontSizePresets.find((f) => f.name === preset.font)
    if (font) applyFontSize(font)
    const anim = animationPresets.find((a) => a.name === preset.anim)
    if (anim) applyAnimation(anim)
  })
}
const sideColors = ref([
  { color: '#324157', name: 'theme-black' },
  { color: '#fff', name: '' }
])
const { setWatermark, removeWatermark } = getmark()
// 可以手动更改当前值 model.value = 'cafe'
const mode = useColorMode({
  modes: {
    // custom colors
    contrast: 'dark contrast',
    cafe: 'cafe',
    auto: 'auto'
  }
})
/** 是否需要topnav */
const topNav = computed({
  get: () => storeSettings.value.topNav,
  set: (val) => {
    settingsStore.changeSetting({ key: 'topNav', value: val })
    if (!val) {
      appStore.toggleSideBarHide(false)
      permissionStore.setSidebarRouters(permissionStore.defaultRoutes)
    }
  }
})
/**导航类型 */
const navType = computed({
  get: () => storeSettings.value.navType,
  set: (val) => {
    settingsStore.changeSetting({ key: 'navType', value: val })
  }
})
/** 是否需要tagview */
const tagsView = computed({
  get: () => storeSettings.value.tagsView,
  set: (val) => {
    settingsStore.changeSetting({ key: 'tagsView', value: val })
  }
})
/**是否需要固定头部 */
const fixedHeader = computed({
  get: () => storeSettings.value.fixedHeader,
  set: (val) => {
    settingsStore.changeSetting({ key: 'fixedHeader', value: val })
  }
})
// 是否显示底部
const showFooter = computed({
  get: () => storeSettings.value.showFooter,
  set: (val) => {
    settingsStore.changeSetting({ key: 'showFooter', value: val })
  }
})
/**是否需要侧边栏的logo */
const sidebarLogo = computed({
  get: () => storeSettings.value.sidebarLogo,
  set: (val) => {
    settingsStore.changeSetting({ key: 'sidebarLogo', value: val })
  }
})
/**是否需要侧边栏的动态网页的title */
const dynamicTitle = computed({
  get: () => storeSettings.value.dynamicTitle,
  set: (val) => {
    settingsStore.changeSetting({ key: 'dynamicTitle', value: val })
    // 动态设置网页标题
    useDynamicTitle()
  }
})
/**是否显示水印 */
const showWatermark = computed({
  get: () => storeSettings.value.showWatermark,
  set: (val) => {
    settingsStore.changeSetting({ key: 'showWatermark', value: val })
    changeWatermark()
  }
})

/**标签持久化 */
const tabsPersist = computed({
  get: () => storeSettings.value.tagsViewPersist,
  set: (val) => {
    settingsStore.changeSetting({ key: 'tagsViewPersist', value: val })
  }
})
/**标签显示icon */
const tabsShowIcon = computed({
  get: () => storeSettings.value.tagsShowIcon,
  set: (val) => {
    settingsStore.changeSetting({ key: 'tagsShowIcon', value: val })
  }
})
const changeWatermark = () => {
  storeSettings.value.showWatermark ? setWatermark(useUserStore().userInfo.userName) : removeWatermark()
}
// 开启水印
changeWatermark()
// 初始化样式设置
const savedStyle = stylePresets.find((s) => s.name === componentStyle.value)
if (savedStyle) applyComponentStyle(savedStyle)
const savedDensity = densityPresets.find((d) => d.name === layoutDensity.value)
if (savedDensity) applyDensity(savedDensity)
const savedShadow = shadowPresets.find((s) => s.name === shadowStyle.value)
if (savedShadow) applyShadow(savedShadow)
const savedFont = fontSizePresets.find((f) => f.name === fontSize.value)
if (savedFont) applyFontSize(savedFont)
const savedAnim = animationPresets.find((a) => a.name === animationSpeed.value)
if (savedAnim) applyAnimation(savedAnim)
// 监控主题颜色
watch(
  () => theme,
  (val) => {
    themeChange(val.value)
  },
  {
    immediate: true
  }
)
watch(
  () => sideTheme,
  (val) => {
    const body = document.documentElement
    body.setAttribute('data-theme', '')
  },
  {
    immediate: true
  }
)
watch(
  () => mode,
  (val) => {
    settingsStore.changeSetting({ key: 'codeMode', value: val.value })
    document.documentElement.setAttribute('data-vxe-ui-theme', val.value === 'dark' ? 'dark' : 'light')
    if (val.value === 'dark') {
      handleSideTheme('')
    }
  },
  {
    immediate: true,
    deep: true
  }
)
// 导航模式
watch(
  () => navType,
  (val) => {
    //侧边栏
    if (val.value == 1) {
      appStore.toggleSideBarHide(false)
      permissionStore.setSidebarRouters(permissionStore.defaultRoutes)
    }
    //top
    if (val.value == 3) {
      appStore.toggleSideBarHide(true)
      permissionStore.setSidebarRouters(permissionStore.defaultRoutes)
    }
  },
  {
    immediate: true,
    deep: true
  }
)
/**
 * 改变主题颜色
 */
function themeChange(val) {
  settingsStore.changeSetting({ key: 'theme', value: val })
  theme.value = val
  // 设置element-plus ui主题
  document.documentElement.style.setProperty('--el-color-primary', val)
  document.documentElement.style.setProperty('--el-color-primary-dark-2', val)
  var num = mode.value == 'dark' ? 8 : 9

  // 颜色变浅
  for (let i = 1; i <= num; i++) {
    document.documentElement.style.setProperty(`--el-color-primary-light-${i}`, `${getLightColor(val, i / 10)}`)
  }
}
function handleSideTheme(val) {
  settingsStore.changeSetting({ key: 'sideTheme', value: val })
  sideTheme.value = val
  const body = document.documentElement
  if (val == 'theme-black') body.setAttribute('data-theme', 'theme-black')
  else body.removeAttribute('data-theme')
}
function handleNavType(val) {
  settingsStore.changeSetting({ key: 'navType', value: val })
}
function saveSetting() {
  proxy.$modal.loading(proxy.$t('layout.savingLocal'))
  // let layoutSetting = {
  //   topNav: storeSettings.value.topNav,
  //   tagsView: storeSettings.value.tagsView,
  //   fixedHeader: storeSettings.value.fixedHeader,
  //   sidebarLogo: storeSettings.value.sidebarLogo,
  //   dynamicTitle: storeSettings.value.dynamicTitle,
  //   sideTheme: storeSettings.value.sideTheme,
  //   theme: storeSettings.value.theme,
  //   showFooter: storeSettings.value.showFooter,
  //   showWatermark: storeSettings.value.showWatermark,
  //   tagsViewPersist: storeSettings.value.tagsViewPersist,
  //   tagsShowIcon: storeSettings.value.tagsShowIcon
  // }
  // localStorage.setItem('layout-setting', JSON.stringify(layoutSetting))
  setTimeout(proxy.$modal.closeLoading(), 100)
  setTimeout('window.location.reload()', 200)
}
function resetSetting() {
  proxy.$modal.loading(proxy.$t('layout.clearingCache'))
  localStorage.removeItem('layout-setting')
  setTimeout('window.location.reload()', 1000)
}
function openSetting() {
  showSettings.value = true
}

defineExpose({
  openSetting
})
</script>

<style lang="scss" scoped>
.setting-drawer-title {
  margin-bottom: 12px;
  color: var(--base-text-color-rgba);
  line-height: 22px;
  font-weight: bold;
  .drawer-title {
    font-size: 14px;
  }
}

// 导航模式
.nav-wrap {
  display: flex;
  justify-content: flex-start;
  align-items: center;
  margin-top: 10px;
  margin-bottom: 20px;

  .activeItem {
    border: 2px solid var(--el-color-primary) !important;
  }

  .item {
    position: relative;
    margin-right: 16px;
    cursor: pointer;
    width: 56px;
    height: 48px;
    border-radius: 4px;
    background: #f0f2f5;
    border: 2px solid transparent;
    // box-shadow: 0 1px 2.5px #0000002e;
  }

  .left {
    b:first-child {
      display: block;
      height: 30%;
      background: #fff;
    }
    b:last-child {
      width: 30%;
      background: #1b2a47;
      position: absolute;
      height: 100%;
      top: 0;
      border-radius: 4px 0 0 4px;
    }
  }
  .mix {
    b:first-child {
      border-radius: 4px 4px 0 0;
      display: block;
      height: 30%;
      background: #1b2a47;
    }
    b:last-child {
      width: 30%;
      background: #1b2a47;
      position: absolute;
      height: 70%;
      border-radius: 0 0 0 4px;
    }
  }
  .top {
    b:first-child {
      display: block;
      height: 30%;
      background: #1b2a47;
      border-radius: 4px 4px 0 0;
    }
  }
}

.style-preset-wrap {
  display: flex;
  gap: 8px;
  margin: 8px 0 12px;

  .style-preset-card {
    flex: 1;
    cursor: pointer;
    text-align: center;
    border: 2px solid transparent;
    border-radius: 6px;
    padding: 6px 4px;
    transition: border-color 0.2s;

    &:hover {
      border-color: var(--el-color-primary-light-5);
    }
  }
  .activeStyle {
    border-color: var(--el-color-primary);
  }
  .style-preview {
    display: flex;
    flex-direction: column;
    gap: 4px;
    align-items: center;

    .preview-btn {
      width: 32px;
      height: 12px;
      background: var(--el-color-primary);
    }
    .preview-input {
      width: 40px;
      height: 10px;
      border: 1px solid var(--el-border-color);
    }
    .preview-row {
      width: 80%;
      background: var(--el-border-color-light);
      border-radius: 2px;
    }
    .preview-shadow-box {
      width: 32px;
      height: 24px;
      border-radius: 4px;
      background: var(--el-bg-color);
    }
    .preview-font {
      font-weight: 600;
      color: var(--base-text-color-rgba);
    }
    .preview-anim-bar {
      width: 80%;
      height: 6px;
      border-radius: 3px;
      background: var(--el-color-primary);
      animation: animPulse infinite alternate ease-in-out;
    }
  }
  .style-label {
    font-size: 12px;
    margin-top: 4px;
    display: block;
    color: var(--base-text-color-rgba);
  }
}

.preset-wrap {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 10px;
  margin-bottom: 10px;

  .preset-card {
    cursor: pointer;
    text-align: center;
    border: 2px solid transparent;
    border-radius: 6px;
    padding: 4px;
    transition: border-color 0.2s;

    &:hover {
      border-color: var(--el-color-primary-light-5);
    }
  }
  .activePreset {
    border-color: var(--el-color-primary);
  }
  .preset-preview {
    height: 48px;
    border-radius: 4px;
    display: flex;
    overflow: hidden;

    .preset-sidebar {
      width: 30%;
      height: 100%;
    }
    .preset-content {
      flex: 1;
      display: flex;
      flex-direction: column;

      .preset-header {
        height: 30%;
      }
    }
  }
  .preset-label {
    font-size: 12px;
    margin-top: 4px;
    display: block;
    color: var(--base-text-color-rgba);
  }
}

.drawer-item {
  color: var(--base-text-color-rgba);
  padding: 12px 0;
  font-size: 14px;

  .color-item {
    width: 25px;
    height: 25px;
    display: inline-flex;
    margin-right: 10px;
    cursor: pointer;
    border-radius: 3px;
    border: 2px solid #ccc;
    position: relative;
    --color: transparent;

    .el-icon {
      height: 1.6em;
      width: 1.6em;
    }
  }
  .sideActive {
    --color: #fff;
    border: 2px solid var(--el-color-primary);
  }
  .comp-style {
    float: right;
    margin: -3px 8px 0px 0px;
  }
  .quick-color-wrap {
    display: flex;
    align-items: center;

    span {
      width: 15px;
      height: 15px;
      margin-right: 10px;
      cursor: pointer;
    }
  }
}

@keyframes animPulse {
  0% { opacity: 0.3; }
  100% { opacity: 1; }
}
</style>

<style lang="scss">
.el-tag {
  --el-tag-border-radius: var(--el-border-radius-base) !important;
}
.el-input__wrapper {
  border-radius: var(--el-border-radius-base);
}
.el-select .el-input__wrapper {
  border-radius: var(--el-border-radius-base);
}
.el-textarea__inner {
  border-radius: var(--el-border-radius-base);
}
.el-pagination {
  --el-pagination-button-border-radius: var(--el-border-radius-base) !important;
}
.el-date-picker {
  --el-date-picker-border-radius: var(--el-border-radius-base) !important;
}
.el-cascader .el-input__wrapper {
  border-radius: var(--el-border-radius-base);
}
</style>
