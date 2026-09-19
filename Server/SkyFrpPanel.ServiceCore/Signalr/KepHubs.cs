using KepServerCore;
using Mapster;
using Microsoft.AspNetCore.SignalR;
using SkyFrpPanel.Infrastructure;
using SkyFrpPanel.Infrastructure.IPTools;
using SkyFrpPanel.Infrastructure.Model;
using SkyFrpPanel.Model.Dto;
using SkyFrpPanel.Model.Models;
using SkyFrpPanel.ServiceCore.Kep;
using SkyFrpPanel.ServiceCore.Kep.IService;
using SkyFrpPanel.ServiceCore.Monitor.IMonitorService;
using SkyFrpPanel.ServiceCore.Services;
using System.Collections.Concurrent;
using System.Web;
namespace SkyFrpPanel.ServiceCore.Signalr
{
    public class KepHub : Hub
    {
        private readonly IKepServerManager _kepServer;

        // 全局推送上下文，生命周期稳定，不会被释放
        private readonly IHubContext<KepHub> _hubContext;

        public KepHub(IKepServerManager kepServer, IHubContext<KepHub> hubContext)
        {
            _kepServer = kepServer;
            _hubContext = hubContext;
        }


        public async Task SubscribeKepStatusPage()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "KepStatusPage");
        }
        public async Task UnSubscribeKepStatusPage()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "KepStatusPage");
        }



        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <returns></returns>
        public KepServerState GetServerStatus() { 
            return _kepServer.GetServerStatus();
        }

        /// <summary>
        /// 服务开关
        /// </summary>
        public KepServerState StateSwitch()
        {
            return _kepServer.StateSwitch();
        }

        /// <summary>
        /// 推送日志
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public void PublishLog(string msg)
        {
            _kepServer.PublishLog(msg);
        }




        //// 客户端调用这个方法发消息
        //public async Task SendMessage(string user, string message)
        //{
        //    //_kepServer.OnLog += async msg =>  GlobalKepLogHandler(msg);
        //    //_kepServer.Start();

        //    //// 广播给所有在线客户端
        //    //await Clients.All.SendAsync("ReceiveMessage", user, message);
        //}







        //// 静态全局日志处理方法，不依赖任何Hub实例
        //private async void GlobalKepLogHandler(string message)
        //{
        //    try
        //    {
        //        // 使用 IHubContext 推送，永远不会报对象已释放
        //        await _hubContext.Clients.All.SendAsync("OnLog", message);
        //    }
        //    catch
        //    {
        //        // 连接全部断开时静默吃掉异常
        //    }
        //}


        //~KepHub()
        //{
        //    // 释放非托管资源
        //    Console.WriteLine("析构函数执行");
        //}
    }
}
