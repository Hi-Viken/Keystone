<!--
 * @Descripttion: (数据差异日志/SqlDiffLog)
 * @Author: (zz)
 * @Date: (2023-08-17)
-->
<template>
  <div>
    <el-form :model="queryParams" label-position="right" inline ref="queryRef" v-show="showSearch" @submit.prevent>
      <el-form-item :label="$t('sqlDiffLogView.tableName')" prop="tableName">
        <el-input v-model="queryParams.tableName" :placeholder="$t('sqlDiffLogView.inputTableName')" />
      </el-form-item>
      <el-form-item :label="$t('sqlDiffLogView.diffType')" prop="diffType">
        <el-select clearable v-model="queryParams.diffType" :placeholder="$t('sqlDiffLogView.selectDiffType')">
          <el-option v-for="item in options.diffTypeOptions" :key="item.dictValue" :label="item.dictLabel" :value="item.dictValue">
            <span class="fl">{{ item.dictLabel }}</span>
            <span class="fr" style="color: var(--el-text-color-secondary)">{{ item.dictValue }}</span>
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item :label="$t('sqlDiffLogView.userName')" prop="userName">
        <el-input v-model="queryParams.userName" :placeholder="$t('sqlDiffLogView.inputUserName')" />
      </el-form-item>
      <el-form-item :label="$t('sqlDiffLogView.operateTime')">
        <el-date-picker
          v-model="dateRangeAddTime"
          type="datetimerange"
          :start-placeholder="$t('sqlDiffLogView.beginDate')"
          :end-placeholder="$t('sqlDiffLogView.endDate')"
          value-format="YYYY-MM-DD HH:mm:ss"
          :default-time="defaultTime"
          :shortcuts="dateOptions">
        </el-date-picker>
      </el-form-item>
      <el-form-item>
        <el-button icon="search" type="primary" @click="handleQuery">{{ $t('btn.search') }}</el-button>
        <el-button icon="refresh" @click="resetQuery">{{ $t('btn.reset') }}</el-button>
      </el-form-item>
    </el-form>
    <!-- 工具区域 -->
    <el-row :gutter="10" class="mb8">
      <el-col :span="1.5">
        <el-button type="warning" plain icon="download" @click="handleExport" v-hasPermi="['sqldifflog:export']">
          {{ $t('btn.export') }}
        </el-button>
      </el-col>
      <right-toolbar v-model:showSearch="showSearch" @queryTable="getList" :columns="columns"></right-toolbar>
    </el-row>

    <el-table
      :data="dataList"
      v-loading="loading"
      ref="table"
      border
      header-cell-class-name="el-table-header-cell"
      highlight-current-row
      @sort-change="sortChange">
      <el-table-column prop="pId" :label="$t('sqlDiffLogView.primaryKey')" align="center" v-if="columns.showColumn('pId')" width="150" />
      <el-table-column prop="tableName" :label="$t('sqlDiffLogView.tableName')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('tableName')" />
      <el-table-column prop="diffType" :label="$t('sqlDiffLogView.operateType')" align="center" v-if="columns.showColumn('diffType')">
        <template #default="scope">
          <dict-tag :options="options.diffTypeOptions" :value="scope.row.diffType" />
        </template>
      </el-table-column>
      <el-table-column
        prop="businessData"
        :label="$t('sqlDiffLogView.businessData')"
        align="center"
        :show-overflow-tooltip="true"
        v-if="columns.showColumn('businessData')" />
      <el-table-column prop="sql" :label="$t('sqlDiffLogView.execSql')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('sql')" />
      <el-table-column prop="beforeData" :label="$t('sqlDiffLogView.beforeData')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('beforeData')" />
      <el-table-column prop="afterData" :label="$t('sqlDiffLogView.afterData')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('afterData')" />
      <el-table-column prop="userName" :label="$t('sqlDiffLogView.userName')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('userName')" />
      <el-table-column prop="addTime" :label="$t('sqlDiffLogView.operateTime')" :show-overflow-tooltip="true" v-if="columns.showColumn('addTime')" />
      <el-table-column prop="configId" :label="$t('sqlDiffLogView.configId')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('configId')" />
      <el-table-column :label="$t('btn.operate')" width="130">
        <template #default="scope">
          <el-button text type="primary" icon="view" :title="$t('btn.details')" @click="handlePreview(scope.row)">{{ $t('btn.details') }}</el-button>
          <el-button v-hasPermi="['sqldifflog:delete']" type="danger" icon="delete" :title="$t('btn.delete')" text @click="handleDelete(scope.row)"
            >{{ $t('btn.delete') }}</el-button
          >
        </template>
      </el-table-column>
    </el-table>
    <pagination :total="total" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />

    <!-- 添加或修改数据差异日志对话框 -->
    <zr-dialog :title="title" :lock-scroll="false" v-model="open" @close="cancel">
      <el-form ref="formRef" :model="form" label-width="100px">
        <el-row :gutter="20">
          <el-col :lg="12">
            <el-form-item :label="$t('sqlDiffLogView.primaryKey')" prop="pId">
              <el-input v-model.number="form.pId" :placeholder="$t('sqlDiffLogView.inputPrimaryKey')" :disabled="opertype != 1" />
            </el-form-item>
          </el-col>

          <el-col :lg="12">
            <el-form-item :label="$t('sqlDiffLogView.tableName')" prop="tableName">
              <el-input v-model="form.tableName" disabled :placeholder="$t('sqlDiffLogView.inputTableName')" />
            </el-form-item>
          </el-col>

          <el-col :lg="24">
            <el-form-item :label="$t('sqlDiffLogView.businessData')" prop="businessData">
              <el-input type="textarea" v-model="form.businessData" :placeholder="$t('sqlDiffLogView.inputBusinessData')" />
            </el-form-item>
          </el-col>

          <el-col :lg="12">
            <el-form-item :label="$t('sqlDiffLogView.operateType')" prop="diffType">
              <dict-tag :options="options.diffTypeOptions" :value="form.diffType"></dict-tag>
            </el-form-item>
          </el-col>

          <el-col :lg="24">
            <el-form-item :label="$t('sqlDiffLogView.execSql')" prop="sql">
              <code class="hljs" v-html="highlightedCode(form.sql)"></code>
              <!-- <el-input type="textarea" v-model="form.sql" placeholder="请输入执行sql语句" /> -->
            </el-form-item>
          </el-col>

          <el-col :lg="24">
            <el-form-item :label="$t('sqlDiffLogView.beforeData')" prop="beforeData">
              <code class="hljs" v-html="highlightedCode(form.beforeData)"></code>
            </el-form-item>
          </el-col>

          <el-col :lg="24">
            <el-form-item :label="$t('sqlDiffLogView.afterData')">
              <code class="hljs" v-html="highlightedCode(form.afterData)"></code>
            </el-form-item>
          </el-col>
          <el-col :lg="24">
            <code-diff :old-string="form.beforeData" :new-string="form.afterData" language="json" output-format="side-by-side" />
          </el-col>

          <el-col :lg="12">
            <el-form-item :label="$t('sqlDiffLogView.userName')" prop="userName">
              <el-input v-model="form.userName" :placeholder="$t('sqlDiffLogView.inputUserName')" disabled />
            </el-form-item>
          </el-col>

          <el-col :lg="12">
            <el-form-item :label="$t('sqlDiffLogView.recordTime')" prop="addTime">
              <el-date-picker v-model="form.addTime" disabled type="datetime" :teleported="false" :placeholder="$t('sqlDiffLogView.selectDateTime')"></el-date-picker>
            </el-form-item>
          </el-col>

          <el-col :lg="12">
            <el-form-item :label="$t('sqlDiffLogView.configId')" prop="configId">
              <el-input v-model="form.configId" disabled :placeholder="$t('sqlDiffLogView.inputConfigId')" />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
      <template #footer v-if="opertype != 3">
        <el-button text @click="cancel">{{ $t('btn.cancel') }}</el-button>
      </template>
    </zr-dialog>
  </div>
