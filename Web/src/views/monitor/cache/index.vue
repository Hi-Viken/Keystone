<template>
  <div class="app-container">
    <el-row>
      <el-col :lg="24" class="card-box">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ $t('cacheView.basicInfo') }}</span>
            </div>
          </template>
          <div class="el-table el-table--enable-row-hover el-table--medium">
            <table cellspacing="0" style="width: 100%">
              <tbody>
                <tr>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.redisVersion') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.redis_version }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.runMode') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.redis_mode == 'standalone' ? $t('cacheView.standalone') : $t('cacheView.cluster') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.port') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.tcp_port }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.clients') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.connected_clients }}</div>
                  </td>
                </tr>
                <tr>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.uptime') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.uptime_in_days }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.usedMemory') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.used_memory_human }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.usedCPU') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ parseFloat(cache.info.used_cpu_user_children).toFixed(2) }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.memoryConfig') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.maxmemory_human }}</div>
                  </td>
                </tr>
                <tr>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.aofEnabled') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.aof_enabled == '0' ? $t('common.no') : $t('common.yes') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.rdbSuccess') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">{{ cache.info.rdb_last_bgsave_status }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.keyCount') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.dbSize">{{ cache.dbSize }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell">{{ $t('cacheView.networkIO') }}</div>
                  </td>
                  <td class="el-table__cell is-leaf">
                    <div class="cell" v-if="cache.info">
                      {{ cache.info.instantaneous_input_kbps }}kps/{{ cache.info.instantaneous_output_kbps }}kps
                    </div>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </el-card>
      </el-col>

      <el-col :lg="8" class="card-box">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ $t('cacheView.commandStats') }}</span>
            </div>
          </template>
          <div class="el-table el-table--enable-row-hover el-table--medium">
            <div ref="commandstatsRef" style="height: 300px" />
          </div>
        </el-card>
      </el-col>

      <el-col :lg="8" class="card-box" v-if="cache.info">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ $t('cacheView.memoryUsage') }}</span>
            </div>
          </template>
          <div class="el-table el-table--enable-row-hover el-table--medium">
            <gauge
              :name="$t('cacheView.memoryUsage')"
              :max="100"
              :data="[
                {
                  value: (parseFloat(cache.info.used_memory_human) / parseFloat(cache.info.total_system_memory_human)).toFixed(2),
                  name: $t('cacheView.memoryUsage'),
                },
              ]" />
          </div>
        </el-card>
      </el-col>
      <el-col :lg="8" class="card-box" v-if="cache.info">
        <el-card>
          <template #header>
            <div class="card-header">
              <span>{{ $t('cacheView.cpuUsage') }}</span>
            </div>
          </template>
          <div class="el-table el-table--enable-row-hover el-table--medium">
            <gauge name="CPU" :max="100" :data="[{ value: parseFloat(cache.info.used_cpu_user_children * 100).toFixed(0), name: $t('cacheView.cpuUsage') }]" />
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup name="cache">
import { getCache } from '@/api/monitor/cache'
import * as echarts from 'echarts'
import Gauge from '@/components/Echarts/Gauge.vue'

// 统计命令信息
const commandstats = ref(null)
// 使用内存
const usedmemory = ref(null)
// cache信息
const cache = ref([])
const commandstatsRef = ref(null)
const { proxy } = getCurrentInstance()

/** 查缓存询信息 */
function getList() {
  getCache().then((response) => {
    cache.value = response.data
    // this.$modal.closeLoading();

    // 命令使用占比
    commandstats.value = echarts.init(proxy.$refs.commandstatsRef)
    commandstats.value.setOption({
      tooltip: {
        trigger: 'item',
        formatter: '{a} <br/>{b} : {c} ({d}%)',
      },
      series: [
        {
          name: proxy.$t('cacheView.command'),
          type: 'pie',
          roseType: 'radius',
          radius: [15, 95],
          center: ['50%', '38%'],
          data: response.data.commandStats,
          animationEasing: 'cubicInOut',
          animationDuration: 1000,
        },
      ],
    })
  })
}

onMounted(() => {
  getList()
})
</script>
