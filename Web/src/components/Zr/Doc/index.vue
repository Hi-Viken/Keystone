<template>
  <div>
    <svg-icon name="question" @click="goto" />
  </div>
</template>
<script setup>
const url = ref('http://vkin.cc/doc');

function goto() {
  window.open(url.value)
}
</script>