</template>

<script setup name="sqldifflog">
import { listSqlDiffLog, delSqlDiffLog } from '@/api/monitor/sqldifflog.js'
import { CodeDiff } from 'v-code-diff'
import hljs from '@/utils/hljs'
import 'highlight.js/styles/default.css' // 这里有多个样式，自己可以根据需要切换

const { proxy } = getCurrentInstance()
const ids = ref([])
const loading = ref(false)
const showSearch = ref(true)
const queryParams = reactive({
  pageNum: 1,
  pageSize: 10,
  sort: 'PId',
  sortType: 'desc',
  tableName: undefined,
  diffType: undefined,
  userName: undefined,
  addTime: undefined
})
const columns = ref([
  { visible: true, prop: 'pId', label: proxy.$t('sqlDiffLogView.primaryKey') },
  { visible: true, prop: 'tableName', label: proxy.$t('sqlDiffLogView.tableName') },
  { visible: true, prop: 'businessData', label: proxy.$t('sqlDiffLogView.businessData') },
  { visible: true, prop: 'diffType', label: proxy.$t('sqlDiffLogView.diffType') },
  { visible: true, prop: 'sql', label: proxy.$t('sqlDiffLogView.execSql') },
  { visible: true, prop: 'beforeData', label: proxy.$t('sqlDiffLogView.beforeData') },
  { visible: true, prop: 'afterData', label: proxy.$t('sqlDiffLogView.afterData') },
  { visible: true, prop: 'userName', label: proxy.$t('sqlDiffLogView.userName') },
  { visible: false, prop: 'addTime', label: proxy.$t('sqlDiffLogView.operateTime') },
  { visible: false, prop: 'configId', label: proxy.$t('sqlDiffLogView.configId') }
])
const total = ref(0)
const dataList = ref([])
const queryRef = ref()
const defaultTime = ref([new Date(2000, 1, 1, 0, 0, 0), new Date(2000, 2, 1, 23, 59, 59)])

