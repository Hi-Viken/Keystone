<template>
  <div class="app-container">
    <!-- 面包屑导航 -->
    <div class="breadcrumb-wrapper mb16">
      <div class="breadcrumb-item" :class="{ active: currentParentId === 0 }" @click="navigateTo(0)">
        <el-icon><HomeFilled /></el-icon>
        <span>根目录</span>
      </div>
      <template v-for="(item, index) in breadcrumbPath" :key="item.id">
        <div class="breadcrumb-separator">
          <el-icon><ArrowRight /></el-icon>
        </div>
        <div
          class="breadcrumb-item"
          :class="{ active: index === breadcrumbPath.length - 1 }"
          @click="navigateTo(item.id)"
        >
          <el-icon><FolderOpened /></el-icon>
          <span>{{ item.fileName }}</span>
        </div>
      </template>
    </div>

    <!-- 搜索栏 -->
    <el-form :model="queryParams" ref="queryForm" :inline="true" v-show="showSearch">
      <el-form-item label="名称" prop="fileName">
        <el-input
          v-model="queryParams.fileName"
          placeholder="请输入名称"
          @keyup.enter="handleQuery"
          clearable
        />
      </el-form-item>
      <el-form-item label="类型" prop="fileType">
        <el-select v-model="queryParams.fileType" placeholder="请选择类型" clearable>
          <el-option label="目录" value="0" />
          <el-option label="文件" value="1" />
        </el-select>
      </el-form-item>
      <el-form-item label="扩展名" prop="fileExtension">
        <el-input
          v-model="queryParams.fileExtension"
          placeholder="请输入扩展名"
          @keyup.enter="handleQuery"
          clearable
        />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" icon="search" @click="handleQuery">搜索</el-button>
        <el-button icon="refresh" @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <!-- 工具栏 -->
    <el-row :gutter="10" class="mb8">
      <el-col :span="1.5">
        <el-button
          type="primary"
          plain
          icon="plus"
          @click="handleAdd"
          v-hasPermi="['kep:filemanagement:add']"
        >新增</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="success"
          plain
          icon="upload"
          @click="openUploadDialog"
          v-hasPermi="['kep:filemanagement:add']"
        >上传文件</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="danger"
          plain
          icon="delete"
          :disabled="multiple"
          @click="handleDelete"
          v-hasPermi="['kep:filemanagement:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="download"
          @click="handleExport"
          v-hasPermi="['kep:filemanagement:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar v-model:showSearch="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <!-- 数据表格（分页） -->
    <el-table
      v-loading="loading"
      :data="fileManagementList"
      @selection-change="handleSelectionChange"
      @row-dblclick="handleRowDblClick"
      highlight-current-row
    >
      <el-table-column type="selection" width="50" align="center" />
      <el-table-column label="名称" align="left" prop="fileName" min-width="200">
        <template #default="scope">
          <el-icon v-if="scope.row.fileType === '0'" class="file-icon folder-icon"><folder /></el-icon>
          <el-icon v-else class="file-icon file-doc-icon"><document /></el-icon>
          <span>{{ scope.row.fileName }}</span>
        </template>
      </el-table-column>
      <el-table-column label="类型" align="center" prop="fileType" width="100">
        <template #default="scope">
          <div  class="flex gap-2">
    <el-tag  v-if="scope.row.fileType === '0'"  type="primary" >目录</el-tag>
    <el-tag  v-else type="success">文件</el-tag>
   
  </div>

          <!-- <el-tag v-if="scope.row.fileType === '0'" type="success">目录</el-tag>
          <el-tag v-else type="info">文件</el-tag> -->
          <!-- <el-tag v-if="scope.row.fileType === '0'" type="success">目录</el-tag>
          <el-tag v-else type="info">文件</el-tag> -->
        </template>
      </el-table-column>
      <el-table-column label="扩展名" align="center" prop="fileExtension" width="100" />
      <el-table-column label="大小" align="center" prop="fileSize" width="120">
        <template #default="scope">
          <span v-if="scope.row.fileSize">{{ formatFileSize(scope.row.fileSize) }}</span>
          <span v-else>-</span>
        </template>
      </el-table-column>
      <el-table-column label="描述" align="center" prop="description" min-width="150" show-overflow-tooltip />
      <el-table-column label="创建人" align="center" prop="createBy" width="160" />
      <el-table-column label="创建时间" align="center" prop="createTime" width="160" />
      <el-table-column label="操作" align="center" width="200" fixed="right">
        <template #default="scope">
          <el-button
            text
            size="small"
            icon="edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['kep:filemanagement:edit']"
          >编辑</el-button>
          <el-button
            text
            size="small"
            icon="download"
            @click="handleDownload(scope.row)"
            v-if="scope.row.fileType === '1'"
          >下载</el-button>
          <el-button
            text
            size="small"
            icon="delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['kep:filemanagement:remove']"
          >删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 分页 -->
    <pagination
      v-show="total > 0"
      :total="total"
      v-model:page="queryParams.pageNum"
      v-model:limit="queryParams.pageSize"
      @pagination="getList"
    />

    <!-- 添加/修改对话框 -->
    <el-dialog :title="title" v-model="open" width="600px" append-to-body>
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="上级目录" prop="parentId">
          <el-cascader
            class="w100"
            :options="typeOptions"
            :props="{ checkStrictly: true, value: 'id', label: 'fileName', emitPath: false }"
            placeholder="请选择上级目录（不选则为当前目录）"
            clearable
            v-model="form.parentId">
            <template #default="{ node, data }">
              <span>{{ data.fileName }}</span>
              <span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
            </template>
          </el-cascader>
        </el-form-item>
        <el-form-item label="类型" prop="fileType">
          <el-radio-group v-model="form.fileType">
            <el-radio label="0">目录</el-radio>
            <el-radio label="1">文件</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="名称" prop="fileName">
          <el-input v-model="form.fileName" placeholder="请输入名称" />
        </el-form-item>
        <el-form-item label="文件路径" prop="filePath" v-if="form.fileType === '1'">
          <el-input v-model="form.filePath" placeholder="请输入文件路径" />
        </el-form-item>
        <el-form-item label="文件大小" prop="fileSize" v-if="form.fileType === '1'">
          <el-input-number v-model="form.fileSize" :min="0" placeholder="请输入文件大小（字节）" />
        </el-form-item>
        <el-form-item label="扩展名" prop="fileExtension" v-if="form.fileType === '1'">
          <el-input v-model="form.fileExtension" placeholder="请输入扩展名（如：pdf、docx）" />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input v-model="form.description" type="textarea" :rows="3" placeholder="请输入描述" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button text @click="cancel">取消</el-button>
        <el-button type="primary" @click="submitForm">提交</el-button>
      </template>
    </el-dialog>

    <!-- 上传文件对话框 -->
    <el-dialog title="上传文件" v-model="uploadDialogVisible" width="700px" append-to-body>
      <el-upload
        ref="uploadRef"
        drag
        multiple
        :auto-upload="false"
        :on-change="handleFileChange"
        :on-remove="handleFileRemove"
        :file-list="uploadFileList"
        :show-file-list="false"
      >
        <el-icon class="el-icon--upload" :size="40"><upload-filled /></el-icon>
        <div class="el-upload__text">
          将文件拖到此处，或<em>点击选择文件</em>
        </div>
        <template #tip>
          <div class="el-upload__tip">
            文件将上传到当前目录：{{ currentParentId === 0 ? '根目录' : '当前目录' }}
          </div>
        </template>
      </el-upload>

      <!-- 文件列表 -->
      <div v-if="uploadFileList.length > 0" class="mt16">
        <el-divider content-position="left">待上传文件列表</el-divider>
        <el-table :data="uploadFileList" max-height="300">
          <el-table-column label="文件名" prop="name" min-width="200" />
          <el-table-column label="大小" prop="size" width="120">
            <template #default="scope">
              {{ formatFileSize(scope.row.size) }}
            </template>
          </el-table-column>
          <el-table-column label="操作" width="100" align="center">
            <template #default="scope">
              <el-button type="danger" size="small" @click="removeFile(scope.$index)">删除</el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>

      <template #footer>
        <el-button @click="uploadDialogVisible = false">取消</el-button>
        <el-button type="primary" @click="submitUpload" :loading="uploading" :disabled="uploadFileList.length === 0">
          上传文件
        </el-button>
      </template>
    </el-dialog>

    <!-- 全局拖拽遮罩层 -->
    <div v-if="isDragover" class="drag-overlay" @dragleave="handleDragLeave" @dragover.prevent @drop.prevent="handleDrop">
      <div class="drag-overlay-content">
        <el-icon :size="60" color="#409eff"><upload-filled /></el-icon>
        <p>释放鼠标上传文件到当前目录</p>
      </div>
    </div>
  </div>
