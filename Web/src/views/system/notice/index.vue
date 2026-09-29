<template>
  <div class="app-container">
    <el-form :model="queryParams" ref="queryRef" :inline="true" v-show="showSearch" label-width="68px">
      <el-form-item :label="$t('noticeView.noticeTitle')" prop="noticeTitle">
        <el-input v-model="queryParams.noticeTitle" :placeholder="$t('noticeView.noticeTitlePh')" clearable @keyup.enter="handleQuery" />
      </el-form-item>
      <el-form-item :label="$t('common.type')" prop="noticeType">
        <el-select v-model="queryParams.noticeType" :placeholder="$t('noticeView.noticeTypePh')" clearable>
          <el-option v-for="dict in options.sys_notice_type" :key="dict.dictValue" :label="dict.dictLabel" :value="dict.dictValue" />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" icon="Search" @click="handleQuery">{{ $t('btn.search') }}</el-button>
        <el-button icon="Refresh" @click="resetQuery">{{ $t('btn.reset') }}</el-button>
      </el-form-item>
    </el-form>

    <el-row :gutter="10" class="mb8">
      <el-col :span="1.5">
        <el-button type="primary" plain icon="Plus" @click="handleAdd" v-hasPermi="['system:notice:add']"> {{ $t('btn.add') }}</el-button>
      </el-col>

      <el-col :span="1.5">
        <el-button type="danger" plain icon="Delete" :disabled="multiple" @click="handleDelete" v-hasPermi="['system:notice:delete']">
          {{ $t('btn.delete') }}
        </el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button type="warning" plain icon="download" @click="handleExport" v-hasPermi="['system:notice:export']">
          {{ $t('btn.export') }}
        </el-button>
      </el-col>
      <right-toolbar v-model:showSearch="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="noticeList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column :label="$t('noticeView.no')" align="center" prop="noticeId" width="100" />
      <el-table-column :label="$t('noticeView.noticeTitle')" prop="noticeTitle" :show-overflow-tooltip="true">
        <template #default="{ row }">
          <el-link type="primary" @click="handleOpenPre(row)">{{ row.noticeTitle }}</el-link>
        </template>
      </el-table-column>
      <el-table-column :label="$t('noticeView.noticeType')" align="center" prop="noticeType" width="100">
        <template #default="scope">
          <dict-tag :options="options.sys_notice_type" :value="scope.row.noticeType" />
        </template>
      </el-table-column>
      <el-table-column :label="$t('common.status')" align="center" prop="status" width="100">
        <template #default="scope">
          <dict-tag :options="options.sys_notice_status" :value="scope.row.status" />
        </template>
      </el-table-column>
      <el-table-column :label="$t('noticeView.showTime')" width="130">
        <template #default="scope">
          <div>{{ scope.row.beginTime }}</div>
          <div>{{ scope.row.endTime }}</div>
        </template>
      </el-table-column>
      <el-table-column :label="$t('noticeView.publisher')" align="center" prop="publisher" />
      <el-table-column :label="$t('noticeView.isPopup')" align="center" prop="popup">
        <template #default="scope">
          <DictTag :options="options.popupStatus" :value="scope.row.popup"></DictTag>
        </template>
      </el-table-column>
      <el-table-column :label="$t('common.creator')" align="center" prop="createBy" width="100" />
      <el-table-column :label="$t('common.createTime')" align="center" prop="createTime" width="100">
        <template #default="scope">
          <span>{{ parseTime(scope.row.createTime, 'YYYY-MM-DD') }}</span>
        </template>
      </el-table-column>
      <el-table-column :label="$t('btn.operate')">
        <template #default="scope">
          <el-button text icon="bell" @click="handleNotice(scope.row)" v-hasPermi="['system:notice:update']"> {{ $t('noticeView.notify') }}</el-button>
          <el-button text icon="Edit" @click="handleUpdate(scope.row)" v-hasPermi="['system:notice:update']"> {{ $t('btn.edit') }}</el-button>
          <el-button text icon="Delete" @click="handleDelete(scope.row)" v-hasPermi="['system:notice:delete']"> {{ $t('btn.delete') }}</el-button>
        </template>
      </el-table-column>
    </el-table>

    <pagination :total="total" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />

    <publishNoticeForm ref="publishRef" :options="options" @success="getList()"></publishNoticeForm>
    <zr-dialog :title="$t('noticeView.preview')" draggable v-model="openPreview" width="580px">
      <template v-if="info">
        <h2 style="text-align: center; margin-top: 0" class="mb10">{{ info.noticeTitle }}</h2>
        <div v-html="info.noticeContent"></div>

        <div class="n_right">{{ info.create_by }}</div>
        <div class="n_right">{{ dayjs(info.create_time).format('YYYY-MM-DD HH:mm') }}</div>
      </template>
      <template #footer>
        <el-button type="primary" @click="openPreview = false">{{ $t('btn.submit') }}</el-button>
      </template>
    </zr-dialog>
  </div>
