using KepServerCore;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyFrpPanel.ServiceCore.Kep.IService
{
    public interface IKepServerManager
    {
        /// <summary>
        /// 获取服务状态
        /// </summary>
        /// <returns></returns>
        KepServerState GetServerStatus();

        /// <summary>
        /// 服务开关
        /// </summary>
        KepServerState StateSwitch();

        /// <summary>
        /// 推送日志
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        void PublishLog(string msg);

        /// <summary>
        /// 客户端认证
        /// </summary>
        /// <param name="arg"></param>
        /// <returns></returns>
        bool OnAuthentication(ZepCommon.Identity arg);

        /// <summary>
        /// 客户端上线
        /// </summary>
        /// <param name="obj"></param>
        void OnClientOnline(KepConversation obj);

        /// <summary>
        /// 客户端下线
        /// </summary>
        /// <param name="obj"></param>
        void OnClientOffline(string obj);
    }
}
