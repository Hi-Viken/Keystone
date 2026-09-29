<template>
  <div class="echarts" ref="chartsRef" />
</template>
<script setup>
import * as echarts from 'echarts'
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
const chartsRef = ref(null)

const getOptions = () => ({
  title: {
    text: t('dataScreen.onlineStats'),
    // 文字属性设置
    textStyle: {
      color: '#00e4ff'
    }
  },
  grid: {
    top: '10%',
    left: '3%',
    right: '4%',
    bottom: '10%',
    containLabel: true
  },
  tooltip: {
    trigger: 'axis',
    axisPointer: {
      type: 'cross'
    },
    padding: [5, 10]
  },
  yAxis: {
    // 设置坐标轴的 文字样式
    axisLabel: {
      color: '#bbdaff',
      margin: 20 // 刻度标签与轴线之间的距离。
    },
    // 坐标轴轴线相关设置。
    splitLine: {
      lineStyle: {
        color: '#fff'
      }
    }
  },
  xAxis: {
    splitLine: {
      show: false
    },
    // 坐标轴轴线相关设置。
    axisLine: {
      lineStyle: {
        color: 'orange'
      }
    },
    type: 'category',
    data: [t('dataScreen.mon'), t('dataScreen.tue'), t('dataScreen.wed'), t('dataScreen.thu'), t('dataScreen.fri'), t('dataScreen.sat'), t('dataScreen.sun')],
    axisLabel: {
      // 设置坐标轴的 文字样式
      color: '#bbdaff',
      margin: 20 // 刻度标签与轴线之间的距离。
    },
    boundaryGap: false, // 设置坐标轴两边的留白 ，从刻度原点开始，
    axisTick: {
      // 取消坐标轴刻度线
      show: false
    }
  },
  series: [
    {
      name: t('dataScreen.onlineCount'),
      itemStyle: {
        color: 'orange',
        lineStyle: {
          color: '#FF005A',
          width: 2
        }
      },
      symbol: 'circle',
      markLine: {
        silent: true
      },
      type: 'line',
      data: [154, 230, 224, 218, 135, 147, 260],
      animationDuration: 2800,
      animationEasing: 'cubicInOut'
    }
  ]
})

let chart = null
const initChart = () => {
  const chart = echarts.init(chartsRef.value)
  chart.setOption(getOptions())
  return chart
}
onMounted(() => {
  chart = initChart()
  window.addEventListener('resize', function () {
    chart && chart.resize()
  })
})
</script>
<style lang="scss" scoped>
.echarts {
  height: 100%;
  width: 100%;
}
</style>
