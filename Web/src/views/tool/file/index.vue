<template>
  <div class="app-container">
    <el-form :model="queryParams" label-position="left" inline ref="queryForm" v-show="showSearch" @submit.prevent>
      <el-form-item label="" prop="storeType">
        <el-radio-group v-model="queryParams.storeType" @change="handleQuery" :placeholder="$t('fileView.storeTypePh')">
          <el-radio-button value=""> {{ $t('common.all') }} </el-radio-button>
          <el-radio-button v-for="item in storeTypeOptions" :key="item.dictValue" :value="item.dictValue">
            {{ item.dictLabel }}
          </el-radio-button>
        </el-radio-group>
      </el-form-item>
      <el-form-item :label="$t('fileView.classifyType')" prop="classifyType">
        <el-select clearable v-model="queryParams.classifyType" :placeholder="$t('fileView.classifyTypePh')">
          <el-option v-for="item in classifyTypeOptions" :key="item.dictValue" :label="item.dictLabel" :value="item.dictValue">
            <span class="fl">{{ item.dictLabel }}</span>
            <span class="fr" style="color: var(--el-text-color-secondary)">{{ item.dictValue }}</span>
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="" prop="fileId">
        <el-input v-model="queryParams.fileId" :placeholder="$t('fileView.fileIdPh')" clearable />
      </el-form-item>
      <el-form-item label="">
        <el-date-picker
          v-model="dateRangeAddTime"
          type="daterange"
          range-separator="-"
          :start-placeholder="$t('common.startDate')"
          :end-placeholder="$t('common.endDate')"
          :placeholder="$t('fileView.uploadTimePh')"
          :shortcuts="dateOptions"></el-date-picker>
      </el-form-item>

      <el-form-item>
        <el-button type="primary" icon="search" @click="handleQuery">{{ $t('btn.search') }}</el-button>
        <el-button icon="refresh" @click="resetQuery">{{ $t('btn.reset') }}</el-button>
      </el-form-item>
    </el-form>
    <!-- 工具区域 -->
    <el-row :gutter="10" class="mb8">
      <el-col :span="1.5">
        <el-button type="primary" v-hasPermi="['tool:file:add']" plain icon="upload" @click="handleAdd">
          {{ $t('btn.upload') }}
        </el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button type="danger" :disabled="multiple" v-hasPermi="['tool:file:delete']" plain icon="delete" @click="handleDelete">
          {{ $t('btn.delete') }}
        </el-button>
      </el-col>
      <right-toolbar :showSearch="showSearch" @queryTable="getList"> </right-toolbar>
    </el-row>

    <!-- 数据区域 -->
    <el-table :data="dataList" v-loading="loading" ref="table" border highlight-current-row @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="50" align="center" />
      <el-table-column prop="id" :label="$t('fileView.fileId')" width="150" :show-overflow-tooltip="true">
        <template #default="scope">
          <el-button text size="small" type="success" @click="handleView(scope.row)">
            {{ scope.row.id }}
          </el-button>
        </template>
      </el-table-column>
      <el-table-column prop="fileName" :label="$t('fileView.fileName')" align="left" width="180" :show-overflow-tooltip="true">
        <template #default="scope">
          <el-link type="primary" :href="scope.row.accessUrl" target="_blank">{{ scope.row.fileName }}</el-link>
        </template>
      </el-table-column>
      <el-table-column prop="accessUrl" align="center" :label="$t('fileView.previewImg')" width="80">
        <template #default="{ row }">
          <el-image
            preview-teleported
            :src="row.accessUrl"
            :preview-src-list="[row.accessUrl]"
            :hide-on-click-modal="true"
            fit="contain"
            lazy
            class="el-avatar">
            <template #error>
              <el-icon><document /></el-icon>
            </template>
          </el-image>
        </template>
      </el-table-column>
      <el-table-column prop="fileSize" :label="$t('fileView.fileSize')" align="center" :show-overflow-tooltip="true" />
      <el-table-column prop="fileExt" :label="$t('fileView.fileExt')" align="center" :show-overflow-tooltip="true" width="80px" />
      <!-- <el-table-column prop="storeType" label="存储类型" align="center">
        <template #default="scope">
          <dict-tag :options="storeTypeOptions" :value="parseInt(scope.row.storeType)" />
        </template>
      </el-table-column> -->
      <el-table-column prop="classifyType" :label="$t('fileView.classifyType')" align="center" width="100px">
        <template #default="scope">
          <!-- <dict-tag :options="classifyTypeOptions" :value="scope.row.classifyType" /> -->
          <el-select
            v-model="scope.row.classifyType"
            clearable
            @change="handleClassifyChange(scope.row)"
            size="small"
            style="width: 90px"
            :placeholder="$t('fileView.selectClassify')">
            <el-option v-for="item in classifyTypeOptions" :key="item.dictValue" :label="item.dictLabel" :value="item.dictValue"></el-option>
          </el-select>
        </template>
      </el-table-column>
      <el-table-column prop="storePath" :label="$t('fileView.storePath')"></el-table-column>
      <el-table-column prop="create_by" :label="$t('fileView.operator')" align="center" />
      <el-table-column prop="create_time" :label="$t('common.createTime')" align="center">
        <template #default="{ row }">
          {{ showTime(row.create_time) }}
        </template>
      </el-table-column>
      <el-table-column :label="$t('btn.operate')" align="center" width="110">
        <template #default="scope">
          <el-button
            text
            size="small"
            icon="download"
            :title="$t('btn.download')"
            v-hasPermi="['tool:file:download']"
            v-if="scope.row.storeType == 1"
            @click="handleDown(scope.row)"></el-button>
          <el-button class="copy-btn-main" icon="document-copy" :title="$t('btn.copy')" text size="small" @click="copyText(scope.row.accessUrl)"> </el-button>
          <el-button v-hasPermi="['tool:file:delete']" :title="$t('btn.delete')" text size="small" icon="delete" @click="handleDelete(scope.row)"> </el-button>
        </template>
      </el-table-column>
    </el-table>
    <pagination background :total="total" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />

    <el-dialog :title="title" :lock-scroll="false" v-model="open" width="400px" draggable>
      <el-form ref="formRef" :model="form" :rules="rules" label-width="90px" label-position="left">
        <el-row>
          <el-col :lg="24">
            <el-form-item :label="$t('fileView.storeType')" prop="storeType">
              <el-radio-group v-model="form.storeType" :placeholder="$t('fileView.storeTypePh')">
                <el-radio-button v-for="item in storeTypeOptions" :key="item.dictValue" :value="parseInt(item.dictValue)">
                  {{ item.dictLabel }}
                </el-radio-button>
              </el-radio-group>
            </el-form-item>
          </el-col>
          <el-col :lg="24">
            <el-form-item :label="$t('fileView.fileNameRule')" prop="fileNameType">
              <el-radio-group v-model="form.fileNameType" :placeholder="$t('fileView.fileNameTypePh')">
                <el-radio-button v-for="item in fileNameTypeOptions" :key="item.dictValue" :value="parseInt(item.dictValue)">
                  {{ item.dictLabel }}
                </el-radio-button>
              </el-radio-group>
            </el-form-item>
          </el-col>

          <el-col :lg="24">
            <el-form-item :label="$t('fileView.storePath')" prop="storePath">
              <template #label>
                <span>
                  <el-tooltip :content="$t('fileView.storePathTip')" placement="top">
                    <el-icon :size="15">
                      <questionFilled />
                    </el-icon>
                  </el-tooltip>
                  {{ $t('fileView.storePath') }}
                </span>
              </template>
              <!-- <el-input v-model="form.storePath" placeholder="请输入文件目录，默认yyyy/MMdd格式" clearable="" auto-complete="" /> -->
              <el-select
                style="width: 100%"
                v-model="form.storePath"
                allow-create
                clearable
                filterable
                default-first-option
                :reserve-keyword="false"
                :placeholder="$t('fileView.storePathPh')">
                <el-option v-for="item in saveDirOptions" :key="item.dictValue" :label="item.dictLabel" :value="item.dictValue" />
              </el-select>
            </el-form-item>
          </el-col>

          <el-col :lg="24" v-if="form.fileNameType == 2">
            <el-form-item :label="$t('fileView.customFileName')" prop="fileName">
              <el-input v-model="form.fileName" :placeholder="$t('fileView.fileNamePh')" clearable="" />
            </el-form-item>
          </el-col>
          <el-col :lg="24">
            <UploadFile
              ref="uploadRef"
              v-model="form.accessUrl"
              :fileType="[]"
              :fileSize="100"
              :drag="true"
              :data="uploadData"
              :autoUpload="false"
              @success="handleUploadSuccess" />
          </el-col>
        </el-row>
      </el-form>
      <template #footer>
        <div class="dialog-footer">
          <el-button text @click="cancel">{{ $t('btn.cancel') }}</el-button>
          <el-button type="primary" @click="submitUpload">{{ $t('btn.submit') }}</el-button>
        </div>
      </template>
    </el-dialog>

    <el-dialog :lock-scroll="false" v-model="openView" draggable="">
      <el-form ref="form" :model="formView" :rules="rules" label-width="90px" label-position="left">
        <el-row>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.classifyType')">
              <dict-tag :options="classifyTypeOptions" :value="formView.classifyType" />
            </el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.realName')">{{ formView.realName }}</el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.fileType')">
              <el-tag>{{ formView.fileType }}</el-tag>
            </el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.fileExt')">
              <el-tag>{{ formView.fileExt }}</el-tag>
            </el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.fileName')">{{ formView.fileName }}</el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.storePath')">{{ formView.storePath }}</el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.fileSize')">{{ formView.fileSize }}</el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('common.createBy')">{{ formView.create_by }}</el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('fileView.qrCode')">
              <div ref="imgContainerRef" id="imgContainer" class="qrCode"></div>
            </el-form-item>
          </el-col>
          <el-col :lg="12">
            <el-form-item :label="$t('common.preview')">
              <el-image :src="formView.accessUrl" fit="contain" style="width: 100px"></el-image>
            </el-form-item>
          </el-col>
          <el-col :lg="24">
            <el-form-item :label="$t('fileView.accessUrl')">
              {{ formView.accessUrl }}
              <el-button class="copy-btn-main" icon="document-copy" text @click="copyText(formView.accessUrl)">
                {{ $t('btn.copy') }}
              </el-button>
            </el-form-item>
          </el-col>
          <el-col :lg="24">
            <el-form-item :label="$t('fileView.storeUrl')">
              <div>
                {{ formView.fileUrl }}
              </div>
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>
    </el-dialog>
  </div>
