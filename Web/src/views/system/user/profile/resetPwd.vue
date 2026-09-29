<template>
  <el-form ref="pwdRef" :model="user" :rules="rules" label-width="100px" label-position="left" style="max-width: 350px">
    <el-form-item :label="$t('user.oldPwd')" prop="oldPassword">
      <el-input v-model="user.oldPassword" :placeholder="$t('user.oldPwdPh')" type="password" show-password />
    </el-form-item>
    <el-form-item :label="$t('user.newPwd')" prop="newPassword">
      <el-input v-model="user.newPassword" :placeholder="$t('user.newPwdPh')" type="password" show-password />
    </el-form-item>
    <el-form-item :label="$t('user.confirmPwd')" prop="confirmPassword">
      <el-input v-model="user.confirmPassword" :placeholder="$t('user.confirmPwdPh')" type="password" show-password />
    </el-form-item>
    <el-form-item>
      <el-button type="danger" icon="Close" @click="close">{{ $t('btn.close') }}</el-button>
      <el-button type="primary" icon="Check" @click="submit">{{ $t('btn.save') }}</el-button>
    </el-form-item>
  </el-form>
</template>

<script setup>
import { updateUserPwd } from '@/api/system/user'

const { proxy } = getCurrentInstance()

const user = reactive({
  oldPassword: undefined,
  newPassword: undefined,
  confirmPassword: undefined
})

const equalToPassword = (rule, value, callback) => {
  if (user.newPassword !== value) {
    callback(new Error(proxy.$t('user.pwdMismatch')))
  } else {
    callback()
  }
}
const rules = ref({
  oldPassword: [{ required: true, message: proxy.$t('user.oldPwdRequired'), trigger: 'blur' }],
  newPassword: [
    { required: true, message: proxy.$t('user.newPwdRequired'), trigger: 'blur' },
    { min: 6, max: 20, message: proxy.$t('user.pwdLengthTip'), trigger: 'blur' }
  ],
  confirmPassword: [
    { required: true, message: proxy.$t('user.confirmPwdRequired'), trigger: 'blur' },
    { required: true, validator: equalToPassword, trigger: 'blur' }
  ]
})

/** 提交按钮 */
function submit() {
  proxy.$refs.pwdRef.validate((valid) => {
    if (valid) {
      updateUserPwd(user.oldPassword, user.newPassword).then((response) => {
        proxy.$modal.msgSuccess(proxy.$t('crud.editSuccess'))
        close()
      })
    }
  })
}
/** 关闭按钮 */
function close() {
  proxy.$tab.closePage()
}
</script>
