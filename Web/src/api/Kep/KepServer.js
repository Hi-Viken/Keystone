import request from '@/utils/request'
import { downFile } from '@/utils/request'

// 启动服务
export function GetServerStatus() {
  return request({
    url: '/Kep/GetServerStatus',
    method: 'get',
  })
}

// // 停止服务
// export function stopServer() {
//   return request({
//     url: '/frp/Server/Stop',
//     method: 'get',
//   })
// }
// // 获取服务信息
// export function GetInfo() {
//   return request({
//     url: '/frp/Server/GetInfo',
//     method: 'get',
//   })
// }
