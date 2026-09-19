import request from '@/utils/request'
import { downFile } from '@/utils/request'

 // 查询节点列表
 export function listNode(query) {
   return request({
     url: '/Kep/list',
     method: 'get',
     params: query
   })
 }

 // 查询节点详细
 export function getNode(nodeId) {
   return request({
     url: '/frp/node/' + nodeId,
     method: 'get'
   })
 }

 // 新增节点
 export function addNode(data) {
   return request({
     url: '/frp/node',
     method: 'post',
     data: data
   })
 }

 // 修改节点
 export function updateNode(data) {
   return request({
     url: '/frp/node',
     method: 'put',
     data: data
   })
 }

 // 删除节点
 export function delNode(nodeId) {
   return request({
     url: '/frp/node/' + nodeId,
     method: 'delete'
   })
 }

 // 修改节点状态
 export function changeNodeStatus(nodeId, isEnable) {
   const data = {
     nodeId,
     isEnable
   }
   return request({
url: '/frp/node/changeStatus',
     method: 'put',
     data: data
   })
 }

 // 生成唯一令牌
 export function generateToken() {
   return request({
     url: '/frp/node/generateToken',
     method: 'get'
   })
 }

 // 获取节点令牌明文
 export function getNodeToken(nodeId) {
   return request({
     url: '/frp/node/token/' + nodeId,
     method: 'get'
   })
 }

 // 导出节点
 export async function exportNode(query) {
   await downFile('/frp/node/export', query)
 }