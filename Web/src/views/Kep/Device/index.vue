<template>
  <div class="app-container">
    <!-- 搜索栏 -->
    <el-form :model="queryParams" ref="queryForm" :inline="true" v-show="showSearch">
      <el-form-item label="设备SN" prop="sn">
        <el-input v-model="queryParams.sn" placeholder="请输入设备SN" @keyup.enter="handleQuery" clearable />
      </el-form-item>
      <el-form-item label="固件版本" prop="firmwareVersion">
        <el-input v-model="queryParams.firmwareVersion" placeholder="请输入固件版本" @keyup.enter="handleQuery" clearable />
      </el-form-item>
      <el-form-item label="硬件版本" prop="hardwareVersion">
        <el-input v-model="queryParams.hardwareVersion" placeholder="请输入硬件版本" @keyup.enter="handleQuery" clearable />
      </el-form-item>
      <el-form-item>
        <el-button type="primary" icon="search" @click="handleQuery">{{ $t('btn.search') }}</el-button>
        <el-button icon="refresh" @click="resetQuery">{{ $t('btn.reset') }}</el-button>
      </el-form-item>
    </el-form>

    <!-- 工具栏 -->
    <el-row :gutter="10" class="mb8">
      <el-col :span="1.5">
        <el-button type="primary" plain icon="plus" @click="handleAdd" v-hasPermi="['kep:device:add']">{{ $t('btn.add') }}</el-button>
      </el-col>
      <el-col :span="1.5" v-show="viewMode === 'table'">
        <el-button type="danger" plain icon="delete" :disabled="multiple" @click="handleDelete" v-hasPermi="['kep:device:remove']">
          {{ $t('btn.delete') }}
        </el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button type="warning" plain icon="download" @click="handleExport" v-hasPermi="['kep:device:export']">{{ $t('btn.export') }}</el-button>
      </el-col>
      <right-toolbar v-model:showSearch="showSearch" @queryTable="getList">
        <el-button-group style="margin-right: 10px;">
          <el-button :type="viewMode === 'table' ? 'primary' : ''" @click="viewMode = 'table'" title="表格视图">
            <el-icon><List /></el-icon>
          </el-button>
          <el-button :type="viewMode === 'card' ? 'primary' : ''" @click="viewMode = 'card'" title="卡片视图">
            <el-icon><Grid /></el-icon>
          </el-button>
        </el-button-group>
      </right-toolbar>
    </el-row>

    <!-- 卡片视图 -->
    <div v-loading="loading" v-show="viewMode === 'card'">
      <el-row :gutter="14" v-if="deviceList.length > 0">
        <el-col :xs="12" :sm="8" :md="6" :lg="6" :xl="4" v-for="device in deviceList" :key="device.id" class="card-col">
          <div class="device-card">
            <!-- 头部：SN + 版本标签 -->
            <div class="card-head">
              <div class="card-head-info">
                <span class="card-name" :title="device.sn">{{ device.sn }}</span>
                <span class="card-sub">
                  <el-tag size="small" effect="plain" type="info" class="ver-tag">FW {{ device.firmwareVersion || '-' }}</el-tag>
                  <el-tag size="small" effect="plain" type="info" class="ver-tag">HW {{ device.hardwareVersion || '-' }}</el-tag>
                </span>
              </div>
            </div>

            <!-- Token -->
            <div class="card-secret-row" @click="toggleListToken(device)">
              <span class="secret-label">Token</span>
              <code class="secret-code">{{ revealedTokens[device.id] !== undefined ? revealedTokens[device.id] : '••••••••' }}</code>
              <el-icon class="secret-eye" :size="14">
                <Hide v-if="revealedTokens[device.id] !== undefined" />
                <View v-else />
              </el-icon>
            </div>

            <!-- Secret -->
            <div class="card-secret-row" @click="toggleListSecret(device)">
              <span class="secret-label">Secret</span>
              <code class="secret-code">{{ revealedSecrets[device.id] !== undefined ? revealedSecrets[device.id] : '••••••••' }}</code>
              <el-icon class="secret-eye" :size="14">
                <Hide v-if="revealedSecrets[device.id] !== undefined" />
                <View v-else />
              </el-icon>
            </div>

            <!-- 信息网格 -->
            <div class="card-meta">
              <div class="meta-item">
                <span class="meta-label">最后上线</span>
                <span class="meta-value">{{ device.lastOnlineAt ? parseTime(device.lastOnlineAt) : '-' }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">最后离线</span>
                <span class="meta-value">{{ device.lastOfflineAt ? parseTime(device.lastOfflineAt) : '-' }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">创建人</span>
                <span class="meta-value">{{ device.createBy || '-' }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">修改人</span>
                <span class="meta-value">{{ device.updateBy || '-' }}</span>
              </div>
            </div>

            <!-- 备注 -->
            <div class="card-remark" v-if="device.remark">
              <span class="remark-text">{{ device.remark }}</span>
            </div>

            <!-- 底部操作按钮 -->
            <div class="card-footer">
              <el-button text type="primary" size="small" @click="handleView(device)" v-hasPermi="['kep:device:query']">
                <el-icon style="margin-right: 2px;"><View /></el-icon> 查看
              </el-button>
              <el-button text type="primary" size="small" @click="handleUpdate(device)" v-hasPermi="['kep:device:edit']">
                <el-icon style="margin-right: 2px;"><Edit /></el-icon> 编辑
              </el-button>
              <el-button text type="danger" size="small" @click="handleDelete(device)" v-hasPermi="['kep:device:remove']">
                <el-icon style="margin-right: 2px;"><Delete /></el-icon> 删除
              </el-button>
            </div>
          </div>
        </el-col>
      </el-row>
      <el-empty v-else description="暂无设备数据" :image-size="120" />
    </div>

    <!-- 表格视图 -->
    <div v-show="viewMode === 'table'">
    <el-table v-loading="loading" :data="deviceList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="50" align="center" />
      <el-table-column label="ID" align="center" prop="id" sortable width="70" />
      <el-table-column label="设备SN" align="center" prop="sn" min-width="150" :show-overflow-tooltip="true" />
      <el-table-column label="Token" align="center" width="200">
        <template #default="scope">
          <code class="tbl-secret">{{ revealedTokens[scope.row.id] !== undefined ? revealedTokens[scope.row.id] : '••••••••' }}</code>
          <el-button link size="small" @click="toggleListToken(scope.row)" style="margin-left: 4px;">
            <el-icon v-if="revealedTokens[scope.row.id] !== undefined" color="#409eff"><Hide /></el-icon>
            <el-icon v-else color="#909399"><View /></el-icon>
          </el-button>
        </template>
      </el-table-column>
      <el-table-column label="Secret" align="center" width="200">
        <template #default="scope">
          <code class="tbl-secret">{{ revealedSecrets[scope.row.id] !== undefined ? revealedSecrets[scope.row.id] : '••••••••' }}</code>
          <el-button link size="small" @click="toggleListSecret(scope.row)" style="margin-left: 4px;">
            <el-icon v-if="revealedSecrets[scope.row.id] !== undefined" color="#409eff"><Hide /></el-icon>
            <el-icon v-else color="#909399"><View /></el-icon>
          </el-button>
        </template>
      </el-table-column>
      <el-table-column label="固件版本" align="center" prop="firmwareVersion" width="120" :show-overflow-tooltip="true" />
      <el-table-column label="硬件版本" align="center" prop="hardwareVersion" width="120" :show-overflow-tooltip="true" />
      <el-table-column label="最后上线" align="center" prop="lastOnlineAt" width="160" sortable>
        <template #default="scope">
          <el-tag v-if="scope.row.lastOnlineAt" type="success" size="small" effect="plain">
            {{ parseTime(scope.row.lastOnlineAt) }}
          </el-tag>
          <span v-else class="text-placeholder">-</span>
        </template>
      </el-table-column>
      <el-table-column label="最后离线" align="center" prop="lastOfflineAt" width="160" sortable>
        <template #default="scope">
          <el-tag v-if="scope.row.lastOfflineAt" type="info" size="small" effect="plain">
            {{ parseTime(scope.row.lastOfflineAt) }}
          </el-tag>
          <span v-else class="text-placeholder">-</span>
        </template>
      </el-table-column>
      <el-table-column label="创建人" align="center" prop="createBy" width="100" :show-overflow-tooltip="true" />
      <el-table-column label="创建时间" align="center" prop="createTime" width="160" sortable>
        <template #default="scope"><span>{{ parseTime(scope.row.createTime) }}</span></template>
      </el-table-column>
      <el-table-column label="修改人" align="center" prop="updateBy" width="100" :show-overflow-tooltip="true" />
      <el-table-column label="修改时间" align="center" prop="updateTime" width="160" sortable>
        <template #default="scope"><span>{{ parseTime(scope.row.updateTime) || '-' }}</span></template>
      </el-table-column>
      <el-table-column label="备注" align="center" prop="remark" width="130" :show-overflow-tooltip="true" />
      <el-table-column label="操作" align="center" width="180" fixed="right">
        <template #default="scope">
          <el-button text size="small" icon="view" @click="handleView(scope.row)" v-hasPermi="['kep:device:query']">查看</el-button>
          <el-button text size="small" icon="edit" @click="handleUpdate(scope.row)" v-hasPermi="['kep:device:edit']">{{ $t('btn.edit') }}</el-button>
          <el-button text size="small" icon="delete" @click="handleDelete(scope.row)" v-hasPermi="['kep:device:remove']">{{ $t('btn.delete') }}</el-button>
        </template>
      </el-table-column>
    </el-table>
    </div>

    <pagination v-show="total > 0" :total="total" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />

    <!-- 查看设备详情对话框 -->
    <el-dialog title="设备详情" v-model="detailOpen" width="600px" destroy-on-close>
      <el-descriptions :column="2" border v-if="detailData">
        <el-descriptions-item label="设备ID">{{ detailData.id }}</el-descriptions-item>
        <el-descriptions-item label="设备SN">{{ detailData.sn }}</el-descriptions-item>
        <el-descriptions-item label="固件版本">{{ detailData.firmwareVersion || '-' }}</el-descriptions-item>
        <el-descriptions-item label="硬件版本">{{ detailData.hardwareVersion || '-' }}</el-descriptions-item>
        <el-descriptions-item label="最后上线">
          <el-tag v-if="detailData.lastOnlineAt" type="success" size="small" effect="plain">
            {{ parseTime(detailData.lastOnlineAt) }}
          </el-tag>
          <span v-else>-</span>
        </el-descriptions-item>
        <el-descriptions-item label="最后离线">
          <el-tag v-if="detailData.lastOfflineAt" type="info" size="small" effect="plain">
            {{ parseTime(detailData.lastOfflineAt) }}
          </el-tag>
          <span v-else>-</span>
        </el-descriptions-item>
        <el-descriptions-item label="Token" :span="2">
          <div class="secret-field">
            <code class="secret-val">{{ showDetailToken ? detailSecrets.token : '••••••••••••' }}</code>
            <el-button link size="small" @click="toggleDetailToken" style="margin-left: 8px;">
              <el-icon v-if="showDetailToken" color="#409eff"><Hide /></el-icon>
              <el-icon v-else color="#909399"><View /></el-icon>
            </el-button>
          </div>
        </el-descriptions-item>
        <el-descriptions-item label="Secret" :span="2">
          <div class="secret-field">
            <code class="secret-val">{{ showDetailSecret ? detailSecrets.secret : '••••••••••••' }}</code>
            <el-button link size="small" @click="toggleDetailSecret" style="margin-left: 8px;">
              <el-icon v-if="showDetailSecret" color="#409eff"><Hide /></el-icon>
              <el-icon v-else color="#909399"><View /></el-icon>
            </el-button>
          </div>
        </el-descriptions-item>
        <el-descriptions-item label="创建人">{{ detailData.createBy || '-' }}</el-descriptions-item>
        <el-descriptions-item label="创建时间">{{ parseTime(detailData.createTime) || '-' }}</el-descriptions-item>
        <el-descriptions-item label="修改人">{{ detailData.updateBy || '-' }}</el-descriptions-item>
        <el-descriptions-item label="修改时间">{{ parseTime(detailData.updateTime) || '-' }}</el-descriptions-item>
        <el-descriptions-item label="备注" :span="2">{{ detailData.remark || '-' }}</el-descriptions-item>
      </el-descriptions>
      <template #footer>
        <el-button @click="detailOpen = false">关闭</el-button>
      </template>
    </el-dialog>

    <!-- 添加或修改设备对话框 -->
    <el-dialog :title="title" v-model="open" width="560px" destroy-on-close>
      <el-form ref="formRef" :model="form" :rules="rules" label-width="100px">
        <el-form-item label="设备SN" prop="sn">
          <el-input v-model="form.sn" placeholder="请输入设备唯一码" />
        </el-form-item>
        <el-form-item label="Token" prop="token">
          <el-input v-model="form.token" placeholder="请输入设备密码" show-password />
        </el-form-item>
        <el-form-item label="Secret" prop="secret">
          <el-input v-model="form.secret" placeholder="请输入设备私钥" show-password />
        </el-form-item>
        <el-form-item label="固件版本" prop="firmwareVersion">
          <el-input v-model="form.firmwareVersion" placeholder="请输入固件版本" />
        </el-form-item>
        <el-form-item label="硬件版本" prop="hardwareVersion">
          <el-input v-model="form.hardwareVersion" placeholder="请输入硬件版本" />
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input v-model="form.remark" type="textarea" placeholder="请输入备注" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button text @click="cancel">{{ $t('btn.cancel') }}</el-button>
        <el-button type="primary" @click="submitForm">{{ $t('btn.submit') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup name="device">
import { listDevice, getDevice, getDeviceToken, getDeviceSecret, addDevice, updateDevice, delDevice, exportDevice } from '@/api/Kep/Device'
import { View, Hide, Grid, List, Edit, Delete } from '@element-plus/icons-vue'

const { proxy } = getCurrentInstance()
const loading = ref(true)
const ids = ref([])
const single = ref(true)
const multiple = ref(true)
const showSearch = ref(true)
const total = ref(0)
const deviceList = ref([])
const title = ref('')
const open = ref(false)
const detailOpen = ref(false)
const detailData = ref(null)
const detailSecrets = reactive({ token: '', secret: '' })
const showDetailToken = ref(false)
const showDetailSecret = ref(false)
const revealedTokens = reactive({})
const revealedSecrets = reactive({})
const viewMode = ref('table')

let queryParams = reactive({
  pageNum: 1,
  pageSize: 12,
  sn: undefined,
  firmwareVersion: undefined,
  hardwareVersion: undefined
})

const state = reactive({ form: {}, rules: {} })
const formRef = ref(null)
const { form, rules } = toRefs(state)

function getRules() {
  const base = {
    sn: [{ required: true, message: '设备SN不能为空', trigger: 'blur' }]
  }
  if (!form.value.id) {
    base.token = [{ required: true, message: 'Token不能为空', trigger: 'blur' }]
    base.secret = [{ required: true, message: 'Secret不能为空', trigger: 'blur' }]
  }
  return base
}

function getList() {
  loading.value = true
  // 刷新列表时清除已揭示的密文
  Object.keys(revealedTokens).forEach(k => delete revealedTokens[k])
  Object.keys(revealedSecrets).forEach(k => delete revealedSecrets[k])
  listDevice(queryParams).then((response) => {
    deviceList.value = response.data.result
    total.value = response.data.totalNum
    loading.value = false
  })
}

function cancel() { open.value = false; reset() }

function reset() {
  form.value = {
    id: undefined,
    sn: undefined,
    token: undefined,
    secret: undefined,
    firmwareVersion: undefined,
    hardwareVersion: undefined,
    remark: undefined
  }
  proxy.resetForm('formRef')
}

function handleQuery() { queryParams.pageNum = 1; getList() }

function resetQuery() { proxy.resetForm('queryForm'); handleQuery() }

function handleSelectionChange(selection) {
  ids.value = selection.map((item) => item.id)
  single.value = selection.length != 1
  multiple.value = !selection.length
}

/** 列表中切换某行的Token明文显示 */
function toggleListToken(row) {
  if (revealedTokens[row.id] !== undefined) {
    delete revealedTokens[row.id]
  } else {
    getDeviceToken(row.id).then((response) => {
      revealedTokens[row.id] = response.data.token
    })
  }
}

/** 列表中切换某行的Secret明文显示 */
function toggleListSecret(row) {
  if (revealedSecrets[row.id] !== undefined) {
    delete revealedSecrets[row.id]
  } else {
    getDeviceSecret(row.id).then((response) => {
      revealedSecrets[row.id] = response.data.secret
    })
  }
}

/** 查看设备详情 */
function handleView(row) {
  detailData.value = null
  detailSecrets.token = ''
  detailSecrets.secret = ''
  showDetailToken.value = false
  showDetailSecret.value = false
  getDevice(row.id).then((response) => {
    detailData.value = response.data
    detailOpen.value = true
  })
}

/** 详情对话框切换Token显示 */
function toggleDetailToken() {
  if (showDetailToken.value) {
    showDetailToken.value = false
    detailSecrets.token = ''
  } else {
    getDeviceToken(detailData.value.id).then((res) => {
      detailSecrets.token = res.data.token
      showDetailToken.value = true
    })
  }
}

/** 详情对话框切换Secret显示 */
function toggleDetailSecret() {
  if (showDetailSecret.value) {
    showDetailSecret.value = false
    detailSecrets.secret = ''
  } else {
    getDeviceSecret(detailData.value.id).then((res) => {
      detailSecrets.secret = res.data.secret
      showDetailSecret.value = true
    })
  }
}

function handleAdd() {
  reset()
  state.rules = getRules()
  open.value = true
  title.value = '添加设备'
}

function handleUpdate(row) {
  reset()
  const deviceId = row.id || ids.value
  getDevice(deviceId).then((response) => {
    form.value = response.data
    // 从后端分别查出Token和Secret明文填入表单
    getDeviceToken(deviceId).then((res) => {
      form.value.token = res.data.token
    })
    getDeviceSecret(deviceId).then((res) => {
      form.value.secret = res.data.secret
    })
    state.rules = getRules()
    open.value = true
    title.value = '修改设备'
  })
}

function submitForm() {
  proxy.$refs['formRef'].validate((valid) => {
    if (valid) {
      if (form.value.id != undefined) {
        updateDevice(form.value).then(() => {
          proxy.$modal.msgSuccess('修改成功')
          open.value = false
          getList()
        })
      } else {
        addDevice(form.value).then(() => {
          proxy.$modal.msgSuccess('新增成功')
          open.value = false
          getList()
        })
      }
    }
  })
}

function handleDelete(row) {
  const deviceIds = row.id || ids.value
  proxy.$confirm('是否确认删除设备编号为"' + deviceIds + '"的数据项?', '警告', {
    confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning'
  }).then(() => delDevice(deviceIds)).then(() => { getList(); proxy.$modal.msgSuccess('删除成功') })
}

function handleExport() {
  proxy.$confirm('是否确认导出所有设备数据项?', '警告', {
    confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning'
  }).then(async () => { await exportDevice(queryParams) })
}

handleQuery()
</script>

<style scoped>
/* ===== 通用 ===== */
.text-placeholder {
  color: var(--el-text-color-placeholder);
}

/* ===== 详情对话框密文 ===== */
.secret-field {
  display: flex;
  align-items: center;
}
.secret-val {
  font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;
  font-size: 12px;
  background: var(--el-fill-color-light);
  padding: 3px 10px;
  border-radius: 4px;
  letter-spacing: 0.5px;
  word-break: break-all;
}

/* ===== 表格密文列 ===== */
.tbl-secret {
  font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;
  font-size: 12px;
  background: var(--el-fill-color-light);
  padding: 2px 8px;
  border-radius: 4px;
  letter-spacing: 0.5px;
}

/* ===== 卡片视图 ===== */
.card-col {
  margin-bottom: 14px;
}

.device-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 12px;
  padding: 16px;
  transition: all 0.35s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}
.device-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 4px;
  height: 100%;
  background: linear-gradient(180deg, #409eff 0%, #67c23a 100%);
  border-radius: 12px 0 0 12px;
  transition: width 0.3s;
}
.device-card:hover {
  border-color: var(--el-color-primary-light-3);
  box-shadow: 0 8px 30px rgba(0, 0, 0, 0.1);
  transform: translateY(-4px);
}
.device-card:hover::before {
  width: 6px;
}

/* 卡片头部 */
.card-head {
  display: flex;
  align-items: flex-start;
  gap: 10px;
  margin-bottom: 14px;
}
.card-head-info {
  flex: 1;
  min-width: 0;
  display: flex;
  flex-direction: column;
  gap: 4px;
}
.card-name {
  font-weight: 600;
  font-size: 14px;
  color: var(--el-text-color-primary);
  word-break: break-all;
  letter-spacing: 0.3px;
  line-height: 1.4;
}
.card-sub {
  display: flex;
  gap: 4px;
  align-items: center;
}
.ver-tag {
  font-size: 10px !important;
  height: 18px !important;
  padding: 0 5px !important;
  border-radius: 3px !important;
}

/* 卡片底部操作栏 */
.card-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 4px;
  margin-top: 12px;
  padding-top: 10px;
  border-top: 1px solid var(--el-border-color-extra-light);
}

/* 卡片密文行 */
.card-secret-row {
  display: flex;
  align-items: center;
  gap: 8px;
  background: var(--el-fill-color-lighter);
  border: 1px solid var(--el-border-color-extra-light);
  border-radius: 8px;
  padding: 7px 12px;
  margin-bottom: 8px;
  cursor: pointer;
  transition: all 0.2s ease;
}
.card-secret-row:hover {
  background: var(--el-fill-color);
  border-color: var(--el-color-primary-light-7);
}
.secret-label {
  font-size: 11px;
  color: var(--el-text-color-secondary);
  flex-shrink: 0;
  width: 40px;
  font-weight: 500;
}
.secret-code {
  font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;
  font-size: 12px;
  color: var(--el-text-color-regular);
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
  letter-spacing: 0.5px;
  background: none;
  padding: 0;
}
.secret-eye {
  color: var(--el-text-color-placeholder);
  flex-shrink: 0;
  transition: color 0.2s;
}
.card-secret-row:hover .secret-eye {
  color: var(--el-color-primary);
}

/* 卡片信息网格 */
.card-meta {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px 14px;
  margin-bottom: 8px;
  padding-top: 10px;
  border-top: 1px solid var(--el-border-color-extra-light);
}
.meta-item {
  display: flex;
  flex-direction: column;
  gap: 2px;
  min-width: 0;
}
.meta-label {
  font-size: 11px;
  color: var(--el-text-color-placeholder);
}
.meta-value {
  font-size: 12px;
  color: var(--el-text-color-regular);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

/* 卡片备注 */
.card-remark {
  padding-top: 8px;
  border-top: 1px dashed var(--el-border-color-extra-light);
}
.remark-text {
  font-size: 11px;
  color: var(--el-text-color-secondary);
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  line-height: 1.5;
}
</style>
