import request from '@/utils/request'
import { downFile } from '@/utils/request'

// 查询材料列表（分页）
export function listMaterial(query) {
  return request({
    url: '/Material/List',
    method: 'get',
    params: query
  })
}

// 查询材料详细
export function getMaterial(id) {
  return request({
    url: '/Material/Query/' + id,
    method: 'get'
  })
}

// 新增材料
export function addMaterial(data) {
  return request({
    url: '/Material/Add',
    method: 'post',
    data: data
  })
}

// 修改材料
export function updateMaterial(data) {
  return request({
    url: '/Material/Update',
    method: 'put',
    data: data
  })
}

// 删除材料
export function delMaterial(id) {
  return request({
    url: '/Material/Delete/' + id,
    method: 'delete'
  })
}

// 导出材料
export async function exportMaterial(query) {
  await downFile('/Material/Export', query)
}
