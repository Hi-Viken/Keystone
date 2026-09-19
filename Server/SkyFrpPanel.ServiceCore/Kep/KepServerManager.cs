using JinianNet.JNTemplate;
using KepServerCore;
using Microsoft.AspNetCore.SignalR;
using SkyFrpPanel.Infrastructure.Attribute;
using SkyFrpPanel.ServiceCore.Kep.IService;
using SkyFrpPanel.ServiceCore.Signalr;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using ZepCommon;
using SqlSugar;
using System.Security.Cryptography;
using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Repository;
using SkyFrpPanel.Model.Kep.Model;
using SkyFrpPanel.Model.Kep.Dto;

namespace SkyFrpPanel.ServiceCore.Kep
{
    [AppService(ServiceType = typeof(IKepServerManager), ServiceLifetime = LifeTime.Singleton, EagerInit = true)]
    public class KepServerManager : IKepServerManager
    {
        private bool ServerState = false;
        private KepServer kepServer;
        private readonly IHubContext<KepHub> _hubContext;
        private readonly IDeviceService _deviceService;
        BindingList<ClientDev> ClientList = new BindingList<ClientDev>();
        private readonly System.Collections.Concurrent.ConcurrentDictionary<uint, (string ClientId, string RequestData)> _pendingClientRequests = new();
        public KepServerManager(IHubContext<KepHub> hubContext,IDeviceService deviceService)
        {
            _hubContext = hubContext;
            _deviceService= deviceService;

        }

        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <returns></returns>
        public KepServerState GetServerStatus()
        {
            if (kepServer == null)
            {
               return new KepServerState() { State = false };
            }
            if (ServerState)
            {
                return new KepServerState() { State = true,IpAddress=kepServer.IpAddress,Port=kepServer.Port,StartTime=kepServer.StartTime, OnlineCount=kepServer.GetOnlineCount() };
            }
            else
            {
                return new KepServerState() { State = false };
            }
        }

        public int GetOnlineCount()
        { 
            return kepServer.GetOnlineCount();
        }

        /// <summary>
        /// 服务开关
        /// </summary>
        public KepServerState StateSwitch()
        {

            if (ServerState)
            {
                ServerState = false;
                _ = kepServer.StopAsync();
                kepServer.OnLog -= msg => PublishLog(msg);//取消注册日志输出事件
                kepServer.OnAuthentication -= OnAuthentication;//取消注册客户端认证事件
                kepServer.OnClientOnline -= OnClientOnline;//取消注册上线事件
                kepServer.OnClientOffline -= OnClientOffline;//取消注册下线事件
                return new KepServerState() { State=false };
            }
            else {
                ServerState = true;
                kepServer = new KepServer("0.0.0.0", 8888);
                kepServer.OnLog += msg => PublishLog(msg);//注册日志输出事件
                kepServer.OnAuthentication += OnAuthentication;//注册客户端认证事件
                kepServer.OnClientOnline += OnClientOnline;//注册上线事件
                kepServer.OnClientOffline += OnClientOffline;//注册下线事件
                //kepServer.OnClientRequestReceived += OnClientRequestReceivedHandler;
                kepServer.Start();
                return new KepServerState() { State = true, IpAddress = kepServer.IpAddress, Port = kepServer.Port, StartTime = kepServer.StartTime };
            }
        }


        /// <summary>
        /// 推送日志
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void PublishLog(string msg)
        {
            //_hubContext.Clients.All.SendAsync("KepLog", msg);
            _hubContext.Clients.Group("KepStatusPage").SendAsync("KepLog", msg);
        }

        /// <summary>
        /// 客户端认证
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        public bool OnAuthentication(ZepCommon.Identity arg)
        {
            return _deviceService.OnAuthentication(arg.SN, arg.Token);
        }

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="obj"></param>
        public async void OnClientOnline(KepConversation obj)
        {
            _deviceService.Online(obj.SN);
            //await _hubContext.Clients.All.SendAsync("ServerStatus", GetServerStatus());
            //await _hubContext.Clients.All.SendAsync("ClientOnline", obj.Id);
            await _hubContext.Clients.Group("KepStatusPage").SendAsync("ServerStatus", GetServerStatus());
            await _hubContext.Clients.Group("KepStatusPage").SendAsync("ClientOnline", obj.Id);


        }

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="obj"></param>
        public async void OnClientOffline( string SN)
        {
            _deviceService.Offline(SN);
            //await _hubContext.Clients.All.SendAsync("ServerStatus", GetServerStatus());
            //await _hubContext.Clients.All.SendAsync("ClientOffline", obj);
            await _hubContext.Clients.Group("KepStatusPage").SendAsync("ServerStatus", GetServerStatus());
            await _hubContext.Clients.Group("KepStatusPage").SendAsync("ClientOffline", SN);
        }

    }

    public class ClientDev
    {
        public string Id { get; set; }
        public string SN { get; set; }
        public string Token { get; set; }
    }

    public class KepServerState
    {
        public bool State { get; set; }
        /// <summary>
        /// 监听地址
        /// </summary>
        public string IpAddress { get; set; }
        /// <summary>
        /// 监听端口
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// 在线数量
        /// </summary>
        public int OnlineCount { get; set; }
        /// <summary>
        /// 启动时间
        /// </summary>
        public DateTime? StartTime { get; set; }
    }
}