</template>

<script setup name="fileManagement">
import {
  listFileManagementByParentId,
  getFileManagement,
  delFileManagement,
  addFileManagement,
  updateFileManagement,
  exportFileManagement,
  uploadFile,
  uploadFiles,
  downloadFile,
  getBreadcrumbPath,
  treeListFileManagement
} from '@/api/Kep/FileManagement'

const { proxy } = getCurrentInstance()
const loading = ref(true)
const ids = ref([])
const single = ref(true)
const multiple = ref(true)
const showSearch = ref(true)
const fileManagementList = ref([])
const typeOptions = ref([])
const title = ref('')
const open = ref(false)
const total = ref(0)
const currentParentId = ref(0) // 当前目录ID
const breadcrumbPath = ref([]) // 面包屑路径
const isDragover = ref(false) // 拖拽状态
const uploadDialogVisible = ref(false) // 上传对话框
const uploadFileList = ref([]) // 待上传文件列表
const uploading = ref(false) // 上传中状态

// 查询参数
let queryParams = reactive({
  pageNum: 1,
  pageSize: 10,
  parentId: 0,
  fileType: undefined,
  fileName: undefined,
  fileExtension: undefined
})

// 表单校验
const state = reactive({
  form: {},
  rules: {
    fileType: [{ required: true, message: '类型不能为空', trigger: 'change' }],
    fileName: [{ required: true, message: '名称不能为空', trigger: 'blur' }]
  }
})
const formRef = ref(null)
const { form, rules } = toRefs(state)

