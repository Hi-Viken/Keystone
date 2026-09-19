<template>
  <div class="app-container">
    <!-- 搜索栏 -->
    <el-form :model="queryParams" ref="queryForm" :inline="true" v-show="showSearch">
      <el-form-item label="编码" prop="code">
        <el-input
          v-model="queryParams.code"
          placeholder="请输入编码"
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
          v-hasPermi="['kep:materialtype:add']"
        >新增</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button type="info" plain icon="sort" @click="toggleExpandAll">展开/折叠</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="danger"
          plain
          icon="delete"
          :disabled="multiple"
          @click="handleDelete"
          v-hasPermi="['kep:materialtype:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="download"
          @click="handleExport"
          v-hasPermi="['kep:materialtype:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar v-model:showSearch="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <!-- 数据表格（树形） -->
    <el-table
      v-if="refreshTable"
      v-loading="loading"
      :data="materialTypeList"
      row-key="id"
      :default-expand-all="isExpandAll"
      :tree-props="{ children: 'children', hasChildren: 'hasChildren' }"
      @selection-change="handleSelectionChange"
    >
      <el-table-column type="selection" width="50" align="center" />
      <el-table-column label="名称" align="left" prop="name" min-width="200" />
      <el-table-column label="编码" align="center" prop="code" width="120" />
      <el-table-column label="描述" align="center" prop="description" min-width="200" show-overflow-tooltip />
      <el-table-column label="创建人" align="center" width="160">
        <template #default="scope">
          <span>{{ scope.row.createByNickName || scope.row.createBy }}</span>
        </template>
      </el-table-column>
      <el-table-column label="创建时间" align="center" prop="create_time" width="160" />
      <el-table-column label="修改人" align="center" width="160">
        <template #default="scope">
          <span>{{ scope.row.updateByNickName || scope.row.updateBy }}</span>
        </template>
      </el-table-column>
      <el-table-column label="修改时间" align="center" prop="update_time" width="160" />
      <el-table-column label="操作" align="center" width="200" fixed="right">
        <template #default="scope">
          <el-button
            text
            size="small"
            icon="edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['kep:materialtype:edit']"
          >编辑</el-button>
          <el-button
            text
            size="small"
            icon="plus"
            @click="handleAdd(scope.row)"
            v-hasPermi="['kep:materialtype:add']"
          >新增</el-button>
          <el-button
            text
            size="small"
            icon="delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['kep:materialtype:remove']"
          >删除</el-button>
        </template>
      </el-table-column>
    </el-table>

    <!-- 添加/修改对话框 -->
    <el-dialog :title="title" v-model="open" width="600px" append-to-body>
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="上级类型" prop="parentId">
          <el-cascader
            class="w100"
            :options="typeOptions"
            :props="{ checkStrictly: true, value: 'id', label: 'name', emitPath: false }"
            placeholder="请选择上级类型（不选则为顶级）"
            clearable
            v-model="form.parentId">
            <template #default="{ node, data }">
              <span>{{ data.name }}</span>
              <span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
            </template>
          </el-cascader>
        </el-form-item>
        <el-form-item label="编码" prop="code" v-if="form.id">
          <el-input v-model="form.code" disabled placeholder="系统自动生成" />
        </el-form-item>
        <el-form-item label="名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入名称" />
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
  </div>
</template>

<script setup name="materialType">
import {
  treeListMaterialType,
  getMaterialType,
  delMaterialType,
  addMaterialType,
  updateMaterialType,
  listMaterialTypeExcludeChild,
  exportMaterialType
} from '@/api/Kep/MaterialType'

const { proxy } = getCurrentInstance()
const loading = ref(true)
const ids = ref([])
const single = ref(true)
const multiple = ref(true)
const showSearch = ref(true)
const materialTypeList = ref([])
const typeOptions = ref([])
const title = ref('')
const open = ref(false)
const isExpandAll = ref(false)
const refreshTable = ref(true)

// 查询参数
let queryParams = reactive({
  code: undefined,
  name: undefined
})

// 表单校验
const state = reactive({
  form: {},
  rules: {
    name: [{ required: true, message: '名称不能为空', trigger: 'blur' }]
  }
})
const formRef = ref(null)
const { form, rules } = toRefs(state)

// 查询列表
function getList() {
  loading.value = true
  treeListMaterialType(queryParams).then((response) => {
    materialTypeList.value = response.data
    loading.value = false
  })
}

// 搜索
function handleQuery() {
  getList()
}

// 重置
function resetQuery() {
  proxy.resetForm('queryForm')
  handleQuery()
}

// 展开/折叠
function toggleExpandAll() {
  refreshTable.value = false
  isExpandAll.value = !isExpandAll.value
  nextTick(() => {
    refreshTable.value = true
  })
}

// 多选
function handleSelectionChange(selection) {
  ids.value = selection.map((item) => item.id)
  single.value = selection.length != 1
  multiple.value = !selection.length
}

// 新增（支持从行上新增子节点）
function handleAdd(row) {
  reset()
  treeListMaterialType().then((response) => {
    typeOptions.value = response.data
  })
  if (row != undefined && row.id) {
    form.value.parentId = row.id
  }
  open.value = true
  title.value = '添加材料类型'
}

// 修改
function handleUpdate(row) {
  reset()
  treeListMaterialType().then((response) => {
    typeOptions.value = response.data
  })
  const id = row.id || ids.value
  getMaterialType(id).then((response) => {
    form.value = response.data
    open.value = true
    title.value = '修改材料类型'
  })
}

// 提交
function submitForm() {
  proxy.$refs['formRef'].validate((valid) => {
    if (valid) {
      if (form.value.id != undefined) {
        updateMaterialType(form.value).then((response) => {
          proxy.$modal.msgSuccess('修改成功')
          open.value = false
          getList()
        })
      } else {
        addMaterialType(form.value).then((response) => {
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
  proxy.$confirm('是否确认删除材料类型"' + row.name + '"?', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(function () {
    return delMaterialType(row.id)
  }).then(() => {
    getList()
    proxy.$modal.msgSuccess('删除成功')
  })
}

// 导出
function handleExport() {
  proxy.$confirm('是否确认导出所有材料类型数据项?', '警告', {
    confirmButtonText: '确定',
    cancelButtonText: '取消',
    type: 'warning'
  }).then(async () => {
    await exportMaterialType(queryParams)
  })
}

// 重置表单
function reset() {
  form.value = {
    id: undefined,
    parentId: 0,
    code: undefined,
    name: undefined,
    description: undefined
  }
  proxy.resetForm('formRef')
}

// 取消
function cancel() {
  open.value = false
  reset()
}

// 初始化
handleQuery()
</script>
