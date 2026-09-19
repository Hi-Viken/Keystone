<template>
  <div class="inner-link-container">
    <div v-if="showLoading">加载中...</div>
    <iframe :id="iframeId" style="width: 100%; height: 100%" :src="src" ref="iframeRef" frameborder="no"></iframe>
  </div>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'

const props = defineProps({
  src: {
    type: String,
    default: '/'
  },
  iframeId: {
    type: String
  }
})
const showLoading = ref(true)
const iframeRef = ref(null)
let resizeObserver = null

const updateHeight = () => {
  document.documentElement.style.setProperty('--inner-link-height', `${document.documentElement.clientHeight - 94.5}px`)
}

onMounted(() => {
  // 初始化高度
  updateHeight()

  // 使用ResizeObserver监听窗口大小变化
  resizeObserver = new ResizeObserver(updateHeight)
  resizeObserver.observe(document.documentElement)

  if (iframeRef.value) {
    iframeRef.value.onload = () => {
      showLoading.value = false
      console.log('Iframe 加载完成')
    }
  }
})

onBeforeUnmount(() => {
  if (resizeObserver) {
    resizeObserver.disconnect()
  }
})
</script>

<style scoped>
.inner-link-container {
  height: var(--inner-link-height, calc(100vh - 94.5px));
}
</style>
