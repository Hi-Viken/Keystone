// import Cookies from 'js-cookie'
import cache from '@/plugins/cache'
import defaultSettings from '@/settings'

const localeMap = {
  'zh-cn': 'zh-CN',
  'zh-tw': 'zh-TW',
  'en': 'en-US',
  'en-us': 'en-US',
  'ja-jp': 'ja-JP',
  'ko-kr': 'ko-KR'
}
function normalizeLocale(lang) {
  if (!lang) return defaultSettings.defaultLang
  const normalized = localeMap[lang.toLowerCase()] || lang
  if (normalized !== lang) {
    cache.local.set('lang', normalized)
  }
  return normalized
}

const useAppStore = defineStore('app', {
  state: () => ({
    sidebar: {
      opened: false,
      hide: false
    },
    device: 'desktop',
    size: cache.local.get('size') || defaultSettings.defaultSize,
    lang: normalizeLocale(cache.local.get('lang') || defaultSettings.defaultLang)
  }),
  actions: {
    toggleSideBar() {
      this.sidebar.opened = !this.sidebar.opened
    },
    closeSideBar() {
      this.sidebar.opened = false
    },
    toggleDevice(device) {
      this.device = device
    },
    setSize(size) {
      this.size = size
      cache.local.set('size', size)
    },
    toggleSideBarHide(status) {
      this.sidebar.hide = status
    },
    setLang(lang) {
      const normalized = normalizeLocale(lang)
      this.lang = normalized
      cache.local.set('lang', normalized)
    }
  }
})

export default useAppStore