</template>
<script setup name="file">
import { listSysfile, delSysfile, getSysfile, updateSysfile } from '@/api/tool/file.js'
import { useClipboard } from '@vueuse/core'
import QRCode from 'qrcodejs2-fixes'
import { showTime } from '@/utils'
// 选中id数组
const ids = ref([])
// 非单个禁用
const single = ref(true)
// 非多个禁用
const multiple = ref(true)
// 遮罩层
const loading = ref(true)
// 显示搜索条件
const showSearch = ref(true)
// 弹出层标题
const title = ref('')
// 是否显示弹出层
const open = ref(false)
const openView = ref(false)
// 表单
const formRef = ref(null)
const formView = ref({})
const uploadRef = ref(null)
// 上传时间时间范围
const dateRangeAddTime = ref([])
// 存储类型选项列表
const { proxy } = getCurrentInstance()
const storeTypeOptions = ref([
  { dictLabel: proxy.$t('fileView.localStorage'), dictValue: 1 },
  { dictLabel: proxy.$t('fileView.aliyunStorage'), dictValue: 2 }
])
//文件名产生选项列表
const fileNameTypeOptions = ref([
  { dictLabel: proxy.$t('fileView.originalName'), dictValue: 1 },
  { dictLabel: proxy.$t('fileView.custom'), dictValue: 2 },
  { dictLabel: proxy.$t('fileView.autoGenerate'), dictValue: 3 }
])
// 存储目录前缀
const saveDirOptions = ref([
  { dictLabel: 'uploads', dictValue: 'uploads' },
  { dictLabel: 'video', dictValue: 'video' },
  { dictLabel: 'avatar', dictValue: 'avatar' }
])
const classifyTypeOptions = ref([])

