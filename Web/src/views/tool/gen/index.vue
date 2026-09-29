<template>
  <div class="app-container">
    <el-form ref="codeform" :inline="true" :model="queryParams">
      <el-form-item :label="$t('genView.tableName')" prop="tableName">
        <el-input v-model="queryParams.tableName" style="width: 200px" clearable :placeholder="$t('genView.tableNamePh')" />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" icon="search" @click="getList()">{{ $t('btn.search') }}</el-button>
        <el-button icon="refresh" @click="handleReset()">{{ $t('btn.reset') }}</el-button>
      </el-form-item>
    </el-form>

    <el-row :gutter="10" class="mb10">
      <el-col :span="1.5">
        <el-button type="info" plain icon="upload" @click="openImportTable" v-hasPermi="['tool:gen:import']">{{ $t('genView.importTable') }}</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button type="danger" :disabled="multiple" plain icon="delete" @click="handleDelete" v-hasPermi="['tool:gen:remove']"> {{ $t('btn.delete') }}</el-button>
      </el-col>
    </el-row>
    <el-table
      ref="gridtable"
      v-loading="tableloading"
      :data="tableList"
      border
      @selection-change="handleSelectionChange"
      highlight-current-row
      height="400px">
      <el-table-column type="selection" align="center" width="55"></el-table-column>
      <el-table-column prop="tableId" label="tableId" width="80" sortable="" />
      <el-table-column prop="dbName" :label="$t('genView.dbName')" width="90" :show-overflow-tooltip="true" />
      <el-table-column prop="tplCategory" :label="$t('genView.genTemplate')" width="90" sortable="" />
      <el-table-column prop="tableName" :label="$t('genView.tableName')" width="120" :show-overflow-tooltip="true" />
      <el-table-column prop="tableComment" :label="$t('genView.tableComment')" :show-overflow-tooltip="true" width="120" />
      <el-table-column prop="className" :label="$t('genView.entity')" :show-overflow-tooltip="true" />
      <el-table-column prop="createTime" :label="$t('common.createTime')" sortable />
      <el-table-column prop="updateTime" :label="$t('common.updateTime')" sortable />
      <el-table-column :label="$t('btn.operate')" align="center" width="200">
        <template #default="scope">
          <el-button-group>
            <el-button text icon="view" @click="handlePreview(scope.row)" v-hasPermi="['tool:gen:preview']"> {{ $t('common.preview') }} </el-button>
            <el-button text icon="edit" @click="handleEditTable(scope.row)" v-hasPermi="['tool:gen:edit']"> {{ $t('btn.edit') }} </el-button>

            <el-dropdown @command="handleCommand($event, scope.row)">
              <el-button text>
                {{ $t('btn.more') }}
                <el-icon class="el-icon--right">
                  <arrow-down />
                </el-icon>
              </el-button>

              <template #dropdown>
                <el-dropdown-menu>
                  <div v-hasPermi="['tool:gen:code']">
                    <el-dropdown-item command="generate">
                      <el-button icon="download" link>{{ $t('genView.genCode') }}</el-button>
                    </el-dropdown-item>
                  </div>
                  <div v-hasPermi="['tool:gen:edit']">
                    <el-dropdown-item command="sync">
                      <el-button icon="refresh" link> {{ $t('btn.synchronize') }} </el-button>
                    </el-dropdown-item>
                  </div>
                  <div v-hasPermi="['tool:gen:remove']">
                    <el-dropdown-item command="delete">
                      <el-button icon="delete" type="danger" link> {{ $t('btn.delete') }} </el-button>
                    </el-dropdown-item>
                  </div>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
          </el-button-group>
        </template>
      </el-table-column>
    </el-table>
    <pagination v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" v-model:total="total" @pagination="getList" />

    <!-- 预览界面 -->
    <zr-dialog v-model="preview.open" width="80%" top="5vh" append-to-body>
      <el-tabs v-model="preview.activeName">
        <el-tab-pane v-for="(item, key) in preview.data" :label="item.title" :id="key" :name="key.toString()" :key="key">
          {{ item.path }}
          <el-link :underline="false" icon="DocumentCopy" @click="onCopy(item.content)" class="btn-copy">{{ $t('btn.copy') }} </el-link>
          <pre><code class="hljs" v-html="highlightedCode(item.content)"></code></pre>
        </el-tab-pane>
      </el-tabs>
    </zr-dialog>
    <import-table ref="importRef" @ok="getList" />
  </div>
</template>

<script setup name="gen">
import { codeGenerator, listTable, delTable, previewTable, synchDb } from '@/api/tool/gen'
import { useRouter } from 'vue-router'
import importTable from './importTable'
import hljs from '@/utils/hljs'
import 'highlight.js/styles/dark.css' // 这里有多个样式，自己可以根据需要切换
import { useClipboard } from '@vueuse/core'
const route = useRoute()
const router = useRouter()
const { proxy } = getCurrentInstance()

const tableList = ref([])
const tableloading = ref(true)
const tableIds = ref([])
const single = ref(true)
const multiple = ref(true)
const total = ref(0)
const dateRange = ref([])
const showGenerate = ref(false)
// 选中行的表
const currentSelected = ref({})

