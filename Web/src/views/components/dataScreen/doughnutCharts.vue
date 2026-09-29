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
    trigger: 'item'
  },
  title: {
    text: t('dataScreen.dataSource'),
    // 文字属性设置
    textStyle: {
      color: '#00e4ff'
    }
  },
  legend: {
    top: '5%',
    left: 'center',
    // 文字属性设置
    textStyle: {
      color: '#fff'
    },
    // 图形属性设置
    itemStyle: {}
  },
  series: [
    {
      name: t('dataScreen.visitSource'),
      type: 'pie',
      radius: ['40%', '70%'],
      avoidLabelOverlap: false,
      itemStyle: {
        borderRadius: 10,
        borderColor: '#fff',
        borderWidth: 2
      },
      label: {
        show: false,
        position: 'center'
      },
      emphasis: {
        label: {
          show: true,
          fontSize: 40,
          fontWeight: 'bold'
        }
      },
      labelLine: {
        show: false
      },
      data: [
        { value: 1048, name: t('dataScreen.searchEngine') },
        { value: 735, name: t('dataScreen.directVisit') },
        { value: 580, name: t('dataScreen.email') },
        { value: 484, name: t('dataScreen.other') },
        { value: 300, name: t('dataScreen.adClick') }
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
