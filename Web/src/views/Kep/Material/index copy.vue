<template>
  <div class="app-container">
    <!-- 搜索栏 -->
    <el-form :model="queryParams" ref="queryForm" :inline="true" v-show="showSearch">
      <el-form-item label="编号" prop="code">
        <el-input
          v-model="queryParams.code"
          placeholder="请输入编号"
          @keyup.enter="handleQuery"
          clearable
        />
      </el-form-item>
      <el-form-item label="名称" prop="name">
        <el-input
          v-model="queryParams.name"
          placeholder="请输入名称"
          @keyup.enter="handleQuery"
          clearable
        />
      </el-form-item>
      <el-form-item label="类型" prop="materialTypeId">
        <el-cascader
          v-model="queryParams.materialTypeId"
          :options="materialTypeOptions"
          :props="{ checkStrictly: true, value: 'id', label: 'name', emitPath: false }"
          placeholder="请选择类型"
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
          v-hasPermi="['kep:material:add']"
        >新增</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="danger"
          plain
          icon="delete"
          :disabled="multiple"
          @click="handleDelete"
          v-hasPermi="['kep:material:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="download"
          @click="handleExport"
          v-hasPermi="['kep:material:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar v-model:showSearch="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <!-- 数据表格 -->
    <el-table
      v-loading="loading"
      :data="materialList"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" width="50" align="center" />
      <el-table-column label="编号" align="center" prop="code" width="120" />
      <el-table-column label="名称" align="left" prop="name" min-width="200" />
      <el-table-column label="类型" align="center" prop="materialTypeName" width="150" />
      <el-table-column label="创建时间" align="center" prop="create_time" width="160" />
      <el-table-column label="操作" align="center" width="200" fixed="right">
        <template #default="scope">
          <el-button
            text
            size="small"
            icon="view"
            @click="handleView(scope.row)"
          >查看</el-button>
          <el-button
            text
            size="small"
            icon="edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['kep:material:edit']"
          >编辑</el-button>
          <el-button
            text
            size="small"
            icon="delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['kep:material:remove']"
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
    <el-dialog :title="title" v-model="open" width="900px" append-to-body>
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="编号" prop="code" v-if="form.id">
          <el-input v-model="form.code" disabled placeholder="系统自动生成" />
        </el-form-item>
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入材料名称" />
        </el-form-item>
        <el-form-item label="类型" prop="materialTypeId">
          <el-cascader
            class="w100"
            v-model="form.materialTypeId"
            :options="materialTypeOptions"
            :props="{ checkStrictly: true, value: 'id', label: 'name', emitPath: false }"
            placeholder="请选择材料类型"
            clearable
          />
        </el-form-item>
        <el-form-item label="详细描述" prop="content">
          <div class="quill-editor-wrapper">
            <quill-editor
              ref="quillEditorRef"
              v-model:content="form.content"
              content-type="html"
              theme="snow"
              :options="editorOptions"
              style="height: 300px"
            />
          </div>
        </el-form-item>
        <el-form-item label="关联文件" prop="fileIds">
          <div class="file-upload-wrapper">
            <el-upload
              ref="uploadRef"
              :action="uploadUrl"
              :headers="uploadHeaders"
              :data="uploadData"
              :file-list="fileList"
              :on-success="handleUploadSuccess"
              :on-remove="handleFileRemove"
              :before-upload="beforeUpload"
              multiple
              :auto-upload="true"
            >
              <el-button type="primary" icon="upload">上传文件</el-button>
              <template #tip>
                <div class="el-upload__tip">支持上传多个文件</div>
              </template>
            </el-upload>
          </div>
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button text @click="cancel">取消</el-button>
        <el-button type="primary" @click="submitForm">提交</el-button>
      </template>
    </el-dialog>

    <!-- 查看详情对话框 -->
    <el-dialog title="材料详情" v-model="viewOpen" width="900px" append-to-body>
      <el-descriptions :column="2" border>
        <el-descriptions-item label="编号">{{ viewForm.code }}</el-descriptions-item>
        <el-descriptions-item label="名称">{{ viewForm.name }}</el-descriptions-item>
        <el-descriptions-item label="类型">{{ viewForm.materialTypeName }}</el-descriptions-item>
        <el-descriptions-item label="创建时间">{{ viewForm.create_time }}</el-descriptions-item>
      </el-descriptions>
      <el-divider content-position="left">详细描述</el-divider>
      <div class="content-preview" v-html="viewForm.content"></div>
      <el-divider content-position="left">关联文件</el-divider>
      <el-table :data="viewForm.fileList" v-if="viewForm.fileList && viewForm.fileList.length > 0">
        <el-table-column label="文件名" align="left" prop="fileName" min-width="200" />
        <el-table-column label="大小" align="center" prop="fileSize" width="120">
          <template #default="scope">
            {{ formatFileSize(scope.row.fileSize) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" align="center" width="100">
          <template #default="scope">
            <el-button type="text" size="small" @click="handleDownload(scope.row)">下载</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-empty v-else description="暂无关联文件" />
    </el-dialog>
  </div>
</template>

<script setup name="material">
import { ref, reactive, getCurrentInstance, onMounted } from 'vue'
import { QuillEditor } from '@vueup/vue-quill'
import '@vueup/vue-quill/dist/vue-quill.snow.css'
import {
  listMaterial,
  getMaterial,
  addMaterial,
  updateMaterial,
  delMaterial,
  exportMaterial
} from '@/api/Kep/Material'
import { treeListMaterialType } from '@/api/Kep/MaterialType'
import { uploadFiles, downloadFile } from '@/api/Kep/FileManagement'
import { getToken } from '@/utils/auth'

const { proxy } = getCurrentInstance()
const loading = ref(true)
const ids = ref([])
const single = ref(true)
const multiple = ref(true)
const showSearch = ref(true)
const materialList = ref([])
const materialTypeOptions = ref([])
const title = ref('')
const open = ref(false)
const total = ref(0)
const viewOpen = ref(false)
const viewForm = ref({})

// 查询参数
let queryParams = reactive({
  pageNum: 1,
  pageSize: 10,
  code: undefined,
  name: undefined,
  materialTypeId: undefined
})

// 表单校验
const state = reactive({
  form: {},
  rules: {
    name: [{ required: true, message: '名称不能为空', trigger: 'blur' }],
    materialTypeId: [{ required: true, message: '类型不能为空', trigger: 'change' }]
  }
})
const formRef = ref(null)
const { form, rules } = toRefs(state)

// 富文本编辑器配置
const quillEditorRef = ref(null)
const editorOptions = {
  modules: {
    toolbar: [
      ['bold', 'italic', 'underline', 'strike'],
      ['blockquote', 'code-block'],
      [{ header: 1 }, { header: 2 }],
      [{ list: 'ordered' }, { list: 'bullet' }],
      [{ script: 'sub' }, { script: 'super' }],
      [{ indent: '-1' }, { indent: '+1' }],
      [{ size: ['small', false, 'large', 'huge'] }],
      [{ header: [1, 2, 3, 4, 5, 6, false] }],
      [{ color: [] }, { background: [] }],
      [{ font: [] }],
      [{ align: [] }],
      ['link', 'image', 'video'],
      ['clean']
    ]
  },
  placeholder: '请输入材料详细描述...'
}

// 文件上传配置
const uploadUrl = ref('/FileManagement/UploadFiles')
const uploadHeaders = ref({ Authorization: 'Bearer ' + getToken() })
const uploadData = ref({ parentId: 0 })
const fileList = ref([])

// 查询列表
function getList() {
  loading.value = true
  listMaterial(queryParams).then((response) => {
    materialList.value = response.data.result
    total.value = response.data.totalNum
    loading.value = false
  })
}

// 加载材料类型选项
function loadMaterialTypeOptions() {
  treeListMaterialType().then((response) => {
    materialTypeOptions.value = response.data
  })
}

// 搜索
function handleQuery() {
  queryParams.pageNum = 1
  getList()
}

// 重置
function resetQuery() {
  proxy.resetForm('queryForm')
  queryParams.materialTypeId = undefined
  handleQuery()
}

// 多选
function handleSelectionChange(selection) {
  ids.value = selection.map((item) => item.id)
  single.value = selection.length != 1
  multiple.value = !selection.length
}

// 新增
function handleAdd() {
  reset()
  loadMaterialTypeOptions()
  uploadData.value.parentId = 0
  fileList.value = []
  open.value = true
  title.value = '添加材料'
}

// 修改
function handleUpdate(row) {
  reset()
  loadMaterialTypeOptions()
  const id = row.id || ids.value
  getMaterial(id).then((response) => {
    form.value = response.data
    // 转换关联文件列表为el-upload格式
    if (form.value.fileList) {
      fileList.value = form.value.fileList.map(file => ({
        name: file.fileName,
        url: file.filePath,
        id: file.id
      }))
      uploadData.value.parentId = form.value.id
    }
    open.value = true
    title.value = '修改材料'
  })
}

// 查看详情
function handleView(row) {
  getMaterial(row.id).then((response) => {
    viewForm.value = response.data
    viewOpen.value = true
  })
}

// 提交
function submitForm() {
  proxy.$refs['formRef'].validate((valid) => {
    if (valid) {
      // 收集已上传的文件ID
      form.value.fileIds = fileList.value
        .filter(file => file.id)
        .map(file => file.id)

      if (form.value.id != undefined) {
        updateMaterial(form.value).then((response) => {
          proxy.$modal.msgSuccess('修改成功')
          open.value = false
          getList()
        })
      } else {
        addMaterial(form.value).then((response) => {
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
  const materialIds = row.id || ids.value
  proxy.$confirm('是否确认删除材料"' + (row.name || '选中项') + '"?', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(function () {
    return delMaterial(materialIds)
  }).then(() => {
    getList()
    proxy.$modal.msgSuccess('删除成功')
  })
}

// 导出
function handleExport() {
  proxy.$confirm('是否确认导出所有材料数据项?', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    await exportMaterial(queryParams)
  })
}

// 上传前校验
function beforeUpload(file) {
  const isLt10M = file.size / 1024 / 1024 < 10
  if (!isLt10M) {
    proxy.$modal.msgError('文件大小不能超过 10MB!')
  }
  return isLt10M
}

// 上传成功回调
function handleUploadSuccess(response, file, fileListData) {
  if (response.code === 200) {
    // 上传成功后，将返回的文件ID添加到列表
    file.id = response.data
    proxy.$modal.msgSuccess('上传成功')
  } else {
    proxy.$modal.msgError(response.msg || '上传失败')
  }
}

// 移除文件回调
function handleFileRemove(file, fileListData) {
  // 文件移除时可以从服务器删除（这里简化处理，只从列表移除）
}

// 下载文件
function handleDownload(row) {
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
    code: undefined,
    name: undefined,
    materialTypeId: undefined,
    content: undefined,
    fileIds: []
  }
  fileList.value = []
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
loadMaterialTypeOptions()
getList()
</script>

<style scoped>
.quill-editor-wrapper {
  width: 100%;
}

.quill-editor-wrapper :deep(.ql-container) {
  min-height: 200px;
}

.content-preview {
  max-height: 400px;
  overflow-y: auto;
  padding: 16px;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 4px;
  background-color: var(--el-fill-color-lighter);
}

.content-preview img {
  max-width: 100%;
  height: auto;
}

.file-upload-wrapper {
  width: 100%;
}
</style>
