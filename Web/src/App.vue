<template>
  <el-config-provider :locale="locale" :size="size">
    <router-view />
  </el-config-provider>
</template>
<script setup>
import useUserStore from './store/modules/user'
import useAppStore from './store/modules/app'
import { ElConfigProvider } from 'element-plus'
import { useI18n } from 'vue-i18n'
import zhCn from 'element-plus/dist/locale/zh-cn'
import en from 'element-plus/dist/locale/en'
import thTw from 'element-plus/dist/locale/zh-tw'
import defaultSettings from '@/settings'
const { proxy } = getCurrentInstance()
const { locale: i18nLocale } = useI18n()

const token = computed(() => {
  return useUserStore().userId
})

const lang = computed(() => {
  return useAppStore().lang
})
const locale = ref(zhCn)
const size = ref(defaultSettings.defaultSize)

size.value = useAppStore().size
watch(
  token,
  (val) => {
    if (val) {
      proxy.signalr.start().then(async (res) => {
        if (res) {
          await proxy.signalr.SR.invoke('logOut')
        }
      })
    }
  },
  {
    immediate: true,
    deep: true
  }
)
watch(
  lang,
  (val) => {
    i18nLocale.value = val
    if (val == 'en-US') {
      locale.value = en
    } else if (val == 'zh-TW') {
      locale.value = thTw
    } else {
      locale.value = zhCn
    }
  },
  {
    immediate: true
  }
)


console.log('🎉源码地址: https://github.com/Hi-Viken/Keystone')
console.log('📖官方文档：http://vkin.cc')
console.log('💰打赏作者：http://vkin.cc/doc/support.html')
console.log('📱移动端体验：http://demo.vkin.cc/h5')
</script>
<style lang="scss" scoped></style>
