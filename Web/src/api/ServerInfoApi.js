import request from '@/utils/request' // 假设已配置好 Axios 实例
const ServerInfoApi = {
	GetServerInfo: () => {
		return request({
			url: '/api/ServerInfo/GetServerInfo',
			method: 'get'
		})
	}
	// GetLineArea: () => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/GetLineArea',
	// 		method: 'get'
	// 	})
	// },
	// GetInitiator: () => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/GetInitiator',
	// 		method: 'get'
	// 	})
	// },
	// GetMaterial: () => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/GetMaterial',
	// 		method: 'get'
	// 	})
	// },
	// GetBadCategoryGP12: () => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/GetBadCategoryGP12',
	// 		method: 'get'
	// 	})
	// },
	// GetUser: (query) => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/GetUser',
	// 		params: query,
	// 		method: 'get'
	// 	})
	// },
	// getDetail: (Id) => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/detail/' + Id,
	// 		method: 'get'
	// 	})
	// },
	// Add: (data) => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/add',
	// 		data: data,
	// 		method: 'post',
	// 		headers: { 'Content-Type': 'multipart/form-data' },
	// 		timeout: 60000 // 60秒超时
	// 	})
	// },
	// Update: (data) => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/update',
	// 		data: data,
	// 		method: 'put',
	// 		headers: { 'Content-Type': 'multipart/form-data' },
	// 		timeout: 60000 // 60秒超时
	// 	})
	// },
	// Getlist: (query) => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/list',
	// 		method: 'get',
	// 		params: query
	// 	})
	// },
	// getMeasurement: (fileId) => {
	// 	return request({
	// 		url: '/worker/SqcnTrack/getMeasurement/' + fileId,
	// 		method: 'get',
	// 	})
	// }
}
export default ServerInfoApi