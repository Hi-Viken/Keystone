import * as signalR from '@microsoft/signalr'
import { getToken } from '@/utils/auth'

let startPromise = null

export default {
  SR: {},
  isInitialized: false,
  async init() {
    if (this.isInitialized) return
    var socketUrl = window.location.origin + '/kephub'
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(socketUrl, { accessTokenFactory: () => getToken() })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Warning)
      .build()
    this.SR = connection
    this.isInitialized = true

    connection.onclose(async (error) => {
      console.error('KepHub 断开连接了' + error)
      startPromise = null
    })

    connection.onreconnected((connectionId) => {
      console.log('KepHub 断线重新连接成功' + connectionId)
      startPromise = null
    })

    connection.onreconnecting(() => {
      console.log('KepHub 断线重新连接中... ')
      startPromise = null
    })
  },
  async start() {
    if (!this.isInitialized) {
      await this.init()
    }
    
    if (this.SR.state === signalR.HubConnectionState.Connected) {
      return true
    }

    if (this.SR.state === signalR.HubConnectionState.Connecting) {
      if (startPromise) {
        await startPromise
        return this.SR.state === signalR.HubConnectionState.Connected
      }
    }

    if (this.SR.state === signalR.HubConnectionState.Disconnected) {
      startPromise = this.SR.start()
      try {
        await startPromise
        startPromise = null
        return true
      } catch (error) {
        console.error('KepHub 连接失败:', error)
        startPromise = null
        return false
      }
    }
    return false
  },
  async ensureConnected() {
    const maxRetries = 3
    let retryCount = 0
    while (retryCount < maxRetries) {
      if (await this.start()) {
        return true
      }
      retryCount++
      await new Promise(resolve => setTimeout(resolve, 1000))
    }
    return false
  }
}