// 数据列表
const dataList = ref([])
// 总记录数
const total = ref(0)

const state = reactive({
  form: {
    storeType: 1
  },
  rules: {
    accessUrl: [
      {
        required: true,
        message: proxy.$t('fileView.fileRequired'),
        trigger: 'blur'
      }
    ],
    storeType: [
      {
        required: true,
        message: proxy.$t('fileView.storeTypeRequired'),
        trigger: 'blur'
      }
    ],
    fileName: [
      {
        required: true,
        message: proxy.$t('fileView.fileNameRequired'),
        trigger: 'blur'
      }
    ]
  },
  queryParams: {
    pageNum: 1,
    pageSize: 20,
    storeType: 1, // 存储类型 1、本地 2、阿里云
    fileId: undefined
  }
})
const { queryParams, form, rules } = toRefs(state)
const uploadData = ref()
// 查询数据
function getList() {
  proxy.addDateRange(queryParams.value, dateRangeAddTime.value, 'Create_time')
  loading.value = true
  listSysfile(queryParams.value).then((res) => {
    if (res.code == 200) {
      dataList.value = res.data.result
      total.value = res.data.totalNum
      loading.value = false
    }
  })
}
proxy.getDicts('sys_classify_type').then((response) => {
  classifyTypeOptions.value = response.data
})
// 取消按钮
function cancel() {
  open.value = false
  reset()
}
// 重置数据表单
function reset() {
  form.value = {
    fileName: '',
    fileUrl: '',
    storePath: '',
    fileSize: 0,
    fileExt: '',
    storeType: 1,
    accessUrl: '',
    fileNameType: 3
  }
  proxy.resetForm('formRef')
}
/** 重置查询操作 */
function resetQuery() {
  // 上传时间时间范围
  dateRangeAddTime.value = []
  proxy.resetForm('queryForm')
  handleQuery()
}
// 多选框选中数据
function handleSelectionChange(selection) {
  ids.value = selection.map((item) => item.id)
  single.value = selection.length != 1
  multiple.value = !selection.length
}
/** 搜索按钮操作 */
function handleQuery() {
  queryParams.pageNum = 1
  getList()
}
/** 新增按钮操作 */
function handleAdd() {
  reset()
  open.value = true
  title.value = proxy.$t('fileView.uploadTitle')
  // form.value.storeType = queryParams.storeType
}
/** 删除按钮操作 */
function handleDelete(row) {
  const Ids = row.id || ids.value

  proxy
    .$confirm(proxy.$t('crud.deleteConfirm', { id: Ids }))
    .then(function () {
      return delSysfile(Ids)
    })
    .then(() => {
      handleQuery()
      proxy.$modal.msgSuccess(proxy.$t('crud.deleteSuccess'))
    })
    .catch(() => {})
}
/** 查看按钮操作 */
function handleView(row) {
  const id = row.id || ids.value
  getSysfile(id).then((res) => {
    const { code, data } = res
    if (code == 200) {
      openView.value = true
      formView.value = data
      proxy.$nextTick(() => {
        createQrCode(data.accessUrl)
      })
    }
  })
}
function createQrCode(url) {
  document.getElementById('imgContainer').innerHTML = ''
  new QRCode(document.getElementById('imgContainer'), {
    text: url,
    width: 100,
    height: 100
  })
}
// 上传成功方法
function handleUploadSuccess(filelist) {
  open.value = false
  getList()
}
// 手动上传
function submitUpload() {
  proxy.$refs['formRef'].validate((valid) => {
    if (valid) {
      var result = new Promise((resolve) => {
        uploadData.value = {
          fileDir: form.value.storePath,
          fileName: form.value.fileName,
          storeType: form.value.storeType,
          fileNameType: form.value.fileNameType
        }
        resolve(true)
      })
      //使用异步解决第一次上次获取不到表单的值
      result.then(() => {
        proxy.$refs.uploadRef.submitUpload()
      })
    }
  })
}
async function handleDown(item) {
  await proxy.downFile('/common/downloadFile', { fileId: item.id })
}
const { copy, isSupported } = useClipboard()
const copyText = async (val) => {
  if (isSupported) {
    copy(val)
    proxy.$modal.msgSuccess(proxy.$t('common.copySuccess'))
  } else {
    proxy.$modal.msgError(proxy.$t('common.browserNotSupport'))
  }
}
function handleClassifyChange(row) {
  console.log(row)

  updateSysfile(row).then(() => {
    proxy.$modal.msgSuccess(proxy.$t('crud.editSuccess'))
  })
}
handleQuery()
</script>
<style scoped>
.el-avatar {
  display: inline-block;
  text-align: center;
  background: #ccc;
  color: #fff;
  white-space: nowrap;
  position: relative;
  overflow: hidden;
  vertical-align: middle;
  width: 32px;
  height: 32px;
  line-height: 32px;
  border-radius: 16px;
}
.qrCode {
  border: 5px solid var(--el-color-primary);
}
</style>
