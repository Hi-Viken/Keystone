<template>
  <el-form ref="basicInfoForm" :model="info" :rules="rules" label-width="100px">
    <el-row>
      <el-col :lg="6">
        <el-form-item :label="$t('genEditView.tableName')" prop="tableName">
          <el-input :placeholder="$t('genEditView.tableNamePh')" v-model="info.tableName" />
        </el-form-item>
      </el-col>
      <el-col :lg="6">
        <el-form-item :label="$t('genEditView.tableComment')" prop="tableComment">
          <el-input :placeholder="$t('genEditView.inputPh')" v-model="info.tableComment" />
        </el-form-item>
      </el-col>

      <el-col :lg="6">
        <el-form-item :label="$t('genEditView.className')" prop="className">
          <el-input :placeholder="$t('genEditView.inputPh')" v-model="info.className" />
        </el-form-item>
      </el-col>
      <el-col :lg="6">
        <el-form-item :label="$t('genEditView.author')" prop="functionAuthor">
          <el-input :placeholder="$t('genEditView.inputPh')" v-model="info.functionAuthor" />
        </el-form-item>
      </el-col>
      <el-col :lg="24">
        <el-form-item :label="$t('common.remark')" prop="remark">
          <el-input type="textarea" :rows="3" v-model="info.remark"></el-input>
        </el-form-item>
      </el-col>
    </el-row>
  </el-form>
</template>
<script setup name="BasicInfoForm">
const props = defineProps({
  info: {
    type: Object,
    default: null,
  },
})
const { proxy } = getCurrentInstance()
const rules = reactive({
  tableName: [{ required: true, message: proxy.$t('genEditView.tableNameRequired'), trigger: 'blur' }],
  tableComment: [{ required: true, message: proxy.$t('genEditView.tableCommentRequired'), trigger: 'blur' }],
  className: [{ required: true, message: proxy.$t('genEditView.classNameRequired'), trigger: 'blur' }],
  functionAuthor: [{ required: true, message: proxy.$t('genEditView.authorRequired'), trigger: 'blur' }],
})
</script>