// AddTime时间范围
const dateRangeAddTime = ref([])

var dictParams = []

function getList() {
  proxy.addDateRange(queryParams, dateRangeAddTime.value, 'AddTime')
  loading.value = true
  listSqlDiffLog(queryParams).then((res) => {
    const { code, data } = res
    if (code == 200) {
      dataList.value = data.result
      total.value = data.totalNum
      loading.value = false
    }
  })
}

// 查询
function handleQuery() {
  queryParams.pageNum = 1
  getList()
}

// 重置查询操作
function resetQuery() {
  // AddTime时间范围
  dateRangeAddTime.value = []
  proxy.resetForm('queryRef')
  handleQuery()
}
// 自定义排序
function sortChange(column) {
  var sort = undefined
  var sortType = undefined

  if (column.prop != null && column.order != null) {
    sort = column.prop
    sortType = column.order
  }
  queryParams.sort = sort
  queryParams.sortType = sortType
  handleQuery()
}

/*************** form操作 ***************/
const formRef = ref()
const title = ref('')
// 操作类型 1、add 2、edit 3、view
const opertype = ref(0)
const open = ref(false)
const state = reactive({
  single: true,
  multiple: true,
  form: {},
  options: {
    // 差异类型 选项列表 格式 eg:{ dictLabel: '标签', dictValue: '0'}
    diffTypeOptions: [
      { dictLabel: 'insert', dictValue: 'insert' },
      { dictLabel: 'update', dictValue: 'update', listClass: 'success' },
      { dictLabel: 'delete', dictValue: 'delete', listClass: 'danger' }
    ]
  }
})

const { form, options } = toRefs(state)

// 关闭dialog
function cancel() {
  open.value = false
  reset()
}

// 重置表单
function reset() {
  form.value = {
    pId: null,
    tableName: null,
    businessData: null,
    diffType: null,
    sql: null,
    beforeData: null,
    afterData: null,
    userName: null,
    addTime: null,
    configId: null
  }
  proxy.resetForm('formRef')
}

// 删除按钮操作
function handleDelete(row) {
  const Ids = row.pId || ids.value

  proxy
    .$confirm(proxy.$t('sqlDiffLogView.deleteConfirm', { ids: Ids }))
    .then(function () {
      return delSqlDiffLog(Ids)
    })
    .then(() => {
      getList()
      proxy.$modal.msgSuccess(proxy.$t('crud.deleteSuccess'))
    })
    .catch(() => {})
}

/**
 * 查看
 * @param {*} row
 */
function handlePreview(row) {
  reset()
  open.value = true
  title.value = proxy.$t('btn.details')
  opertype.value = 3
  form.value = { ...row }
}

// 导出按钮操作
function handleExport() {
  proxy
    .$confirm(proxy.$t('sqlDiffLogView.exportConfirm'), proxy.$t('common.tips'), {
      confirmButtonText: proxy.$t('common.ok'),
      cancelButtonText: proxy.$t('common.cancel'),
      type: 'warning'
    })
    .then(async () => {
      await proxy.downFile('/monitor/SqlDiffLog/export', { ...queryParams })
    })
}
function highlightedCode(code) {
  const result = hljs.highlightAuto(code || '')
  return result.value || '&nbsp;'
}
handleQuery()
</script>
