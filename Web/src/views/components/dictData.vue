<template>
  <el-form :model="queryParams" ref="queryForm" :inline="true">
    <el-form-item :label="$t('dict.dictName')" prop="dictType">
      <el-select v-model="queryParams.dictType">
        <el-option v-for="item in typeOptions" :key="item.dictId" :label="item.dictName" :value="item.dictType" />
      </el-select>
    </el-form-item>
    <el-form-item :label="$t('dict.status')" prop="status">
      <el-radio-group v-model="queryParams.status" :placeholder="$t('dict.dataStatus')" @change="handleQuery()">
        <el-radio-button :label="$t('dict.all')" value="" />
        <el-radio-button v-for="dict in statusOptions" :key="dict.dictValue" :label="dict.dictLabel" :value="dict.dictValue" />
      </el-radio-group>
    </el-form-item>
    <el-form-item>
      <el-button type="primary" icon="search" @click="handleQuery">{{ $t('btn.search') }}</el-button>
      <el-button icon="refresh" @click="resetQuery">{{ $t('btn.reset') }}</el-button>
    </el-form-item>
  </el-form>

  <el-row :gutter="10" class="mb8">
    <el-col :span="1.5">
      <el-button type="primary" plain icon="plus" @click="handleAdd" v-hasPermi="['system:dict:add']">{{ $t('dict.addData') }}</el-button>
    </el-col>
  </el-row>
  <el-table :data="dataList" border>
    <!-- <el-table-column type="selection" width="55" align="center" /> -->
    <el-table-column :label="$t('dict.dictCode')" align="center" prop="dictCode" />
    <el-table-column :label="$t('dict.dictLabel')" align="center" prop="dictLabel" width="140">
      <template #default="scope">
        <span v-if="scope.row.listClass == '' || scope.row.listClass == 'default'" :class="scope.row.cssClass">{{ scope.row.dictLabel }}</span>
        <el-tag v-else :type="scope.row.listClass == 'primary' ? '' : scope.row.listClass" :class="scope.row.cssClass"
          >{{ scope.row.dictLabel }}
        </el-tag>
      </template>
    </el-table-column>
    <el-table-column :label="$t('dict.langKey')" align="center" prop="langKey" />
    <el-table-column :label="$t('dict.dictValue')" align="center" prop="dictValue" sortable />
    <el-table-column :label="$t('dict.dictSort')" align="center" prop="dictSort" sortable />
    <el-table-column :label="$t('dict.enabled')" align="center" prop="status" width="90">
      <template #default="scope">
        <!-- <dict-tag :options="statusOptions" :value="scope.row.status" /> -->
        <el-switch
          v-model="scope.row.status"
          active-value="0"
          inactive-value="1"
          :active-text="$t('common.yes')"
          :inactive-text="$t('common.no')"
          inline-prompt
          @click="handleStatusChange(scope.row)"></el-switch>
      </template>
    </el-table-column>
    <el-table-column :label="$t('dict.remark')" align="center" prop="remark" :show-overflow-tooltip="true" />
    <el-table-column :label="$t('dict.operate')" align="center" class-name="small-padding fixed-width" width="90">
      <template #default="scope">
        <div v-if="scope.row.dictCode > 0">
          <el-button text size="default" icon="edit" @click="handleUpdate(scope.row)" v-hasPermi="['system:dict:edit']"></el-button>
          <el-button text size="default" icon="delete" @click="handleDelete(scope.row)" v-hasPermi="['system:dict:remove']"> </el-button>
        </div>
      </template>
    </el-table-column>
  </el-table>
  <pagination :total="total" v-show="total > 0" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />

  <!-- 添加或修改参数配置对话框 -->
  <el-dialog :title="title" v-model="open" draggable width="500px" append-to-body>
    <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
      <el-row :gutter="20">
        <el-col :lg="24">
          <el-form-item :label="$t('dict.dictType')">
            <el-input v-model="form.dictType" :disabled="true" />
          </el-form-item>
        </el-col>

        <el-col :lg="12">
          <el-form-item :label="$t('dict.dictLabel')" prop="dictLabel">
            <el-input v-model="form.dictLabel" :placeholder="$t('dict.inputDictLabel')" />
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('dict.langKey')" prop="langKey">
            <el-input v-model="form.langKey" :placeholder="$t('dict.inputLangKey')" />
          </el-form-item>
        </el-col>
        <el-col :lg="24">
          <el-form-item :label="$t('dict.dataValue')" prop="dictValue">
            <el-input v-model="form.dictValue" :placeholder="$t('dict.inputDataValue')" />
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('dict.cssClass')" prop="cssClass">
            <el-select v-model="form.cssClass" allow-create filterable clearable="">
              <el-option v-for="dict in cssClassOptions" :class="dict.value" :key="dict.value" :label="dict.label" :value="dict.value">
                <span style="float: left" :class="dict.value">{{ dict.label }}</span>
                <span style="float: right">{{ dict.value }}</span>
              </el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('dict.listClass')" prop="listClass">
            <el-select v-model="form.listClass">
              <el-option v-for="item in listClassOptions" :key="item.value" :label="item.label + '(' + item.value + ')'" :value="item.value">
              </el-option>
            </el-select>
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('dict.showSort')" prop="dictSort">
            <el-input-number v-model="form.dictSort" controls-position="right" :min="0" />
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('dict.status')" prop="status">
            <el-radio-group v-model="form.status">
              <el-radio v-for="dict in statusOptions" :key="dict.dictValue" :value="dict.dictValue" :label="dict.dictLabel"> </el-radio>
            </el-radio-group>
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('dict.extend1')" prop="extend1">
            <el-input v-model="form.extend1" :placeholder="$t('dict.inputExtend')"></el-input>
          </el-form-item>
        </el-col>

        <el-col :lg="12">
          <el-form-item :label="$t('dict.extend2')" prop="extend2">
            <el-input v-model="form.extend2" :placeholder="$t('dict.inputExtend')"></el-input>
          </el-form-item>
        </el-col>
        <el-col :lg="24">
          <el-form-item :label="$t('dict.remark')" prop="remark">
            <el-input v-model="form.remark" type="textarea" :placeholder="$t('dict.inputRemark')"></el-input>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
    <template #footer>
      <div class="dialog-footer">
        <el-button text @click="cancel">{{ $t('btn.cancel') }}</el-button>
        <el-button type="primary" @click="submitForm">{{ $t('common.ok') }}</el-button>
      </div>
    </template>
  </el-dialog>
