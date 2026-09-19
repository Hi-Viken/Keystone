const CACHE_NAME = 'astra-cache-v1'

// 需要预缓存的资源
const PRECACHE_URLS = [
	'/',
	'/index.html',
	'/offline.html'
]

// 安装
self.addEventListener('install', event => {
	console.log('[SW] 安装')

	event.waitUntil(
		caches.open(CACHE_NAME).then(cache => {
			return cache.addAll(PRECACHE_URLS)
		})
	)

	self.skipWaiting()
})

// 激活
self.addEventListener('activate', event => {
	console.log('[SW] 激活')

	event.waitUntil(
		caches.keys().then(keys =>
			Promise.all(
				keys.map(key => {
					if (key !== CACHE_NAME) {
						return caches.delete(key)
					}
				})
			)
		)
	)

	self.clients.claim()
})

// 请求拦截
self.addEventListener('fetch', event => {
	const req = event.request

	// 只处理 GET
	if (req.method !== 'GET') return

	// API 请求不缓存
	if (req.url.includes('/api/')) {
		event.respondWith(
			fetch(req).catch(() => {
				return new Response(
					JSON.stringify({ msg: '网络不可用' }),
					{ headers: { 'Content-Type': 'application/json' } }
				)
			})
		)
		return
	}

	event.respondWith(
		fetch(req)
			.then(res => {
				// 缓存一份
				const resClone = res.clone()
				caches.open(CACHE_NAME).then(cache => {
					cache.put(req, resClone)
				})
				return res
			})
			.catch(() => {
				// 👉 页面请求 → 返回离线页
				if (req.mode === 'navigate') {
					return caches.match('/offline.html')
				}

				// 👉 其他资源 → 尝试缓存
				return caches.match(req)
			})
	)
})