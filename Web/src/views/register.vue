<template>
  <starBackground></starBackground>
  <div class="login-wrap">
    <div class="login">
      <el-form ref="registerFormRef" :model="registerForm" :rules="registerRules" class="login-form">
        <h3 class="title">{{ title }}</h3>
        <el-form-item prop="username">
          <el-input v-model="registerForm.username" type="text" size="default" auto-complete="off" :placeholder="$t('register.account')">
            <template #prefix>
              <svg-icon name="user" />
            </template>
          </el-input>
        </el-form-item>
        <el-form-item prop="password">
          <el-input
            v-model="registerForm.password"
            type="password"
            size="default"
            auto-complete="off"
            :placeholder="$t('register.password')"
            @keyup.enter="handleRegister">
            <template #prefix>
              <svg-icon name="password" />
            </template>
          </el-input>
        </el-form-item>
        <el-form-item prop="confirmPassword">
          <el-input
            v-model="registerForm.confirmPassword"
            type="password"
            size="default"
            auto-complete="off"
            :placeholder="$t('register.confirmPassword')"
            @keyup.enter="handleRegister">
            <template #prefix>
              <svg-icon name="password" />
            </template>
          </el-input>
        </el-form-item>
        <el-form-item prop="code" v-if="captchaOnOff">
          <el-input
            v-model="registerForm.code"
            auto-complete="off"
            size="default"
            :placeholder="$t('register.captcha')"
            style="width: 63%"
            @keyup.enter="handleRegister">
            <template #prefix>
              <svg-icon name="validCode" />
            </template>
          </el-input>
          <div class="register-code ml10">
            <img :src="codeUrl" @click="getCode" class="register-code-img" />
          </div>
        </el-form-item>
        <el-form-item style="width: 100%">
          <el-button :loading="loading" type="primary" size="default" round style="width: 100%" @click.prevent="handleRegister">
            <span v-if="!loading">{{ $t('login.register') }}</span>
            <span v-else>{{ $t('register.registering') }}</span>
          </el-button>
        </el-form-item>
        <div style="text-align: center">
          <router-link class="link-type" :to="'/login'">{{ $t('register.useExistingAccount') }}</router-link>
        </div>
      </el-form>
      <oauthLogin></oauthLogin>
    </div>
    <!--  底部  -->
    <div class="el-register-footer">
      <div v-html="copyRight"></div>
    </div>
  </div>
</template>

<script setup name="register">
import starBackground from '@/views/components/starBackground.vue'
import { getCodeImg, register } from '@/api/system/login'
import defaultSettings from '@/settings'
import { ElMessageBox } from 'element-plus'
import oauthLogin from './components/Login/oauthLogin.vue'
const { proxy } = getCurrentInstance()
const router = useRouter()
const codeUrl = ref('')
const registerForm = reactive({
  username: '',
  password: '',
  confirmPassword: '',
  code: '',
  uuid: ''
})

const registerFormRef = ref(null)
const loading = ref(false)
const captchaOnOff = ref(true)
const equalToPassword = (rule, value, callback) => {
  if (registerForm.password !== value) {
    callback(new Error(proxy.$t('register.passwordMismatch')))
  } else {
    callback()
  }
}
const registerRules = reactive({
  username: [
    { required: true, trigger: 'blur', message: proxy.$t('register.account') },
    {
      min: 5,
      max: 20,
      message: proxy.$t('register.accountLength'),
      trigger: 'blur'
    }
  ],
  password: [
    { required: true, trigger: 'blur', message: proxy.$t('register.password') },
    {
      min: 5,
      max: 20,
      message: proxy.$t('register.passwordLength'),
      trigger: 'blur'
    }
  ],
  confirmPassword: [
    { required: true, trigger: 'blur', message: proxy.$t('register.confirmPassword') },
    { required: true, validator: equalToPassword, trigger: 'blur' }
  ],
  code: [{ required: true, trigger: 'change', message: proxy.$t('register.captcha') }]
})
const copyRight = computed(() => {
  return defaultSettings.copyright
})
const title = computed(() => {
  return defaultSettings.title
})

function getCode() {
  getCodeImg().then((res) => {
    codeUrl.value = 'data:image/gif;base64,' + res.data.img
    registerForm.uuid = res.data.uuid
    // this.$forceUpdate()
  })
}
function handleRegister() {
  proxy.$refs['registerFormRef'].validate((valid) => {
    if (valid) {
      loading.value = true
      register(registerForm)
        .then((res) => {
          if (res.code == 200) {
            const username = registerForm.username
            ElMessageBox.alert("<font color='red'>" + proxy.$t('register.registerSuccess', { username }) + '</font>', proxy.$t('common.tips'), {
              dangerouslyUseHTMLString: true,
              type: 'success'
            })
              .then(() => {
                router.push('/login')
              })
              .catch(() => {})
          }
        })
        .catch(() => {
          loading.value = false
          if (captchaOnOff.value) {
            getCode()
          }
        })
    }
  })
}
getCode()
</script>

<style rel="stylesheet/scss" lang="scss">
@use '@/assets/styles/login.scss';
.register {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100%;
  background-size: cover;
  flex-direction: column;
  background: radial-gradient(220% 105% at top center, #1b2947 10%, #4b76a7 40%, #81acae 65%, #f7f7b6);
}
.login-form {
  padding: 15px 25px 15px 25px;
  height: 320px;
}
.title {
  margin: 0px auto 30px auto;
  text-align: center;
  // color: #fff;
}

.register-tip {
  font-size: 13px;
  text-align: center;
  color: #bfbfbf;
}
.register-code {
  width: 33%;
  height: 38px;
  float: right;
  img {
    cursor: pointer;
    vertical-align: middle;
  }
}
.el-register-footer {
  height: 40px;
  line-height: 40px;
  position: fixed;
  bottom: 0;
  width: 100%;
  text-align: center;
  color: #fff;
  font-family: Arial;
  font-size: 12px;
  letter-spacing: 1px;
}
.register-code-img {
  height: 38px;
}
</style>
