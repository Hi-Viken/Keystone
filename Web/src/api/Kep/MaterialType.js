import request from '@/utils/request'
import { downFile } from '@/utils/request'

// 查询材料类型列表（分页）
export function listMaterialType(query) {
  return request({
    url: '/MaterialType/List',
    method: 'get',
    params: query
  })
}

// 查询材料类型树形列表
export function treeListMaterialType(query) {
  return request({
    url: '/MaterialType/TreeList',
    method: 'get',
    params: query
  })
}

// 查询材料类型列表（排除指定节点及其子节点）
export function listMaterialTypeExcludeChild(id) {
  return request({
    url: '/MaterialType/ExcludeChild/' + id,
    method: 'get'
  })
}

// 查询材料类型详细
export function getMaterialType(id) {
  return request({
    url: '/MaterialType/Query/' + id,
    method: 'get'
  })
}

// 新增材料类型
export function addMaterialType(data) {
  return request({
    url: '/MaterialType/Add',
    method: 'post',
    data: data
  })
}

// 修改材料类型
export function updateMaterialType(data) {
  return request({
    url: '/MaterialType/Update',
    method: 'put',
    data: data
  })
}

// 删除材料类型
export function delMaterialType(id) {
  return request({
    url: '/MaterialType/Delete/' + id,
    method: 'delete'
  })
}

// 导出材料类型
export async function exportMaterialType(query) {
  await downFile('/MaterialType/Export', query)
}