// 查询列表
function getList() {
  loading.value = true
  queryParams.parentId = currentParentId.value
  listFileManagementByParentId(queryParams).then((response) => {
    fileManagementList.value = response.data.result
    total.value = response.data.totalNum
    loading.value = false
  })
}

// 加载面包屑路径
function loadBreadcrumb() {
  if (currentParentId.value === 0) {
    breadcrumbPath.value = []
  } else {
    getBreadcrumbPath(currentParentId.value).then((response) => {
      breadcrumbPath.value = response.data
    })
  }
}

// 导航到指定目录
function navigateTo(id) {
  currentParentId.value = id
  queryParams.pageNum = 1
  loadBreadcrumb()
  getList()
}

// 搜索
function handleQuery() {
  queryParams.pageNum = 1
  getList()
}

// 重置
function resetQuery() {
  proxy.resetForm('queryForm')
  queryParams.fileType = undefined
  queryParams.fileName = undefined
  queryParams.fileExtension = undefined
  handleQuery()
}

// 多选
function handleSelectionChange(selection) {
  ids.value = selection.map((item) => item.id)
  single.value = selection.length != 1
  multiple.value = !selection.length
}

// 双击进入目录
function handleRowDblClick(row) {
  if (row.fileType === '0') {
    navigateTo(row.id)
  }
}

// 新增
function handleAdd() {
  reset()
  treeListFileManagement({ fileType: '0' }).then((response) => {
    typeOptions.value = response.data
  })
  form.value.parentId = currentParentId.value
  open.value = true
  title.value = '添加文件/目录'
}

// 修改
function handleUpdate(row) {
  reset()
  treeListFileManagement({ fileType: '0' }).then((response) => {
    typeOptions.value = response.data
  })
  getFileManagement(row.id).then((response) => {
    form.value = response.data
    open.value = true
    title.value = '修改文件/目录'
  })
}

// 提交
function submitForm() {
  proxy.$refs['formRef'].validate((valid) => {
    if (valid) {
      if (form.value.id != undefined) {
        updateFileManagement(form.value).then((response) => {
          proxy.$modal.msgSuccess('修改成功')
          open.value = false
          getList()
        })
      } else {
        addFileManagement(form.value).then((response) => {
          proxy.$modal.msgSuccess('新增成功')
          open.value = false
          getList()
        })
      }
    }
  })
}

// 删除
function handleDelete(row) {
  const fileIds = row.id || ids.value
  proxy.$confirm('是否确认删除"' + (row.fileName || '选中项') + '"?', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(function () {
    return delFileManagement(fileIds)
  }).then(() => {
    getList()
    proxy.$modal.msgSuccess('删除成功')
  })
}

// 导出
function handleExport() {
  proxy.$confirm('是否确认导出所有文件管理数据项?', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    await exportFileManagement(queryParams)
  })
}

// 打开上传对话框
function openUploadDialog() {
  uploadFileList.value = []
  uploadDialogVisible.value = true
}

// 文件选择时的处理
function handleFileChange(file, fileList) {
  uploadFileList.value = fileList
}

// 从列表中移除文件
function handleFileRemove(file, fileList) {
  uploadFileList.value = fileList
}

// 删除指定索引的文件
function removeFile(index) {
  uploadFileList.value.splice(index, 1)
}

