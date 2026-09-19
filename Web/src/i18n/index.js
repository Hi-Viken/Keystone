import { createI18n } from 'vue-i18n'
// import useAppStore from '@/store/modules/app'
import { listLangByLocale } from '@/api/system/commonlang.js'
import defaultSettings from '@/settings'
import cache from '@/plugins/cache'
const language = computed(() => {
  // return useAppStore().lang
  return cache.local.get('lang') || defaultSettings.defaultLang
})

import zhCn from './lang/zh-CN.json'
import en from './lang/en-US.json'
import zhTw from './lang/zh-TW.json'
import ja from './lang/ja-JP.json'
import ko from './lang/ko-KR.json'



import pageLoginCn from './pages/login/zh-CN.json'
import pageLoginEn from './pages/login/en-US.json'
import pageLoginTw from './pages/login/zh-TW.json'
import pageLoginJa from './pages/login/ja-JP.json'
import pageLoginKo from './pages/login/ko-KR.json'



// 菜单页面
import pagemenuCn from './pages/menu/zh-CN.json'
import pagemenuEn from './pages/menu/en-US.json'
import pagemenuTw from './pages/menu/zh-TW.json'
import pagemenuJa from './pages/menu/ja-JP.json'
import pagemenuKo from './pages/menu/ko-KR.json'



const i18n = createI18n({
  // 全局注入 $t 函数
  globalInjection: true,
  fallbackLocale: 'zh-CN',
  locale: language.value, //默认选择的语言
  legacy: false, // 使用 Composition API 模式，则需要将其设置为false
  messages: {
    'zh-CN': {
      ...zhCn,
      ...pageLoginCn,
      ...pagemenuCn
    },
    'zh-TW': {
      ...zhTw,
      ...pageLoginTw,
      ...pagemenuTw
    },
    'en-US': {
      ...en,
      ...pageLoginEn,
      ...pagemenuEn
    },
    'ja-JP': {
      ...ja,
      ...pageLoginJa,
      ...pagemenuJa
    },
    'ko-KR': {
      ...ko,
      ...pageLoginKo,
      ...pagemenuKo
    }
    //... 在这里添加其他语言支持
  }
})

const loadLocale = () => {
  listLangByLocale(language.value).then((res) => {
    const { code, data } = res
    if (code == 200) {
      i18n.global.mergeLocaleMessage(language.value, data)
    }
  })
}
loadLocale()
export default i18n