</template>
<script setup name="dictData">
import { listData, getData, delData, addData, updateData, changeStatus } from '@/api/system/dict/data'
import { listType, getType } from '@/api/system/dict/type'
const { proxy } = getCurrentInstance()
const props = defineProps({
  dictId: {
    type: Number,
    default: 0
  }
})
watch(
  () => props.dictId,
  (newVal, oldValue) => {
    if (newVal) {
      getTypeInfo(newVal)
      getTypeList()
      proxy.getDicts('sys_normal_disable').then((response) => {
        statusOptions.value = response.data
      })
    }
  },
  {
    immediate: true,
    deep: true
  }
)

const ids = ref()
const loading = ref(false)
// 总条数
const total = ref(0)
// 字典表格数据
const dataList = ref([])
// 默认字典类型
const defaultDictType = ref('')
// 弹出层标题
const title = ref('')
// 是否显示弹出层
const open = ref(false)
// 数据标签回显样式
const listClassOptions = ref([
  {
    value: 'default',
    label: proxy.$t('dict.classDefault')
  },
  {
    value: 'primary',
    label: proxy.$t('dict.classPrimary')
  },
  {
    value: 'success',
    label: proxy.$t('dict.classSuccess')
  },
  {
    value: 'info',
    label: proxy.$t('dict.classInfo')
  },
  {
    value: 'warning',
    label: proxy.$t('dict.classWarning')
  },
  {
    value: 'danger',
    label: proxy.$t('dict.classDanger')
  }
])

const cssClassOptions = ref([
  {
    value: 'text-primary',
    label: proxy.$t('dict.classPrimary')
  },
  {
    value: 'text-success',
    label: proxy.$t('dict.classSuccess')
  },
  {
    value: 'text-info',
    label: proxy.$t('dict.classInfo')
  },
  {
    value: 'text-warning',
    label: proxy.$t('dict.classWarning')
  },
  {
    value: 'text-danger',
    label: proxy.$t('dict.classDanger')
  },
  {
    value: 'text-orange',
    label: proxy.$t('dict.colorOrange')
  },
  {
    value: 'text-hotpink',
    label: proxy.$t('dict.colorPink')
  },
  {
    value: 'text-green',
    label: proxy.$t('dict.colorGreen')
  },
  {
    value: 'text-greenyellow',
    label: proxy.$t('dict.colorYellowGreen')
  },
  {
    value: 'text-purple',
    label: proxy.$t('dict.colorPurple')
  }
])
// 状态数据字典
const statusOptions = ref([])
// 类型数据字典
const typeOptions = ref([])
// 查询参数
const queryParams = reactive({
  pageNum: 1,
  pageSize: 10,
  dictName: undefined,
  dictType: undefined,
  status: ''
})
// 表单参数

