<template>
  <div v-loading="loading" class="scalar-container">
    <iframe :src="src" frameborder="no" style="width: 100%;height: 100%" scrolling="auto" />
  </div>
</template>
<script>
import { ref, onMounted, onBeforeUnmount } from "vue";
export default {
  name: "Scalar",
  setup() {
    const src = ref(import.meta.env.VITE_APP_BASE_API + "/scalar/");
    const loading = ref(true);
    let resizeObserver = null;

    const updateHeight = () => {
      document.documentElement.style.setProperty('--scalar-height', `${document.documentElement.clientHeight - 94.5}px`);
    };

    onMounted(() => {
      // 初始化高度
      updateHeight();
      
      // 使用ResizeObserver监听窗口大小变化
      resizeObserver = new ResizeObserver(updateHeight);
      resizeObserver.observe(document.documentElement);

      setTimeout(() => {
        loading.value = false;
      }, 230);
    });

    onBeforeUnmount(() => {
      if (resizeObserver) {
        resizeObserver.disconnect();
      }
    });

    return {
      src,
      loading,
    };
  },
};
</script>
<style scoped>
.scalar-container {
  height: var(--scalar-height, calc(100vh - 94.5px));
}
</style>
