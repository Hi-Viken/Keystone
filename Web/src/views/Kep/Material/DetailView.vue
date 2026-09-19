<template>
  <div class="detail-container">
    <!-- 顶部操作栏 -->
    <div class="detail-header">
      <div class="header-left">
        <el-button icon="arrow-left" @click="goBack">返回</el-button>
        <el-divider direction="vertical" />
        <span class="material-title">{{ material.name }}</span>
        <el-tag type="info" size="small">{{ material.code }}</el-tag>
      </div>
      <div class="header-right">
        <el-button
          type="primary"
          icon="edit"
          @click="handleEdit"
          v-hasPermi="['kep:material:edit']"
        >编辑</el-button>
        <el-button
          type="danger"
          icon="delete"
          @click="handleDelete"
          v-hasPermi="['kep:material:remove']"
        >删除</el-button>
      </div>
    </div>

    <!-- 主体内容 -->
    <div class="detail-body" v-loading="loading">
      <el-row :gutter="20">
        <!-- 左侧：基本信息 -->
        <el-col :span="6">
          <!-- 封面图片 -->
          <el-card class="cover-card">
            <div class="cover-wrapper">
              <el-image
                v-if="material.coverImage"
                :src="baseUrl + material.coverImage"
                :preview-src-list="[baseUrl + material.coverImage]"
                fit="cover"
                class="cover-image-full"
                preview-teleported
              />
              <div v-else class="cover-placeholder">
                <el-icon :size="60" color="#c0c4cc"><picture /></el-icon>
                <span>暂无封面</span>
              </div>
            </div>
          </el-card>

          <el-card class="info-card mt20">
            <template #header>
              <div class="card-header">
                <span>基本信息</span>
              </div>
            </template>
            <el-descriptions :column="1" border>
              <el-descriptions-item label="编号">{{ material.code }}</el-descriptions-item>
              <el-descriptions-item label="名称">{{ material.name }}</el-descriptions-item>
              <el-descriptions-item label="类型">
                <el-tag v-if="material.materialTypeName">{{ material.materialTypeName }}</el-tag>
                <span v-else class="text-muted">未分类</span>
              </el-descriptions-item>
              <el-descriptions-item label="创建人">{{ material.create_by || '-' }}</el-descriptions-item>
              <el-descriptions-item label="创建时间">{{ material.create_time || '-' }}</el-descriptions-item>
              <el-descriptions-item label="更新人">{{ material.update_by || '-' }}</el-descriptions-item>
              <el-descriptions-item label="更新时间">{{ material.update_time || '-' }}</el-descriptions-item>
            </el-descriptions>
          </el-card>

          <!-- 关联文件 -->
          <el-card class="file-card mt20">
            <template #header>
              <div class="card-header">
                <span>关联文件</span>
                <el-tag size="small" type="info">{{ material.fileList ? material.fileList.length : 0 }}</el-tag>
              </div>
            </template>
            <div v-if="material.fileList && material.fileList.length > 0">
              <div
                v-for="file in material.fileList"
                :key="file.id"
                class="file-item"
                @click="handleDownload(file)"
              >
                <el-icon class="file-icon"><document /></el-icon>
                <div class="file-info">
                  <div class="file-name" :title="file.fileName">{{ file.fileName }}</div>
                  <div class="file-size">{{ formatFileSize(file.fileSize) }}</div>
                </div>
              </div>
            </div>
            <el-empty v-else description="暂无关联文件" :image-size="60" />
          </el-card>
        </el-col>

        <!-- 右侧：富文本内容 -->
        <el-col :span="18">
          <el-card class="content-card">
            <template #header>
              <div class="card-header">
                <span>详细描述</span>
              </div>
            </template>
            <div class="content-preview" v-html="material.content"></div>
            <el-empty
              v-if="!material.content"
              description="暂无详细描述"
              :image-size="100"
            />
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup name="materialDetail">
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getMaterial, delMaterial } from '@/api/Kep/Material'
import { downloadFile } from '@/api/Kep/FileManagement'
import { ElMessageBox } from 'element-plus'

const route = useRoute()
const router = useRouter()
const baseUrl = import.meta.env.VITE_APP_BASE_API
const loading = ref(true)
const material = ref({})

// 加载详情
function loadDetail() {
  const id = route.params.id || route.query.id
  if (!id) {
    ElMessageBox.alert('缺少材料ID参数', '错误', { type: 'error' })
    return
  }

  loading.value = true
  getMaterial(id).then((response) => {
    material.value = response.data || {}
    loading.value = false
  }).catch(() => {
    loading.value = false
  })
}

