<template>
  <div class="app-container">
    <el-form :model="queryParams" ref="queryRef" :inline="true">
      <el-form-item>
        <el-button plain type="primary" @click="onLockAll()" icon="lock" v-hasPermi="['monitor:online:forceLogout']">{{ $t('onlineUserView.forceLogoutAll') }}</el-button>
      </el-form-item>
      <el-form-item>
        <el-radio-group v-model="viewSwitch">
          <el-radio-button value="1">{{ $t('onlineUserView.tableView') }}</el-radio-button>
          <el-radio-button value="2">{{ $t('onlineUserView.cardView') }}</el-radio-button>
        </el-radio-group>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" icon="Search" @click="handleQuery">{{ $t('onlineUserView.refresh') }}</el-button>
      </el-form-item>
    </el-form>
    <el-table :data="onlineUsers" v-loading="loading" ref="tableRef" border highlight-current-row v-if="viewSwitch == 1">
      <el-table-column label="No" type="index" width="50" align="center">
        <template #default="scope">
          <span>{{ (queryParams.pageNum - 1) * queryParams.pageSize + scope.$index + 1 }}</span>
        </template>
      </el-table-column>
      <el-table-column prop="name" :label="$t('onlineUserView.userName')" align="center" />
      <el-table-column :label="$t('onlineUserView.loginLocation')" prop="location" align="center"> </el-table-column>
      <el-table-column :label="$t('onlineUserView.loginIP')" prop="userIP" align="center"></el-table-column>
      <el-table-column prop="browser" :label="$t('onlineUserView.loginBrowser')" width="210"></el-table-column>
      <el-table-column prop="platform" :label="$t('onlineUserView.loginPlatform')" align="center"></el-table-column>
      <el-table-column prop="loginTime" :label="$t('onlineUserView.loginTime')" witdh="280px">
        <template #default="scope">
          {{ dayjs(scope.row.loginTime).format('MM/DD日HH:mm:ss') }}
          <div>{{ $t('onlineUserView.onlineDuration') }}：{{ scope.row.onlineTime }}{{ $t('onlineUserView.minutes') }}</div>
        </template>
      </el-table-column>
      <el-table-column :label="$t('btn.operate')" align="center" width="160">
        <template #default="scope">
          <el-button text @click="onChat(scope.row)" icon="ChatDotRound" v-hasRole="['admin']">{{ $t('onlineUserView.chat') }}</el-button>
          <el-button text @click="onLock(scope.row)" icon="lock" v-hasPermi="['monitor:online:forceLogout']">{{ $t('onlineUserView.forceLogout') }}</el-button>
        </template>
      </el-table-column>
    </el-table>

    <el-row :gutter="20" v-if="viewSwitch == 2">
      <el-col v-for="item in onlineUsers" :lg="4" :span="24">
        <el-card :body-style="{ padding: '15px 15px 0' }">
          <el-descriptions :column="1" :title="item.name">
            <el-descriptions-item :label="$t('onlineUserView.loginPlatform')">{{ item.platform }}</el-descriptions-item>
            <el-descriptions-item :label="$t('onlineUserView.loginLocation')">{{ item.location }}</el-descriptions-item>
            <el-descriptions-item :label="$t('onlineUserView.onlineDuration')" :span="2">
              <el-tag type="success">{{ item.onlineTime }}{{ $t('onlineUserView.minutes') }}</el-tag>
            </el-descriptions-item>
          </el-descriptions>
          <el-text truncated>{{ item.browser }}</el-text>
          <div>
            <el-button text @click="onChat(item)" size="small" icon="ChatDotRound" :title="$t('onlineUserView.chat')" v-hasRole="['admin']">{{ $t('onlineUserView.chat') }}</el-button>
            <el-button text @click="onLock(item)" size="small" icon="lock" :title="$t('onlineUserView.forceLogout')" v-hasPermi="['monitor:online:forceLogout']">{{ $t('onlineUserView.forceLogout') }}</el-button>
          </div>
        </el-card>
      </el-col>

      <el-empty v-show="total == 0" description="no data" />
    </el-row>
    <pagination :total="total" v-model:page="queryParams.pageNum" v-model:limit="queryParams.pageSize" @pagination="getList" />
  </div>
</template>

<script setup name="onlineuser">
import { listOnline, forceLogout, forceLogoutAll } from '@/api/monitor/online'
import dayjs from 'dayjs'
import useSocketStore from '@/store/modules/socket'
const { proxy } = getCurrentInstance()
const queryRef = ref(null)
const queryParams = reactive({
  pageNum: 1,
  pageSize: 10
})

const onlineNum = computed(() => {
  return useSocketStore().onlineNum
})
const viewSwitch = ref('1')
const loading = ref(false)
const onlineUsers = ref([])
const total = ref(0)
function handleQuery() {
  queryParams.pageNum = 1
  getList()
}
function getList() {
  loading.value = true
  listOnline(queryParams).then((res) => {
    if (res.code == 200) {
      total.value = res.data.totalNum
      onlineUsers.value = res.data.result
      setTimeout(() => {
        loading.value = false
      }, 200)
    }
  })
}
getList()

function onChat(item) {
  proxy
    .$prompt(proxy.$t('onlineUserView.inputMessage'), '', {
      confirmButtonText: proxy.$t('onlineUserView.send'),
      cancelButtonText: proxy.$t('common.cancel'),
      inputPattern: /\S/,
      inputErrorMessage: proxy.$t('onlineUserView.messageRequired')
    })
    .then(({ value }) => {
      proxy.signalr.SR.invoke('sendMessage', item.userid, value).catch(function (err) {
        console.error(err.toString())
      })
    })
    .catch(() => {})
}
function onLock(row) {
  proxy
    .$prompt(proxy.$t('onlineUserView.inputForceLogoutReason'), '', {
      confirmButtonText: proxy.$t('onlineUserView.send'),
      cancelButtonText: proxy.$t('common.cancel')
    })
    .then((val) => {
      forceLogout({ ...row, time: 10, reason: val.value, clientId: row.clientId }).then(() => {
        proxy.$modal.msgSuccess(proxy.$t('onlineUserView.forceLogoutSuccess'))
      })
    })
}

// 批量强退
function onLockAll() {
  proxy
    .$prompt(proxy.$t('onlineUserView.inputForceLogoutReason'), '', {
      confirmButtonText: proxy.$t('onlineUserView.send'),
      cancelButtonText: proxy.$t('common.cancel')
    })
    .then((val) => {
      forceLogoutAll({ time: 10, reason: val.value }).then((res) => {
        proxy.$modal.msgSuccess(proxy.$t('onlineUserView.forceLogoutSuccess'))
      })
    })
}
watch(
  onlineNum,
  () => {
    handleQuery()
  },
  {
    immediate: true
  }
)
</script>
<style>
.el-col {
  margin-bottom: 10px;
}
</style>
