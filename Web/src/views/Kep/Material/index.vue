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
      <el-table-column label="封面" align="center" width="80">
        <template #default="scope">
          <el-image
            v-if="scope.row.coverImageId"
            :src="getCoverImageUrl(scope.row.coverImageId)"
            :preview-src-list="[getCoverImageUrl(scope.row.coverImageId)]"
            fit="cover"
            style="width: 50px; height: 50px; border-radius: 4px;"
            preview-teleported
          />
          <el-icon v-else :size="30" color="#c0c4cc"><picture /></el-icon>
        </template>
      </el-table-column>
      <el-table-column label="编号" align="center" prop="code" width="120" />
      <el-table-column label="名称" align="left" prop="name" min-width="200" />
      <el-table-column label="类型" align="center" prop="materialTypeName" width="150" />
      <el-table-column label="创建时间" align="center" prop="createTime" width="160" />
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
        <el-form-item label="封面图片" prop="coverImage">
          <el-upload
            class="cover-uploader"
            :auto-upload="false"
            :show-file-list="false"
            :on-change="handleCoverChange"
            :before-upload="beforeCoverUpload"
            accept="image/*"
          >
            <el-image v-if="coverPreview" :src="coverPreview" fit="cover" class="cover-image" />
            <el-icon v-else class="cover-uploader-icon"><plus /></el-icon>
          </el-upload>
          <el-button v-if="coverPreview" link type="danger" @click="removeCover" class="ml10">移除封面</el-button>
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
              :file-list="fileList"
              :on-change="handleFileChange"
              :on-remove="handleFileRemove"
              :before-upload="beforeUpload"
              :auto-upload="false"
              multiple
            >
              <el-button type="primary" icon="upload">选择文件</el-button>
              <template #tip>
                <div class="el-upload__tip">支持上传多个文件，提交时一起上传</div>
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

  </div>
</template>

<script setup name="material">
import { ref, reactive, getCurrentInstance, onMounted } from 'vue'
import { useRouter } from 'vue-router'
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
import { uploadFile, uploadFiles } from '@/api/Kep/FileManagement'
import { getToken } from '@/utils/auth'

const router = useRouter()
const { proxy } = getCurrentInstance()
const baseUrl = import.meta.env.VITE_APP_BASE_API
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
const coverFile = ref(null) // 封面图片文件对象
const coverPreview = ref('') // 封面图片预览URL

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
    // 如果已有封面图片，设置预览
    if (form.value.coverImage) {
      coverPreview.value = baseUrl + form.value.coverImage
    }
    open.value = true
    title.value = '修改材料'
  })
}

// 查看详情
function handleView(row) {
  const routeData = router.resolve('/kep/material/detail/' + row.id)
  window.open(routeData.href, '_blank')
}

// 提交
function submitForm() {
  proxy.$refs['formRef'].validate(async (valid) => {
    if (valid) {
      try {
        // 如果有新的封面图片，先上传
        if (coverFile.value) {
          const formData = new FormData()
          formData.append('file', coverFile.value)

          const uploadResponse = await uploadFile(0, formData)
          if (uploadResponse.code === 200) {
            // 上传成功，获取文件ID
            form.value.coverImageId = uploadResponse.data.id
          } else {
            proxy.$modal.msgError('封面图片上传失败')
            return
          }
        }

        // 收集文件ID：已存在的文件 + 新上传的文件
        const fileIds = []
        
        // 1. 收集已存在的文件ID（编辑时）
        fileList.value
          .filter(file => file.id)
          .forEach(file => fileIds.push(file.id))
        
        // 2. 上传新选择的文件
        const newFiles = fileList.value.filter(file => !file.id && file.raw)
        if (newFiles.length > 0) {
          const formData = new FormData()
          newFiles.forEach(file => {
            formData.append('files', file.raw)
          })

          const uploadResponse = await uploadFiles(0, formData)
          if (uploadResponse.code === 200) {
            // 上传成功，获取文件ID列表
            const uploadedFileIds = uploadResponse.data.fileIds || []
            fileIds.push(...uploadedFileIds)
          } else {
            proxy.$modal.msgError('关联文件上传失败')
            return
          }
        }

        // 设置文件ID列表
        form.value.fileIds = fileIds

        if (form.value.id != undefined) {
          await updateMaterial(form.value)
          proxy.$modal.msgSuccess('修改成功')
          open.value = false
          getList()
        } else {
          await addMaterial(form.value)
          proxy.$modal.msgSuccess('新增成功')
          open.value = false
          getList()
        }
      } catch (error) {
        proxy.$modal.msgError('提交失败')
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

// 文件选择处理
function handleFileChange(file, newFileList) {
  // 校验文件大小
  const isLt10M = file.raw.size / 1024 / 1024 < 10
  if (!isLt10M) {
    proxy.$modal.msgError('文件 ' + file.name + ' 大小不能超过 10MB!')
    // 从列表中移除不符合要求的文件
    fileList.value = newFileList.filter(f => f.uid !== file.uid)
    return
  }
  // 保存文件列表
  fileList.value = newFileList
}

// 移除文件回调
function handleFileRemove(file, newFileList) {
  fileList.value = newFileList
}

// 封面图片选择
function handleCoverChange(file) {
  const isImage = file.raw.type.startsWith('image/')
  const isLt2M = file.raw.size / 1024 / 1024 < 2

  if (!isImage) {
    proxy.$modal.msgError('只能上传图片文件!')
    return
  }
  if (!isLt2M) {
    proxy.$modal.msgError('图片大小不能超过 2MB!')
    return
  }

  coverFile.value = file.raw
  coverPreview.value = URL.createObjectURL(file.raw)
}

// 移除封面图片
function removeCover() {
  coverFile.value = null
  coverPreview.value = ''
}

// 重置表单
function reset() {
  proxy.resetForm('formRef')
  form.value = {
    id: undefined,
    code: undefined,
    name: undefined,
    materialTypeId: undefined,
    coverImage: undefined,
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

// 获取封面图片URL
function getCoverImageUrl(coverImageId) {
  if (!coverImageId) return ''
  return baseUrl + '/FileManagement/Download/' + coverImageId
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

.file-upload-wrapper {
  width: 100%;
}

.cover-uploader {
  display: inline-block;
}

.cover-uploader :deep(.el-upload) {
  border: 1px dashed var(--el-border-color);
  border-radius: 6px;
  cursor: pointer;
  position: relative;
  overflow: hidden;
  transition: var(--el-transition-duration-fast);
}

.cover-uploader :deep(.el-upload:hover) {
  border-color: var(--el-color-primary);
}

.cover-uploader-icon {
  font-size: 28px;
  color: #8c939d;
  width: 120px;
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.cover-image {
  width: 120px;
  height: 120px;
  display: block;
}

.ml10 {
  margin-left: 10px;
}
</style>