// 提交上传
function submitUpload() {
  if (uploadFileList.value.length === 0) {
    proxy.$modal.msgWarning('请先选择文件')
    return
  }

  uploading.value = true
  const formData = new FormData()
  
  // 将所有文件添加到 FormData
  uploadFileList.value.forEach(file => {
    formData.append('files', file.raw)
  })

  uploadFiles(currentParentId.value, formData)
    .then((response) => {
      proxy.$modal.msgSuccess(`成功上传 ${uploadFileList.value.length} 个文件`)
      uploadDialogVisible.value = false
      uploadFileList.value = []
      getList()
    })
    .catch((error) => {
      proxy.$modal.msgError('上传失败')
    })
    .finally(() => {
      uploading.value = false
    })
}

// 全局拖拽进入
function handleDragEnter(e) {
  e.preventDefault()
  isDragover.value = true
}

// 全局拖拽离开
function handleDragLeave(e) {
  e.preventDefault()
  // 只有当离开整个页面时才隐藏遮罩
  if (e.relatedTarget === null || !document.documentElement.contains(e.relatedTarget)) {
    isDragover.value = false
  }
}

// 拖拽释放
function handleDrop(e) {
  e.preventDefault()
  isDragover.value = false
  
  const files = e.dataTransfer.files
  if (files.length > 0) {
    uploadFileList.value = Array.from(files).map(file => ({
      name: file.name,
      size: file.size,
      raw: file
    }))
    uploadDialogVisible.value = true
  }
}

// 下载文件
function handleDownload(row) {
  if (row.fileType !== '1') {
    proxy.$modal.msgWarning('只能下载文件类型的记录')
    return
  }
  
  downloadFile(row.id).then((response) => {
    const blob = new Blob([response.data])
    const url = window.URL.createObjectURL(blob)
    const link = document.createElement('a')
    link.href = url
    link.download = row.fileName
    link.click()
    window.URL.revokeObjectURL(url)
  }).catch((error) => {
    proxy.$modal.msgError('下载失败')
  })
}

// 重置表单
function reset() {
  proxy.resetForm('formRef')
  form.value = {
    id: undefined,
    parentId: 0,
    fileType: '0',
    fileName: undefined,
    filePath: undefined,
    fileSize: undefined,
    fileExtension: undefined,
    description: undefined
  }
}

// 取消
function cancel() {
  open.value = false
  reset()
}

// 格式化文件大小
function formatFileSize(bytes) {
  if (bytes === 0) return '0 B'
  const k = 1024
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB']
  const i = Math.floor(Math.log(bytes) / Math.log(k))
  return (bytes / Math.pow(k, i)).toFixed(2) + ' ' + sizes[i]
}

// 初始化
loadBreadcrumb()
getList()

// 监听全局拖拽事件
onMounted(() => {
  document.addEventListener('dragenter', handleDragEnter)
  document.addEventListener('dragleave', handleDragLeave)
  document.addEventListener('dragover', (e) => e.preventDefault())
  document.addEventListener('drop', (e) => e.preventDefault())
})

onUnmounted(() => {
  document.removeEventListener('dragenter', handleDragEnter)
  document.removeEventListener('dragleave', handleDragLeave)
})
</script>

<style scoped>
.breadcrumb-wrapper {
  display: flex;
  align-items: center;
  flex-wrap: wrap;
  gap: 4px;
  padding: 10px 16px;
  margin-bottom: 16px;
  background-color: var(--el-fill-color-lighter);
  border-radius: 8px;
  border: 1px solid var(--el-border-color-lighter);
}

.breadcrumb-item {
  display: inline-flex;
  align-items: center;
  gap: 6px;
  padding: 4px 12px;
  border-radius: 6px;
  font-size: 14px;
  color: var(--el-color-primary);
  cursor: pointer;
  transition: all 0.25s ease;
  user-select: none;
}

.breadcrumb-item:hover {
  background-color: var(--el-color-primary-light-9);
  color: var(--el-color-primary-light-3);
}

.breadcrumb-item.active {
  background-color: var(--el-color-primary);
  color: #fff;
  font-weight: 500;
  cursor: default;
}

.breadcrumb-item.active:hover {
  background-color: var(--el-color-primary);
  color: #fff;
}

.breadcrumb-separator {
  display: inline-flex;
  align-items: center;
  color: var(--el-text-color-placeholder);
  font-size: 12px;
}

.drag-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(64, 158, 255, 0.1);
  border: 3px dashed #409eff;
  z-index: 9999;
  display: flex;
  align-items: center;
  justify-content: center;
}

.drag-overlay-content {
  text-align: center;
  color: #409eff;
  font-size: 18px;
  font-weight: bold;
}

.drag-overlay-content p {
  margin-top: 16px;
}

.file-icon {
  margin-right: 5px;
}

.folder-icon {
  color: var(--el-color-primary);
}

.file-doc-icon {
  color: var(--el-color-success);
}
</style>