const data = reactive({
  queryParams: {
    pageNum: 1,
    pageSize: 10,
    tableName: undefined,
    t: 0
  },
  preview: {
    open: false,
    title: proxy.$t('genView.codePreview'),
    data: {},
    activeName: '0'
  }
})

const { queryParams, preview } = toRefs(data)
watch(
  route,
  (val) => {
    if (val) {
      getList()
    }
  },
  {
    immediate: true
  }
)
/** 查询表集合 */
function getList() {
  tableloading.value = true
  listTable(proxy.addDateRange(queryParams.value, dateRange.value)).then((response) => {
    tableList.value = response.data.result
    total.value = response.data.totalNum
    tableloading.value = false
  })
}
/** 搜索按钮操作 */
function handleQuery() {
  queryParams.value.pageNum = 1
  getList()
}
/** 生成代码操作 */
function handleGenTable(row) {
  currentSelected.value = row
  if (!currentSelected.value) {
    proxy.$modal.msgError(proxy.$t('genView.selectTableFirst'))
    return false
  }
  proxy.$refs['codeform'].validate((valid) => {
    if (valid) {
      proxy.$modal.loading(proxy.$t('genView.generating'))

      codeGenerator({
        tableId: currentSelected.value.tableId,
        tableName: currentSelected.value.name,
        VueVersion: 3
      })
        .then(async (res) => {
          const { data } = res
          showGenerate.value = false
          if (row.genType === '1') {
            proxy.$modal.msgSuccess(proxy.$t('genView.genCustomPathSuccess'))
          } else {
            proxy.$modal.msgSuccess(proxy.$t('genView.genComplete'))
            // proxy.download(data.path)
            await proxy.downFile('/common/downloadFile', { path: data.path })
          }
        })
        .finally(() => {
          proxy.$modal.closeLoading()
        })
    } else {
      return false
    }
  })
}
/** 同步数据库操作 */
function handleSynchDb(row) {
  const tableName = row.tableName
  proxy
    .$confirm(proxy.$t('genView.syncConfirm', { name: tableName }))
    .then(function () {
      return synchDb(row.tableId, { tableName, dbName: row.dbName })
    })
    .then(() => {
      proxy.$modal.msgSuccess(proxy.$t('genView.syncSuccess'))
    })
    .catch(() => {})
}
/** 打开导入表弹窗 */
function openImportTable() {
  proxy.$refs['importRef'].show()
}
/** 预览按钮 */
function handlePreview(row) {
  proxy.$refs['codeform'].validate((valid) => {
    if (!valid) {
      proxy.$modal.msgError(proxy.$t('genView.completeFormFirst'))
      return
    }
    proxy.$modal.loading(proxy.$t('genView.pleaseWait'))
    previewTable(row.tableId, { VueVersion: 3 })
      .then((res) => {
        if (res.code === 200) {
          showGenerate.value = false
          preview.value.open = true
          preview.value.data = res.data
        }
      })
      .finally(() => {
        proxy.$modal.closeLoading()
      })
  })
}
// 多选框选中数据
function handleSelectionChange(selection) {
  tableIds.value = selection.map((item) => item.tableId)
  multiple.value = !selection.length
}
/** 编辑表格 */
function handleEditTable(row) {
  queryParams.value.tableName = row.tableName
  getList()

  router.push({
    path: '/gen/editTable',
    query: { tableId: row.tableId }
  })
}
/** 删除按钮操作 */
function handleDelete(row) {
  const Ids = row.tableId || tableIds.value
  proxy
    .$confirm(proxy.$t('genView.deleteConfirm'), proxy.$t('common.tips'), {
      confirmButtonText: proxy.$t('common.ok'),
      cancelButtonText: proxy.$t('common.cancel'),
      type: 'warning'
    })
    .then(() => {
      delTable(Ids.toString()).then((res) => {
        if (res.code == 200) {
          proxy.$modal.msgSuccess(proxy.$t('crud.deleteSuccess'))

          handleQuery()
        }
      })
    })
    .catch(() => {
      proxy.$message({
        type: 'info',
        message: proxy.$t('genView.deleteCanceled')
      })
    })
}
/** 高亮显示 */
function highlightedCode(code) {
  const result = hljs.highlightAuto(code || '')
  return result.value || '&nbsp;'
}

const { copy, isSupported } = useClipboard()
function onCopy(input) {
  if (isSupported) {
    copy(input)
    proxy.$modal.msgSuccess(proxy.$t('common.copySuccess'))
  } else {
    proxy.$modal.msgError(proxy.$t('common.browserNotSupport'))
  }
}
function handleCommand(command, row) {
  switch (command) {
    case 'generate':
      handleGenTable(row)
      break
    case 'delete':
      handleDelete(row)
      break
    case 'sync':
      handleSynchDb(row)
      break
  }
}
function handleReset() {
  proxy.resetForm('codeform')
  handleQuery()
}
getList()
</script>
<style>
.btn-copy {
  position: absolute;
  right: 0;
  top: -5px;
}
.el-dropdown {
  vertical-align: middle;
}
</style>
