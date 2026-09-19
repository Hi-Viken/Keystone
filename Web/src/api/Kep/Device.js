import request from '@/utils/request'
import { downFile } from '@/utils/request'

// 查询设备列表
export function listDevice(query) {
  return request({
    url: '/Device/List',
    method: 'get',
    params: query
  })
}

// 查询设备详细
export function getDevice(id) {
  return request({
    url: '/Device/Query/' + id,
    method: 'get'
  })
}

// 获取设备Token明文
export function getDeviceToken(id) {
  return request({
    url: '/Device/GetToken/' + id,
    method: 'get'
  })
}

// 获取设备Secret明文
export function getDeviceSecret(id) {
  return request({
    url: '/Device/GetSecret/' + id,
    method: 'get'
  })
}

// 新增设备
export function addDevice(data) {
  return request({
    url: '/Device/Add',
    method: 'post',
    data: data
  })
}

// 修改设备
export function updateDevice(data) {
  return request({
    url: '/Device/Update',
    method: 'put',
    data: data
  })
}

// 删除设备
export function delDevice(id) {
  return request({
    url: '/Device/Delete/' + id,
    method: 'delete'
  })
}

// 导出设备
export async function exportDevice(query) {
  await downFile('/Device/Export', query)
}