const formRef = ref()
const state = reactive({
  form: {},
  rules: {
    dictLabel: [{ required: true, message: proxy.$t('dict.dictLabelRequired'), trigger: 'blur' }],
    dictValue: [{ required: true, message: proxy.$t('dict.dictValueRequired'), trigger: 'blur' }],
    dictSort: [{ required: true, message: proxy.$t('dict.dictSortRequired'), trigger: 'blur' }],
    langKey: [{ pattern: /^[A-Za-z].+$/, message: proxy.$t('dict.langKeyFormatIncorrect'), trigger: 'blur' }]
  }
})

const { form, rules } = toRefs(state)
/** 查询字典类型详细 */
function getTypeInfo(dictId) {
  getType(dictId).then((response) => {
    queryParams.dictType = response.data.dictType
    defaultDictType.value = response.data.dictType
    getList()
  })
}
/** 查询字典类型列表 */
function getTypeList() {
  listType().then((response) => {
    typeOptions.value = response.data.result
  })
}

/** 查询字典数据列表 */
function getList() {
  loading.value = true
  listData(queryParams).then((response) => {
    dataList.value = response.data.result
    total.value = response.data.totalNum
    loading.value = false
  })
}

// 取消按钮
function cancel() {
  open.value = false
  reset()
}
// 表单重置
function reset() {
  form.value = {
    dictCode: undefined,
    dictLabel: undefined,
    dictValue: undefined,
    dictSort: 0,
    status: '0',
    remark: undefined
  }
  proxy.resetForm('formRef')
}

/** 搜索按钮操作 */
function handleQuery() {
  queryParams.pageNum = 1
  getList()
}

/** 重置按钮操作 */
function resetQuery() {
  proxy.resetForm('queryForm')
  queryParams.dictType = defaultDictType.value
  handleQuery()
}
/** 新增按钮操作 */
function handleAdd() {
  reset()
  open.value = true
  title.value = proxy.$t('dict.addDictData')
  form.value.dictType = queryParams.dictType
}
// 多选框选中数据
// function handleSelectionChange(selection) {
//   this.ids = selection.map((item) => item.dictCode)
//   this.single = selection.length != 1
//   this.multiple = !selection.length
// }
/** 修改按钮操作 */
function handleUpdate(row) {
  reset()
  const dictCode = row.dictCode || ids.value
  getData(dictCode).then((response) => {
    form.value = response.data
    open.value = true
    title.value = proxy.$t('dict.editDictData')
  })
}
/** 提交按钮 */
function submitForm() {
  proxy.$refs['formRef'].validate((valid) => {
    if (valid) {
      if (form.value.dictCode != undefined) {
        updateData(form.value).then(() => {
          proxy.$modal.msgSuccess(proxy.$t('common.updateSuccess'))
          open.value = false
          getList()
        })
      } else {
        addData(form.value).then(() => {
          proxy.$modal.msgSuccess(proxy.$t('common.addSuccess'))
          open.value = false
          getList()
        })
      }
    }
  })
}

/** 删除按钮操作 */
function handleDelete(row) {
  const dictCodes = row.dictCode || ids.value
  proxy
    .$confirm(proxy.$t('dict.confirmDelete') + dictCodes, proxy.$t('common.warning'), {
      confirmButtonText: proxy.$t('common.ok'),
      cancelButtonText: proxy.$t('btn.cancel'),
      type: 'warning'
    })
    .then(function () {
      return delData(dictCodes)
    })
    .then(() => {
      getList()
      proxy.$modal.msgSuccess(proxy.$t('common.deleteSuccess'))
    })
}

function handleStatusChange(row) {
  const text = row.status == '0' ? proxy.$t('dict.enable') : proxy.$t('dict.disable')

  proxy
    .$confirm(proxy.$t('dict.confirmStatus', { action: text, label: row.dictLabel }), proxy.$t('common.warning'), {
      confirmButtonText: proxy.$t('common.ok'),
      cancelButtonText: proxy.$t('btn.cancel'),
      type: 'warning'
    })
    .then(function () {
      return changeStatus(row.dictCode, row.status)
    })
    .then(() => {
      proxy.$modal.msgSuccess(text + proxy.$t('common.successSuffix'))
    })
    .catch(function () {
      row.status = row.status == '0' ? '1' : '0'
    })
}
</script>
