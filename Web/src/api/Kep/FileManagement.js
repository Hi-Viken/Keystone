import request from '@/utils/request'
import { downFile } from '@/utils/request'

// 查询文件管理列表（分页）
export function listFileManagement(query) {
  return request({
    url: '/FileManagement/List',
    method: 'get',
    params: query
  })
}

// 按父级ID分页查询当前目录内容
export function listFileManagementByParentId(query) {
  return request({
    url: '/FileManagement/ListByParentId',
    method: 'get',
    params: query
  })
}

// 获取面包屑路径
export function getBreadcrumbPath(id) {
  return request({
    url: '/FileManagement/Breadcrumb/' + id,
    method: 'get'
  })
}

// 查询文件管理树形列表
export function treeListFileManagement(query) {
  return request({
    url: '/FileManagement/TreeList',
    method: 'get',
    params: query
  })
}

// 查询文件管理列表（排除指定节点及其子节点）
export function listFileManagementExcludeChild(id) {
  return request({
    url: '/FileManagement/ExcludeChild/' + id,
    method: 'get'
  })
}

// 查询文件管理详细
export function getFileManagement(id) {
  return request({
    url: '/FileManagement/Query/' + id,
    method: 'get'
  })
}

// 新增文件/目录
export function addFileManagement(data) {
  return request({
    url: '/FileManagement/Add',
    method: 'post',
    data: data
  })
}

// 修改文件/目录
export function updateFileManagement(data) {
  return request({
    url: '/FileManagement/Update',
    method: 'put',
    data: data
  })
}

// 删除文件/目录
export function delFileManagement(id) {
  return request({
    url: '/FileManagement/Delete/' + id,
    method: 'delete'
  })
}

// 导出文件管理
export async function exportFileManagement(query) {
  await downFile('/FileManagement/Export', query)
}

// 上传文件
export function uploadFile(parentId, formData) {
  formData.append('parentId', parentId)
  return request({
    url: '/FileManagement/UploadFile',
    method: 'post',
    data: formData,
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}

// 批量上传文件
export function uploadFiles(parentId, formData) {
  formData.append('parentId', parentId)
  return request({
    url: '/FileManagement/UploadFiles',
    method: 'post',
    data: formData,
    headers: {
      'Content-Type': 'multipart/form-data'
    }
  })
}

// 下载文件
export function downloadFile(id) {
  return request({
    url: '/FileManagement/Download/' + id,
    method: 'get',
    responseType: 'blob'
  })
}