</template>

<script setup name="notice">
import publishNoticeForm from '@/views/components/form/publishNoticeForm.vue'

import { listNotice, delNotice, sendNotice } from '@/api/system/notice'
import { getCurrentInstance } from 'vue'
import dayjs from 'dayjs'

const { proxy } = getCurrentInstance()
const noticeList = ref([])

const loading = ref(true)
const showSearch = ref(true)
const ids = ref([])
const single = ref(true)
const multiple = ref(true)
const total = ref(0)

const data = reactive({
  form: {},
  queryParams: {
    pageNum: 1,
    pageSize: 10,
    noticeTitle: undefined,
    createBy: undefined,
    status: undefined
  },
  options: {
    sys_notice_type: [],
    sys_notice_status: [],
    popupStatus: [{ dictLabel: proxy.$t('common.yes'), dictValue: '1' }]
  }
})

proxy.getDicts(['sys_notice_type', 'sys_notice_status']).then((response) => {
  response.data.forEach((element) => {
    data.options[element.dictType] = element.list
  })
})

const { queryParams, options } = toRefs(data)

/** 查询公告列表 */
function getList() {
  loading.value = true
  listNotice(queryParams.value).then((response) => {
    noticeList.value = response.data.result
    total.value = response.data.totalNum
    loading.value = false
  })
}
/** 搜索按钮操作 */
function handleQuery() {
  queryParams.value.pageNum = 1
  getList()
}
/** 重置按钮操作 */
function resetQuery() {
  proxy.resetForm('queryRef')
  handleQuery()
}
/** 多选框选中数据 */
function handleSelectionChange(selection) {
  ids.value = selection.map((item) => item.noticeId)
  single.value = selection.length != 1
  multiple.value = !selection.length
}
/** 新增按钮操作 */
function handleAdd() {
  proxy.$refs.publishRef.handleAdd()
}
/**修改按钮操作 */
function handleUpdate(row) {
  proxy.$refs.publishRef.handleUpdate(row)
}
/** 删除按钮操作 */
function handleDelete(row) {
  const noticeIds = row.noticeId || ids.value
  proxy.$modal
    .confirm(proxy.$t('crud.deleteConfirm', { id: noticeIds }))
    .then(function () {
      return delNotice(noticeIds)
    })
    .then(() => {
      getList()
      proxy.$modal.msgSuccess(proxy.$t('crud.deleteSuccess'))
    })
}
// 发送通知
function handleNotice(row) {
  const noticeId = row.noticeId || ids.value
  sendNotice(noticeId).then((res) => {
    proxy.$modal.msgSuccess(proxy.$t('noticeView.sendSuccess'))
  })
}
// 导出按钮操作
function handleExport() {
  proxy
    .$confirm(proxy.$t('noticeView.confirmExport'), proxy.$t('common.warning'), {
      confirmButtonText: proxy.$t('common.confirm'),
      cancelButtonText: proxy.$t('common.cancel'),
      type: 'warning'
    })
    .then(async () => {
      await proxy.downFile('/system/Notice/export', { ...queryParams })
    })
}
const openPreview = ref(false)
const info = ref(undefined)
const handleOpenPre = function (row) {
  openPreview.value = true
  info.value = { ...row }
}

getList()
</script>
<style>
.n_right {
  text-align: right;
  margin: 10px;
}
</style>
