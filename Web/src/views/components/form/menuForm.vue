<template>
  <el-dialog draggable="" :title="title" v-model="open" width="720px" append-to-body>
    <el-form ref="menuRef" :model="form" :rules="rules" label-width="110px">
      <el-row>
        <el-col :lg="24">
          <el-form-item :label="$t('m.parentMenu')">
            <el-cascader
              class="w100"
              :options="menuOptions"
              :props="{ checkStrictly: true, value: 'menuId', label: 'menuName', emitPath: false }"
              :placeholder="$t('menuForm.selectParentMenu')"
              clearable
              v-model="form.parentId">
              <template #default="{ node, data }">
                <span>{{ data.menuName }}</span>
                <span v-if="!node.isLeaf"> ({{ data.children.length }}) </span>
              </template>
            </el-cascader>
          </el-form-item>
        </el-col>
        <el-col :lg="24">
          <el-form-item :label="$t('m.menuType')" prop="menuType">
            <el-radio-group v-model="form.menuType">
              <el-radio-button value="M">{{ $t('m.directory') }}M</el-radio-button>
              <el-radio-button value="C">{{ $t('m.menu') }}C</el-radio-button>
              <el-radio-button value="F">{{ $t('m.button') }}F</el-radio-button>
              <el-radio-button value="L">{{ $t('m.link') }}L</el-radio-button>
            </el-radio-group>
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('m.menuName')" prop="menuName">
            <el-input v-model="form.menuName" :placeholder="$t('menuForm.inputMenuName')" />
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('menuForm.menuNameLabel')" prop="menuNameKey">
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.menuNameKeyTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.menuNameKey') }}
              </span>
            </template>
            <el-input v-model="form.menuNameKey" :placeholder="$t('menuForm.inputMenuNameKey')" />
          </el-form-item>
        </el-col>
        <el-col :lg="12" v-if="form.menuType != 'F'">
          <el-form-item :label="$t('m.icon')" prop="icon">
            <el-popover placement="bottom-start" :width="540" trigger="click">
              <template #reference>
                <el-input v-model="form.icon" :placeholder="$t('menuForm.selectIcon')" readonly>
                  <template #prefix>
                    <svg-icon v-if="form.icon" :name="form.icon" />
                    <el-icon v-else>
                      <search />
                    </el-icon>
                  </template>
                </el-input>
              </template>
              <icon-select ref="iconSelectRef" @selected="selected" />
            </el-popover>
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item :label="$t('m.sort')" prop="orderNum">
            <el-input-number v-model="form.orderNum" controls-position="right" :min="0" />
          </el-form-item>
        </el-col>
        <el-col :lg="12" v-if="!['F'].includes(form.menuType)">
          <el-form-item prop="path">
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.routePathTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.routePath') }}
              </span>
            </template>
            <el-input v-model="form.path" :placeholder="$t('menuForm.inputRoutePath')"> </el-input>
          </el-form-item>
        </el-col>
        <el-col :lg="12" v-if="!['F', 'M', 'L'].includes(form.menuType)">
          <el-form-item prop="component">
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.componentPathTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.componentPath') }}
              </span>
            </template>
            <el-input v-model="form.component" :placeholder="$t('menuForm.inputComponentPath')">
              <template #prepend>
                <span style="width: 40px">src/views/</span>
              </template>
            </el-input>
          </el-form-item>
        </el-col>
        <el-col :lg="12">
          <el-form-item>
            <el-input v-model="form.perms" :placeholder="$t('menuForm.inputPermission')" maxlength="100" />
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.permissionTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.permissionStr') }}
              </span>
            </template>
          </el-form-item>
        </el-col>
        <el-col :lg="12" v-if="form.menuType == 'C'">
          <el-form-item>
            <el-input v-model="form.query" :placeholder="$t('menuForm.inputRouteQuery')" maxlength="255" />
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.routeQueryTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('menuForm.routeQuery') }}
              </span>
            </template>
          </el-form-item>
        </el-col>

        <el-col :lg="12" v-if="form.menuType == 'L'">
          <el-form-item>
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.isFrameTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.isFrame') }}
              </span>
            </template>
            <el-radio-group v-model="form.isFrame">
              <el-radio-button value="0">{{ $t('common.no') }}</el-radio-button>
              <el-radio-button value="1">{{ $t('common.yes') }}</el-radio-button>
            </el-radio-group>
          </el-form-item>
        </el-col>
        <el-col :lg="12" v-if="form.menuType == 'C'">
          <el-form-item prop="isCache">
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.isCacheTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.isCache') }}
              </span>
            </template>
            <!-- <el-radio-group v-model="form.isCache">
                <el-radio value="0">{{ $t('common.yes') }}</el-radio>
                <el-radio value="1">{{ $t('common.no') }}</el-radio>
              </el-radio-group> -->
            <el-switch v-model="form.isCache" active-value="0" inactive-value="1"></el-switch>
          </el-form-item>
        </el-col>
        <el-col :lg="12" v-if="form.menuType != 'F'">
          <el-form-item prop="visible">
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.isShowTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.isShow') }}
              </span>
            </template>
            <el-radio-group v-model="form.visible">
              <el-radio v-for="dict in options.sys_show_hide" :key="dict.dictValue" :value="dict.dictValue">{{ dict.dictLabel }}</el-radio>
            </el-radio-group>
          </el-form-item>
        </el-col>
        <el-col :lg="12" v-if="form.menuType != 'F'">
          <el-form-item>
            <template #label>
              <span>
                <el-tooltip :content="$t('menuForm.menuStateTip')" placement="top">
                  <el-icon :size="15">
                    <questionFilled />
                  </el-icon>
                </el-tooltip>
                {{ $t('m.menuState') }}
              </span>
            </template>
            <el-radio-group v-model="form.status">
              <el-radio v-for="dict in options.sys_normal_disable" :key="dict.dictValue" :value="dict.dictValue">{{ dict.dictLabel }}</el-radio>
            </el-radio-group>
          </el-form-item>
        </el-col>
      </el-row>
    </el-form>
    <template #footer>
      <el-button text @click="cancel">{{ $t('btn.cancel') }}</el-button>
      <el-button type="primary" @click="submitForm">{{ $t('btn.submit') }}</el-button>
    </template>
  </el-dialog>
