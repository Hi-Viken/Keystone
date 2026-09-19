<template>
  <div class="app-container">
    <!-- 搜索栏 -->
    <el-form :model="queryParams" ref="queryForm" :inline="true" v-show="showSearch">
      <el-form-item label="节点名称" prop="nodeName">
        <el-input v-model="queryParams.nodeName" placeholder="请输入节点名称" @keyup.enter="handleQuery" />
      </el-form-item>
      <el-form-item label="状态" prop="isEnable">
        <el-select v-model="queryParams.isEnable" placeholder="节点状态" clearable>
          <el-option label="启用" :value="true" />
          <el-option label="禁用" :value="false" />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" icon="search" @click="handleQuery">{{ $t('btn.search') }}</el-button>
        <el-button icon="refresh" @click="resetQuery">{{ $t('btn.reset') }}</el-button>
      </el-form-item>
    </el-form>

    <!-- 工具栏 -->
    <el-row :gutter="10" class="mb8">
      <el-col :span="1.5">
        <el-button type="primary" plain icon="plus" @click="handleAdd" v-hasPermi="['frp:node:add']">{{ $t('btn.add') }}</el-button>
      </el-col>
      <el-col :span="1.5" v-show="viewMode === 'table'">
        <el-button type="danger" plain icon="delete" :disabled="multiple" @click="handleDelete" v-hasPermi="['frp:node:remove']">
          {{ $t('btn.delete') }}
        </el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button type="warning" plain icon="download" @click="handleExport" v-hasPermi="['frp:node:export']">{{ $t('btn.export') }}</el-button>
      </el-col>
      <right-toolbar v-model:showSearch="showSearch" @queryTable="getList">
        <el-button-group style="margin-right: 10px;">
          <el-button :type="viewMode === 'card' ? 'primary' : ''" @click="viewMode = 'card'" title="卡片视图">
            <el-icon><Grid /></el-icon>
          </el-button>
          <el-button :type="viewMode === 'table' ? 'primary' : ''" @click="viewMode = 'table'" title="表格视图">
            <el-icon><List /></el-icon>
          </el-button>
        </el-button-group>
      </right-toolbar>
    </el-row>

    <!-- 卡片视图 -->
    <div v-loading="loading" v-show="viewMode === 'card'">
      <el-row :gutter="14" v-if="nodeList.length > 0">
        <el-col :xs="12" :sm="8" :md="6" :lg="6" :xl="4" v-for="node in nodeList" :key="node.nodeId" class="card-col">
          <div class="node-card" :class="{ disabled: !node.isEnable }">
            <!-- 头部 -->
            <div class="card-head">
              <div v-if="getIsp(node.nodeName)" class="isp-logo" :style="{ background: getIsp(node.nodeName).color }">
                <span class="isp-char">{{ getIsp(node.nodeName).short.charAt(0) }}</span>
              </div>
              <div v-else class="card-avatar" :class="node.isEnable ? 'av-on' : 'av-off'">
                {{ node.nodeName ? node.nodeName.charAt(0).toUpperCase() : 'N' }}
              </div>
              <div class="card-head-info">
                <div class="card-name-row">
                  <span class="card-name" :title="node.nodeName">{{ node.nodeName }}</span>
                  <span v-if="getIsp(node.nodeName)" class="isp-badge" :style="{ color: getIsp(node.nodeName).color, borderColor: getIsp(node.nodeName).color + '40', background: getIsp(node.nodeName).color + '12' }">
                    {{ getIsp(node.nodeName).short }}
                  </span>
                </div>
                <el-tag :type="node.isEnable ? 'success' : 'info'" size="small" round effect="plain">
                  {{ node.isEnable ? '启用' : '禁用' }}
                </el-tag>
              </div>
              <div class="card-head-actions">
                <el-switch
                  v-model="node.isEnable"
                  :active-value="true"
                  :inactive-value="false"
                  size="small"
                  inline-prompt
                  @change="handleStatusChange(node)"
                  v-hasPermi="['frp:node:edit']" />
                <el-button link type="primary" @click="handleUpdate(node)" v-hasPermi="['frp:node:edit']">
                  <el-icon :size="15"><Edit /></el-icon>
                </el-button>
                <el-button link type="danger" @click="handleDelete(node)" v-hasPermi="['frp:node:remove']">
                  <el-icon :size="15"><Delete /></el-icon>
                </el-button>
              </div>
            </div>

            <!-- 令牌 -->
            <div class="card-token" @click="toggleToken(node)">
              <span class="token-label">令牌</span>
              <code class="token-val">{{ revealedTokens[node.nodeId] !== undefined ? revealedTokens[node.nodeId] : '••••••••' }}</code>
              <el-icon class="token-eye-icon" :size="14">
                <Hide v-if="revealedTokens[node.nodeId] !== undefined" />
                <View v-else />
              </el-icon>
            </div>

            <!-- 信息网格 -->
            <div class="card-meta">
              <div class="meta-item">
                <span class="meta-label">创建人</span>
                <span class="meta-value">{{ node.createBy || '-' }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">创建时间</span>
                <span class="meta-value">{{ parseTime(node.createTime) || '-' }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">修改人</span>
                <span class="meta-value">{{ node.updateBy || '-' }}</span>
              </div>
              <div class="meta-item">
                <span class="meta-label">修改时间</span>
                <span class="meta-value">{{ parseTime(node.updateTime) || '-' }}</span>
              </div>
            </div>

            <!-- 备注 -->
            <div class="card-remark" v-if="node.remark">
              <span class="remark-text">{{ node.remark }}</span>
            </div>
          </div>
        </el-col>
      </el-row>
      <el-empty v-else description="暂无数据" />
    </div>

    <!-- 表格视图 -->
    <div v-show="viewMode === 'table'">
      <el-table v-loading="loading" :data="nodeList" @selection-change="handleSelectionChange">
        <el-table-column type="selection" width="50" align="center" />
        <el-table-column label="编号" align="center" prop="nodeId" sortable width="70" />
        <el-table-column label="节点名称" align="center" prop="nodeName" min-width="160">
          <template #default="scope">
            <div class="tbl-name-cell">
              <span v-if="getIsp(scope.row.nodeName)" class="tbl-isp-dot" :style="{ background: getIsp(scope.row.nodeName).color }"></span>
              <span>{{ scope.row.nodeName }}</span>
              <span v-if="getIsp(scope.row.nodeName)" class="tbl-isp-tag" :style="{ color: getIsp(scope.row.nodeName).color, background: getIsp(scope.row.nodeName).color + '15' }">
                {{ getIsp(scope.row.nodeName).short }}
              </span>
            </div>
          </template>
        </el-table-column>
        <el-table-column label="令牌" align="center" width="220">
          <template #default="scope">
            <code class="tbl-token">{{ revealedTokens[scope.row.nodeId] !== undefined ? revealedTokens[scope.row.nodeId] : '••••••••' }}</code>
            <el-button link size="small" @click="toggleToken(scope.row)" style="margin-left: 4px;">
              <el-icon v-if="revealedTokens[scope.row.nodeId] !== undefined" color="#409eff"><Hide /></el-icon>
              <el-icon v-else color="#909399"><View /></el-icon>
            </el-button>
          </template>
        </el-table-column>
        <el-table-column label="状态" align="center" width="80">
          <template #default="scope">
            <el-switch v-model="scope.row.isEnable" :active-value="true" :inactive-value="false" @change="handleStatusChange(scope.row)" />
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
        <el-table-column label="操作" align="center" width="120" fixed="right">
          <template #default="scope">
            <el-button text size="small" icon="edit" @click="handleUpdate(scope.row)" v-hasPermi="['frp:node:edit']">{{ $t('btn.edit') }}</el-button>
            <el-button text size="small" icon="delete" @click="handleDelete(scope.row)" v-hasPermi="['frp:node:remove']">{{ $t('btn.delete') }}</el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <pagination v-show="total > 0" :total="total" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />

    <!-- 添加或修改节点对话框 -->
    <el-dialog :title="title" v-model="open" width="520px">
      <el-form ref="formRef" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="节点名称" prop="nodeName">
          <el-input v-model="form.nodeName" placeholder="请输入节点名称" />
        </el-form-item>
        <el-form-item label="令牌" prop="tocken">
          <div style="display: flex; width: 100%; gap: 8px;">
            <el-input v-model="form.tocken" placeholder="请输入或生成令牌" style="flex: 1;" />
            <el-button type="primary" :loading="generating" @click="handleGenerateToken">
              <el-icon v-if="!generating" style="margin-right: 4px;"><MagicStick /></el-icon>
              生成
            </el-button>
          </div>
        </el-form-item>
        <el-form-item label="状态" prop="isEnable">
          <el-radio-group v-model="form.isEnable">
            <el-radio :value="true">启用</el-radio>
            <el-radio :value="false">禁用</el-radio>
          </el-radio-group>
        </el-form-item>
        <el-form-item label="备注" prop="remark">
          <el-input v-model="form.remark" type="textarea" placeholder="请输入内容" />
        </el-form-item>
      </el-form>
      <template #footer>
        <el-button text @click="cancel">{{ $t('btn.cancel') }}</el-button>
        <el-button type="primary" @click="submitForm">{{ $t('btn.submit') }}</el-button>
      </template>
    </el-dialog>
  </div>
</template>

<script setup name="node">
import { listNode, getNode, delNode, addNode, updateNode, changeNodeStatus, generateToken, getNodeToken, exportNode } from '@/api/frp/node'
import { View, Hide, Grid, List, MagicStick, Edit, Delete } from '@element-plus/icons-vue'

const { proxy } = getCurrentInstance()
const loading = ref(true)
const ids = ref([])
const single = ref(true)
const multiple = ref(true)
const showSearch = ref(true)
const total = ref(0)
const nodeList = ref([])
const title = ref('')
const open = ref(false)
const generating = ref(false)
const viewMode = ref('card')
const revealedTokens = reactive({})

/** 运营商识别配置 */
const ISP_LIST = [
  { keywords: ['电信', 'CT', 'ChinaTelecom', 'chinat'], name: '电信', short: '电信', color: '#005AAA' },
  { keywords: ['联通', 'CU', 'ChinaUnicom', 'chinaun'], name: '联通', short: '联通', color: '#E60012' },
  { keywords: ['移动', 'CMCC', 'ChinaMobile', 'chinamobile'], name: '移动', short: '移动', color: '#00835E' },
  { keywords: ['广电', 'CBN', 'guangdian'], name: '广电', short: '广电', color: '#1D6FF2' },
  { keywords: ['铁通', 'CTT'], name: '铁通', short: '铁通', color: '#2B7A3D' },
  { keywords: ['长城', 'GWB', 'greatwall'], name: '长城', short: '长城', color: '#B8860B' },
  { keywords: ['BGP'], name: 'BGP', short: 'BGP', color: '#6C3483' },
  { keywords: ['CN2', 'GIA'], name: 'CN2', short: 'CN2', color: '#D4AC0D' },
  { keywords: ['AWS'], name: 'AWS', short: 'AWS', color: '#FF9900' },
  { keywords: ['阿里', 'aliyun', 'alicloud'], name: '阿里', short: '阿里', color: '#FF6A00' },
  { keywords: ['腾讯', 'tencent'], name: '腾讯', short: '腾讯', color: '#00A4FF' },
  { keywords: ['华为', 'huawei'], name: '华为', short: '华为', color: '#CF0A2C' },
]

function getIsp(nodeName) {
  if (!nodeName) return null
  const lower = nodeName.toLowerCase()
  return ISP_LIST.find(isp => isp.keywords.some(kw => lower.includes(kw.toLowerCase()))) || null
}

let queryParams = reactive({
  pageNum: 1,
  pageSize: 12,
  nodeName: undefined,
  isEnable: undefined
})

const state = reactive({ form: {}, rules: {} })
const formRef = ref(null)
const { form, rules } = toRefs(state)

function getRules() {
  const base = { nodeName: [{ required: true, message: '节点名称不能为空', trigger: 'blur' }] }
  if (!form.value.nodeId) {
    base.tocken = [{ required: true, message: '令牌不能为空，请点击生成', trigger: 'blur' }]
  }
  return base
}

function getList() {
  loading.value = true
  listNode(queryParams).then((response) => {
    nodeList.value = response.data.result
    total.value = response.data.totalNum
    loading.value = false
  })
}

function cancel() { open.value = false; reset() }

function reset() {
  form.value = { nodeId: undefined, nodeName: undefined, tocken: undefined, isEnable: true, remark: undefined }
  proxy.resetForm('formRef')
}

function handleQuery() { queryParams.pageNum = 1; getList() }

function resetQuery() { proxy.resetForm('queryForm'); handleQuery() }

function handleSelectionChange(selection) {
  ids.value = selection.map((item) => item.nodeId)
  single.value = selection.length != 1
  multiple.value = !selection.length
}

function toggleToken(node) {
  if (revealedTokens[node.nodeId] !== undefined) {
    delete revealedTokens[node.nodeId]
  } else {
    getNodeToken(node.nodeId).then((response) => {
      revealedTokens[node.nodeId] = response.data.token
    })
  }
}

function handleGenerateToken() {
  generating.value = true
  generateToken().then((response) => { form.value.tocken = response.data.token }).finally(() => { generating.value = false })
}

function handleAdd() { reset(); state.rules = getRules(); open.value = true; title.value = '添加节点' }

function handleUpdate(row) {
  reset()
  const nodeId = row.nodeId || ids.value
  getNode(nodeId).then((response) => {
    form.value = response.data
    form.value.tocken = undefined
    state.rules = getRules()
    open.value = true
    title.value = '修改节点'
  })
}

function handleStatusChange(row) {
  const text = row.isEnable ? '启用' : '禁用'
  proxy.$confirm('确认要"' + text + '""' + row.nodeName + '"节点吗?', '警告', {
    confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning'
  }).then(() => changeNodeStatus(row.nodeId, row.isEnable))
    .then(() => { proxy.$modal.msgSuccess(text + '成功') })
    .catch(() => { row.isEnable = !row.isEnable })
}

function submitForm() {
  proxy.$refs['formRef'].validate((valid) => {
    if (valid) {
      if (form.value.nodeId != undefined) {
        updateNode(form.value).then(() => { proxy.$modal.msgSuccess('修改成功'); open.value = false; getList() })
      } else {
        addNode(form.value).then(() => { proxy.$modal.msgSuccess('新增成功'); open.value = false; getList() })
      }
    }
  })
}

function handleDelete(row) {
  const nodeIds = row.nodeId || ids.value
  proxy.$confirm('是否确认删除节点编号为"' + nodeIds + '"的数据项?', '警告', {
    confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning'
  }).then(() => delNode(nodeIds)).then(() => { getList(); proxy.$modal.msgSuccess('删除成功') })
}

function handleExport() {
  proxy.$confirm('是否确认导出所有节点数据项?', '警告', {
    confirmButtonText: '确定', cancelButtonText: '取消', type: 'warning'
  }).then(async () => { await exportNode(queryParams) })
}

handleQuery()
</script>

<style scoped>
.card-col { margin-bottom: 14px; }

/* ===== 卡片主体 ===== */
.node-card {
  background: var(--el-bg-color);
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 10px;
  padding: 16px;
  transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
  position: relative;
  overflow: hidden;
}
.node-card::before {
  content: '';
  position: absolute;
  top: 0; left: 0;
  width: 4px; height: 100%;
  background: var(--el-color-success);
  border-radius: 10px 0 0 10px;
  transition: width 0.3s;
}
.node-card.disabled::before {
  background: var(--el-color-info-light-3);
}
.node-card:hover {
  border-color: var(--el-color-primary-light-3);
  box-shadow: 0 6px 24px rgba(0, 0, 0, 0.08);
  transform: translateY(-3px);
}
.node-card:hover::before { width: 6px; }
.node-card.disabled { opacity: 0.55; }
.node-card.disabled:hover { opacity: 0.75; }

/* 头部：头像 + 名称 + 操作 */
.card-head {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 14px;
}
.card-avatar {
  width: 36px; height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: 700;
  font-size: 16px;
  color: #fff;
  flex-shrink: 0;
  letter-spacing: 0;
}
.av-on  { background: linear-gradient(135deg, #67c23a, #529b2e); }
.av-off { background: linear-gradient(135deg, #a0a0a0, #808080); }
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
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}
.card-head-actions {
  display: flex;
  align-items: center;
  gap: 2px;
  flex-shrink: 0;
}

/* 令牌条 */
.card-token {
  display: flex;
  align-items: center;
  gap: 6px;
  background: var(--el-fill-color-light);
  border: 1px solid var(--el-border-color-extra-light);
  border-radius: 6px;
  padding: 6px 10px;
  margin-bottom: 12px;
  cursor: pointer;
  transition: background 0.2s;
}
.card-token:hover {
  background: var(--el-fill-color);
}
.token-label {
  font-size: 11px;
  color: var(--el-text-color-secondary);
  flex-shrink: 0;
}
.token-val {
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
.token-eye-icon {
  color: var(--el-text-color-secondary);
  flex-shrink: 0;
}

/* 信息网格 */
.card-meta {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 8px 12px;
  margin-bottom: 8px;
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

/* 备注 */
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

/* 表格内令牌样式 */
.tbl-token {
  font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;
  font-size: 12px;
  background: var(--el-fill-color-light);
  padding: 2px 6px;
  border-radius: 3px;
  letter-spacing: 0.5px;
}

/* ===== 运营商标识 ===== */
.isp-logo {
  width: 36px; height: 36px;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.12);
}
.isp-char {
  font-weight: 700;
  font-size: 15px;
  color: #fff;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.15);
}
.isp-badge {
  font-size: 10px;
  font-weight: 600;
  padding: 1px 6px;
  border-radius: 3px;
  border: 1px solid;
  line-height: 1.4;
  flex-shrink: 0;
  white-space: nowrap;
}
.card-name-row {
  display: flex;
  align-items: center;
  gap: 6px;
  min-width: 0;
}

/* 表格运营商样式 */
.tbl-name-cell {
  display: inline-flex;
  align-items: center;
  gap: 4px;
}
.tbl-isp-dot {
  display: inline-block;
  width: 8px; height: 8px;
  border-radius: 50%;
  flex-shrink: 0;
}
.tbl-isp-tag {
  font-size: 10px;
  font-weight: 600;
  padding: 0 5px;
  border-radius: 3px;
  line-height: 1.6;
  white-space: nowrap;
}
</style>
