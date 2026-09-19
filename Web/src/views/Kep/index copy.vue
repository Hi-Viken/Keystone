<template>
  <div class="app-container">
   asdfas        <el-button
          :type="primary"
          :icon="VideoPlay"
          @click="handleToggleService"
        
          size="large"
        >
          {{ isRunning ? '停止服务' : '启动服务' }}
        </el-button>
  </div>
</template>

<script setup>
import { ref, onMounted, nextTick, onUnmounted } from 'vue'
import kepHub from '@/signalr/KepHub.js'
import { ElMessage } from 'element-plus'
const toggleLoading = ref(false)
const isRunning = ref(false)
const handleToggleService = async () => {
  console.log('handleToggleService')
  
  toggleLoading.value = true
  try {
    if (!await kepHub.ensureConnected()) {
      ElMessage.error('连接失败，请检查网络')
      return
    }
    const result = await kepHub.SR.invoke('SendMessage', 'hello world','kepHub')
    isRunning.value = result
    ElMessage.success(isRunning.value ? '服务启动成功' : '服务停止成功')
  } catch (error) {
    ElMessage.error('操作失败: ' + error.message)
  } finally {
    toggleLoading.value = false
  }
}
let logHandler = null

onMounted(async () => {
try {
    if (!await kepHub.ensureConnected()) {
      logs.value = [`[${new Date().toLocaleTimeString()}] 连接失败`]
      return
    }
    logHandler = (user,message) => {
      console.log('收到日志:', user)
      //appendLogs(data)
    }
      kepHub.SR.on('ReceiveMessage', logHandler)
    // await kepHub.SR.invoke('subscribeLog')
    // console.log('已订阅日志')
    // const status = await kepHub.SR.invoke('getServiceStatus')
    // isRunning.value = status
  } catch (error) {
    console.error('初始化失败:', error)
    logs.value = [`[${new Date().toLocaleTimeString()}] 连接失败: ${error.message}`]
  }
})



</script>

<style scoped>

</style>