</template>
<script setup>
import { addMenu, getMenu, updateMenu } from '@/api/system/menu'
import IconSelect from '@/components/IconSelect'
const { proxy } = getCurrentInstance()
const emit = defineEmits()
const iconSelectRef = ref(null)
const open = ref(false)
const title = ref('')
const menuRef = ref(null)

const props = defineProps({
  options: {},
  menuOptions: {}
})
const state = reactive({
  form: {},
  rules: {
    menuName: [{ required: true, message: proxy.$t('menuForm.menuNameRequired'), trigger: 'blur' }],
    menuNameKey: [{ pattern: /^[A-Za-z].+$/, message: proxy.$t('menuForm.inputFormatIncorrect'), trigger: 'blur' }],
    orderNum: [{ required: true, message: proxy.$t('menuForm.orderNumRequired'), trigger: 'blur' }],
    path: [
      { required: false, message: proxy.$t('menuForm.routePathRequired'), trigger: 'blur' },
      { pattern: /^[/A-Za-z].+$/, message: proxy.$t('menuForm.pathFormatIncorrect'), trigger: 'blur' }
    ],
    visible: [{ required: true, message: proxy.$t('menuForm.visibleRequired'), trigger: 'blur' }],
    component: [{ required: true, message: proxy.$t('menuForm.componentRequired'), trigger: 'blur' }]
  },
  sys_show_hide: [],
  sys_normal_disable: []
})
const { form, rules } = toRefs(state)

// 监听 isRequired 变化，动态更新校验规则
watch(
  () => form.value.menuType,
  (newVal) => {
    if (newVal) {
      rules.value.path[0].required = ['C', 'L'].includes(newVal) ? true : false
    }
  }
)

/** 取消按钮 */
function cancel() {
  open.value = false
  reset()
}
/** 表单重置 */
function reset() {
  form.value = {
    menuId: undefined,
    parentId: 0,
    menuName: undefined,
    icon: undefined,
    menuType: 'M',
    orderNum: 999,
    isFrame: '0',
    isCache: '0',
    visible: '0',
    status: '0'
  }
  proxy.resetForm('menuRef')
}

/** 选择图标 */
function selected(name) {
  form.value.icon = name
}
/** 新增按钮操作 */
function handleAdd(row) {
  reset()
  if (row != null && row.menuId != undefined) {
    form.value.parentId = row.menuId
  } else {
    form.value.parentId = 0
  }
  open.value = true
  title.value = proxy.$t('btn.add')
}
/** 修改按钮操作 */
async function handleUpdate(row) {
  reset()
  getMenu(row.menuId).then((response) => {
    form.value = response.data
    open.value = true
    title.value = proxy.$t('btn.edit')
  })
}
/** 提交按钮 */
function submitForm() {
  proxy.$refs['menuRef'].validate((valid) => {
    if (valid) {
      if (form.value.menuId != undefined) {
        updateMenu(form.value).then(() => {
          proxy.$modal.msgSuccess(proxy.$t('common.updateSuccess'))
          open.value = false

          emit('success', form.value.parentId)
          // refreshMenu(form.value.parentId)
        })
      } else {
        addMenu(form.value).then(() => {
          proxy.$modal.msgSuccess(proxy.$t('common.addSuccess'))
          open.value = false
          // refreshMenu(form.value.parentId)
          emit('success', form.value.parentId)
        })
      }
    }
  })
}

defineExpose({
  handleAdd,
  handleUpdate
})
</script>
