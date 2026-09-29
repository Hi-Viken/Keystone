<template>
  <div class="component-upload-image">
    <el-upload
      multiple
      v-bind="$attrs"
      :action="uploadImgUrl"
      :list-type="listType"
      :on-success="handleUploadSuccess"
      :before-upload="handleBeforeUpload"
      :limit="limit"
      :on-error="handleUploadError"
      :on-exceed="handleExceed"
      name="file"
      :data="uploadData"
      :on-remove="handleRemove"
      :show-file-list="true"
      :headers="headers"
      v-model:file-list="fileList"
      :on-preview="handlePictureCardPreview"
      :style="cssVars"
      :class="{ hide: fileList.length >= limit }">
      <slot name="icon">
        <el-icon class="avatar-uploader-icon"><uploadFilled /></el-icon>
      </slot>

      <template v-slot:tip>
        <div class="el-upload__tip" v-if="showTip">
          <slot name="tip">
            <template v-if="fileSize">
              {{ $t('upload.sizeLimit') }} <b class="text-danger">{{ fileSize }}MB</b>
            </template>
            <template v-if="fileType">
              {{ $t('upload.formatLimit') }} <b class="text-danger">{{ fileType.join('/') }}</b>
            </template>
          </slot>
        </div>
      </template>
    </el-upload>

    <el-dialog v-model="dialogVisible" append-to-body>
      <el-form label-width="100px">
        <el-form-item :label="$t('upload.preview')">
          <el-image style="display: block; max-width: 50%" :src="dialogImageUrl">
            <template #error>
              <div class="image-slot">{{ $t('upload.loadFailed') }}</div>
            </template>
          </el-image>
        </el-form-item>

        <el-form-item :label="$t('upload.accessPath')">
          <el-link type="warning" :href="dialogImageUrl" target="_blank">{{ dialogImageUrl }}</el-link>
          <el-button type="danger" text icon="document-copy" plain class="ml10" v-clipboard:success="copySuccess" v-clipboard:copy="dialogImageUrl"
            >{{ $t('btn.copy') }}</el-button
          >
        </el-form-item>
      </el-form>
    </el-dialog>
  </div>
</template>

<script setup>
import { getToken } from '@/utils/auth'

const props = defineProps({
  modelValue: [String, Object, Array],
  // 图片数量限制
  limit: {
    type: Number,
    default: 1
  },
  // 大小限制(MB)
  fileSize: {
    type: Number,
    default: 5
  },
  // 文件类型, 例如['png', 'jpg', 'jpeg']
  fileType: {
    type: Array,
    default: () => ['png', 'jpg', 'jpeg', 'webp']
  },
  // 是否显示提示
  isShowTip: {
    type: Boolean,
    default: true
  },
  // 上传携带的参数
  data: {
    type: Object
  },
  style: {
    type: Object,
    default: {
      width: '120px'
    }
  },
  listType: {
    type: String,
    default: 'picture-card'
  }
})

const { proxy } = getCurrentInstance()
const emit = defineEmits()
const number = ref(0)
const uploadList = ref([])
const dialogImageUrl = ref('')
const dialogVisible = ref(false)
const baseUrl = import.meta.env.VITE_APP_BASE_API
const uploadImgUrl = ref(baseUrl + import.meta.env.VITE_APP_UPLOAD_URL) // 上传的图片服务器地址
const headers = ref({ Authorization: 'Bearer ' + getToken() })
const fileList = ref([])
const showTip = computed(() => props.isShowTip && (props.fileType || props.fileSize))
const uploadData = computed(() => props.data)
const cssVars = computed(() => {
  return {
    '--el-upload-picture-card-size': props.style.width,
    '--el-upload-list-picture-card-size': props.style.width
  }
})
watch(
  () => props.modelValue,
  (val) => {
    if (val) {
      // 首先将值转为数组
      const list = Array.isArray(val) ? val : props.modelValue.split(',')
      // 然后将数组转为对象数组
      fileList.value = list.map((item) => {
        if (typeof item === 'string') {
          item = { name: item, url: item }
        }
        return item
      })
    } else {
      fileList.value = []
      return []
    }
  },
  { deep: true, immediate: true }
)

// 删除图片
function handleRemove(file, files) {
  emit('update:modelValue', listToString(fileList.value))
}

// 上传成功回调
function handleUploadSuccess(res) {
  if (res.code != 200) {
    proxy.$modal.msgError(proxy.$t('upload.uploadFailedReason') + res.msg + '!')
    proxy.$modal.closeLoading()
    fileList.value = fileList.value.slice(0, fileList.value.length - 1)
    return
  }
  uploadList.value.push({ name: res.data.fileName, url: res.data.url })
  if (uploadList.value.length === number.value) {
    fileList.value = fileList.value.filter((f) => f.url !== undefined).concat(uploadList.value)
    uploadList.value = []
    number.value = 0
    emit('update:modelValue', listToString(fileList.value))
  }
  proxy.$modal.closeLoading()
}

// 上传前loading加载
function handleBeforeUpload(file) {
  let isImg = false
  if (props.fileType.length) {
    let fileExtension = ''
    if (file.name.lastIndexOf('.') > -1) {
      fileExtension = file.name.slice(file.name.lastIndexOf('.') + 1)
    }
    isImg = props.fileType.some((type) => {
      if (file.type.indexOf(type) > -1) return true
      if (fileExtension && fileExtension.indexOf(type) > -1) return true
      return false
    })
  } else {
    isImg = file.type.indexOf('image') > -1
  }
  if (!isImg) {
    proxy.$modal.msgError(proxy.$t('upload.wrongImageFormat') + props.fileType.join('/') + '!')
    return false
  }
  if (props.fileSize) {
    const isLt = file.size / 1024 / 1024 < props.fileSize
    if (!isLt) {
      proxy.$modal.msgError(proxy.$t('upload.imageSizeExceed') + props.fileSize + ' MB!')
      return false
    }
  }
  proxy.$modal.loading(proxy.$t('upload.uploadingImage'))
  number.value++
}

// 文件个数超出
function handleExceed() {
  proxy.$modal.msgError(proxy.$t('upload.exceedLimit') + props.limit)
}

// 上传失败
function handleUploadError() {
  proxy.$modal.msgError(proxy.$t('upload.imageUploadFailed'))
  proxy.$modal.closeLoading()
}

// 预览
function handlePictureCardPreview(file) {
  dialogImageUrl.value = file.url
  dialogVisible.value = true
}

// 对象转成指定字符串分隔
function listToString(list, separator) {
  let strs = ''
  separator = separator || ','
  for (let i in list) {
    if (undefined !== list[i].url && list[i].url.indexOf('blob:') !== 0) {
      strs += list[i].url + separator
    }
  }
  return strs != '' ? strs.substr(0, strs.length - 1) : ''
}
function copySuccess() {
  proxy.$modal.msgSuccess(proxy.$t('common.copySuccess'))
}
</script>
<style>
.el-upload--picture-card {
  --el-upload-picture-card-size: unset;
}
.el-upload-list--picture-card {
  --el-upload-list-picture-card-size: unset;
}
.el-upload__tip {
  font-size: 10px;
}
</style>
