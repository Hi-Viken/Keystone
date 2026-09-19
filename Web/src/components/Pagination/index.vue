<template>
  <div :class="{ hidden: hidden }" class="pagination-container">
    <el-pagination
      background
      v-model:current-page="currentPage"
      v-model:page-size="pageSize"
      :layout="layout"
      :page-sizes="pageSizes"
      :pager-count="pagerCountValue"
      :total="total"
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange" />
  </div>
</template>

<script>
// import { scrollTo } from "@/utils/scroll-to";
import { computed, ref, onMounted, onBeforeUnmount } from 'vue'
export default {
  name: 'pagingation',
  emits: ['update:page', 'update:limit', 'pagination'],
  props: {
    total: {
      required: true,
      type: Number
    },
    page: {
      type: Number,
      default: 1
    },
    limit: {
      type: Number,
      default: 20
    },
    pageSizes: {
      type: Array,
      default() {
        return [10, 20, 30, 50, 100]
      }
    },
    // 移动端页码按钮的数量端默认值5
    pagerCount: {
      type: Number,
      default: 7
    },
    layout: {
      type: String,
      default: 'total, sizes, prev, pager, next, jumper'
    },
    background: {
      type: Boolean,
      default: true
    },
    autoScroll: {
      type: Boolean,
      default: true
    },
    hidden: {
      type: Boolean,
      default: false
    }
  },
  setup(props, { ctx, emit }) {
    const currentPage = computed({
      get() {
        return props.page
      },
      set(val) {
        emit('update:page', val)
      }
    })
    const pageSize = computed({
      get() {
        return props.limit
      },
      set(val) {
        emit('update:limit', val)
      }
    })
    
    const pagerCountValue = ref(props.pagerCount)
    let resizeObserver = null

    const updatePagerCount = () => {
      // 使用requestAnimationFrame批量处理布局操作
      requestAnimationFrame(() => {
        pagerCountValue.value = document.body.clientWidth < 992 ? 5 : props.pagerCount
      })
    }

    function handleSizeChange(val) {
      emit('pagination', { page: currentPage.value, limit: val })
      if (props.autoScroll) {
        // scrollTo(0, 800);
      }
    }
    function handleCurrentChange(val) {
      emit('pagination', { page: val, limit: pageSize.value })
      if (props.autoScroll) {
        // scrollTo(0, 800);
      }
    }

    onMounted(() => {
      // 初始化pagerCount
      updatePagerCount()
      
      // 使用ResizeObserver监听窗口大小变化
      resizeObserver = new ResizeObserver(updatePagerCount)
      resizeObserver.observe(document.body)
    })

    onBeforeUnmount(() => {
      if (resizeObserver) {
        resizeObserver.disconnect()
        resizeObserver = null
      }
    })

    return {
      currentPage,
      pageSize,
      pagerCountValue,
      handleSizeChange,
      handleCurrentChange
    }
  }
}
</script>
<style scoped>
.pagination-container {
  /* background: #fff; */
  display: flex;
  justify-content: flex-end;
  margin-top: 20px;
}
.pagination-container.hidden {
  display: none;
}
</style>