// 返回
function goBack() {
  router.back()
}

// 编辑
function handleEdit() {
  const id = material.value.id
  router.push({ path: '/kep/material', query: { editId: id } })
}

// 删除
function handleDelete() {
  ElMessageBox.confirm('是否确认删除材料"' + material.value.name + '"?', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(() => {
    return delMaterial(material.value.id)
  }).then(() => {
    ElMessageBox.alert('删除成功', '提示', { type: 'success' })
    goBack()
  })
}

// 下载文件
function handleDownload(file) {
  const url = baseUrl + '/FileManagement/Download/' + file.id
  window.open(url, '_blank')
}

// 格式化文件大小
function formatFileSize(bytes) {
  if (!bytes || bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return (bytes / Math.pow(k, i)).toFixed(2) + ' ' + sizes[i]
}

onMounted(() => {
  loadDetail()
})
</script>

<style scoped>
.detail-container {
  height: 100vh;
  display: flex;
  flex-direction: column;
  background-color: var(--el-bg-color-page);
  overflow: hidden;
}

.detail-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 24px;
  background-color: var(--el-bg-color);
  border-bottom: 1px solid var(--el-border-color-lighter);
  flex-shrink: 0;
}

.header-left {
  display: flex;
  align-items: center;
  gap: 12px;
}

.material-title {
  font-size: 18px;
  font-weight: 600;
  color: var(--el-text-color-primary);
}

.header-right {
  display: flex;
  gap: 8px;
}

.detail-body {
  flex: 1;
  padding: 20px 24px;
  overflow-y: auto;
}

.card-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  font-weight: 600;
}

.info-card :deep(.el-descriptions__label) {
  width: 80px;
  font-weight: 500;
}

.cover-card {
  overflow: hidden;
}

.cover-wrapper {
  width: 100%;
  aspect-ratio: 16 / 9;
  background-color: var(--el-fill-color-lighter);
  border-radius: 6px;
  overflow: hidden;
}

.cover-image-full {
  width: 100%;
  height: 100%;
}

.cover-placeholder {
  width: 100%;
  height: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  gap: 10px;
  color: var(--el-text-color-secondary);
  font-size: 14px;
}

.file-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 10px 12px;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.file-item:hover {
  background-color: var(--el-fill-color-light);
}

.file-icon {
  font-size: 24px;
  color: var(--el-color-primary);
  flex-shrink: 0;
}

.file-info {
  flex: 1;
  min-width: 0;
}

.file-name {
  font-size: 14px;
  color: var(--el-text-color-primary);
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.file-size {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  margin-top: 2px;
}

.content-card {
  min-height: calc(100vh - 160px);
}

.content-preview {
  line-height: 1.8;
  font-size: 15px;
  color: var(--el-text-color-primary);
  word-wrap: break-word;
}

.content-preview :deep(img) {
  max-width: 100%;
  height: auto;
  border-radius: 4px;
  margin: 8px 0;
}

.content-preview :deep(video) {
  max-width: 100%;
  border-radius: 4px;
  margin: 8px 0;
}

.content-preview :deep(h1),
.content-preview :deep(h2),
.content-preview :deep(h3),
.content-preview :deep(h4),
.content-preview :deep(h5),
.content-preview :deep(h6) {
  margin: 16px 0 8px;
  font-weight: 600;
}

.content-preview :deep(p) {
  margin: 8px 0;
}

.content-preview :deep(ul),
.content-preview :deep(ol) {
  padding-left: 24px;
  margin: 8px 0;
}

.content-preview :deep(blockquote) {
  margin: 12px 0;
  padding: 8px 16px;
  border-left: 4px solid var(--el-color-primary);
  background-color: var(--el-fill-color-lighter);
  border-radius: 0 4px 4px 0;
}

.content-preview :deep(pre) {
  background-color: var(--el-fill-color-darker);
  padding: 12px;
  border-radius: 4px;
  overflow-x: auto;
  margin: 8px 0;
}

.content-preview :deep(table) {
  border-collapse: collapse;
  width: 100%;
  margin: 8px 0;
}

.content-preview :deep(th),
.content-preview :deep(td) {
  border: 1px solid var(--el-border-color);
  padding: 8px 12px;
  text-align: left;
}

.content-preview :deep(th) {
  background-color: var(--el-fill-color-light);
  font-weight: 600;
}

.text-muted {
  color: var(--el-text-color-secondary);
}

.mt20 {
  margin-top: 20px;
}
</style>
