<template>
  <div class="echarts" ref="chartsRef" />
</template>
<script setup>
import * as echarts from 'echarts'
import { useI18n } from 'vue-i18n'
const { t } = useI18n()
const chartsRef = ref(null)

const getOptions = () => ({
  tooltip: {
    formatter: '{a} <br/>{b} : {c}%'
  },
  title: {
    text: t('dataScreen.airMetrics'),
    // 文字属性设置
    textStyle: {
      color: '#00e4ff'
    }
  },
  series: [
    {
      name: 'Pressure',
      type: 'gauge',
      progress: {
        show: true
      },
      detail: {
        valueAnimation: true,
        formatter: '{value}'
      },
      data: [
        {
          value: 50,
          name: 'SCORE'
        }
      ]
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
