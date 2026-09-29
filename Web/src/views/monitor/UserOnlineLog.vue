<!--
 * @Descripttion: (用户在线时长/UserOnlineLog)
 * @Author: (admin)
 * @Date: (2024-03-27)
-->
<template>
  <div>
    <el-form :model="queryParams" label-position="right" inline ref="queryRef" v-show="showSearch" @submit.prevent>
      <el-form-item :label="$t('userOnlineLogView.userId')" prop="userId">
        <el-input v-model.number="queryParams.userId" :placeholder="$t('userOnlineLogView.inputUserId')" />
      </el-form-item>
      <el-form-item :label="$t('userOnlineLogView.userIP')" prop="userIP">
        <el-input v-model="queryParams.userIP" :placeholder="$t('userOnlineLogView.inputUserIP')" />
      </el-form-item>
      <el-form-item :label="$t('userOnlineLogView.endTime')">
        <el-date-picker
          v-model="dateRangeAddTime"
          type="datetimerange"
          :start-placeholder="$t('userOnlineLogView.beginDate')"
          :end-placeholder="$t('userOnlineLogView.endDate')"
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
    <el-row :gutter="15" class="mb10">
      <el-col :span="1.5">
        <el-button type="warning" plain icon="download" @click="handleExport" v-hasPermi="['useronlinelog:export']">
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
      <el-table-column prop="id" label="Id" v-if="columns.showColumn('id')" />
      <el-table-column prop="userId" :label="$t('userOnlineLogView.userId')" align="center" v-if="columns.showColumn('userId')" />
      <el-table-column prop="nickName" :label="$t('userOnlineLogView.nickName')" align="center" />
      <el-table-column prop="platform" :label="$t('userOnlineLogView.platform')" align="center" />
      <el-table-column prop="onlineTime" :label="$t('userOnlineLogView.onlineTime')" align="center" v-if="columns.showColumn('onlineTime')" />
      <el-table-column prop="todayOnlineTime" :label="$t('userOnlineLogView.todayOnlineTime')" align="center" v-if="columns.showColumn('onlineTime')" />
      <el-table-column prop="addTime" :label="$t('userOnlineLogView.endTime')" width="170" :show-overflow-tooltip="true" v-if="columns.showColumn('addTime')" />
      <el-table-column prop="location" :label="$t('userOnlineLogView.location')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('location')" />
      <el-table-column prop="userIP" :label="$t('userOnlineLogView.userIP')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('userIP')" />
      <el-table-column prop="remark" :label="$t('userOnlineLogView.remark')" align="center" :show-overflow-tooltip="true" v-if="columns.showColumn('remark')" />
      <el-table-column :label="$t('btn.operate')" width="90" align="center">
        <template #default="scope">
          <el-button
            type="danger"
            size="small"
            icon="delete"
            :title="$t('btn.delete')"
            v-hasPermi="['useronlinelog:delete']"
            @click="handleDelete(scope.row)"></el-button>
        </template>
      </el-table-column>
    </el-table>
    <pagination :total="total" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />
  </div>
</template>

<script setup name="useronlinelog">
import { listUserOnlineLog, delUserOnlineLog } from '@/api/monitor/useronlinelog.js'
const { proxy } = getCurrentInstance()
const ids = ref([])
const loading = ref(false)
const showSearch = ref(true)
const queryParams = reactive({
  pageNum: 1,
  pageSize: 10,
  sort: 'Id',
  sortType: 'desc',
  userId: undefined,
  userIP: undefined
})
const columns = ref([
  { visible: false, prop: 'id', label: 'Id' },
  { visible: true, prop: 'userId', label: proxy.$t('userOnlineLogView.userId') },
  { visible: true, prop: 'onlineTime', label: proxy.$t('userOnlineLogView.onlineTime') },
  { visible: true, prop: 'addTime', label: proxy.$t('userOnlineLogView.endTime') },
  { visible: true, prop: 'location', label: proxy.$t('userOnlineLogView.location') },
  { visible: true, prop: 'userIP', label: proxy.$t('userOnlineLogView.userIP') },
  { visible: true, prop: 'remark', label: proxy.$t('userOnlineLogView.remark') }
])
const total = ref(0)
const dataList = ref([])
const queryRef = ref()
const defaultTime = ref([new Date(2000, 1, 1, 0, 0, 0), new Date(2000, 2, 1, 23, 59, 59)])
// 结束时间时间范围
const dateRangeAddTime = ref([])

var dictParams = []

function getList() {
  proxy.addDateRange(queryParams, dateRangeAddTime.value, 'AddTime')
  loading.value = true
  listUserOnlineLog(queryParams).then((res) => {
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
  rules: {
    id: [{ required: true, message: proxy.$t('userOnlineLogView.idRequired'), trigger: 'blur', type: 'number' }],
    userId: [{ required: true, message: proxy.$t('userOnlineLogView.userIdRequired'), trigger: 'blur', type: 'number' }],
    onlineTime: [{ required: true, message: proxy.$t('userOnlineLogView.onlineTimeRequired'), trigger: 'blur', type: 'number' }]
  },
  options: {}
})

const { form, rules, options, single, multiple } = toRefs(state)

// 删除按钮操作
function handleDelete(row) {
  const Ids = row.id || ids.value

  proxy
    .$confirm(proxy.$t('userOnlineLogView.deleteConfirm', { ids: Ids }), proxy.$t('common.tips'), {
      confirmButtonText: proxy.$t('common.ok'),
      cancelButtonText: proxy.$t('common.cancel'),
      type: 'warning'
    })
    .then(function () {
      return delUserOnlineLog(Ids)
    })
    .then(() => {
      getList()
      proxy.$modal.msgSuccess(proxy.$t('crud.deleteSuccess'))
    })
}

// 导出按钮操作
function handleExport() {
  proxy
    .$confirm(proxy.$t('userOnlineLogView.exportConfirm'), proxy.$t('common.tips'), {
      confirmButtonText: proxy.$t('common.ok'),
      cancelButtonText: proxy.$t('common.cancel'),
      type: 'warning'
    })
    .then(async () => {
      await proxy.downFile('/monitor/UserOnlineLog/export', { ...queryParams })
    })
}

handleQuery()
</script>
