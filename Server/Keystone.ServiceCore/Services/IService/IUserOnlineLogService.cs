using Keystone.Model;
using Keystone.Model.Dto;
using Keystone.Model.Models;
using Keystone.ServiceCore.Signalr;

namespace Keystone.ServiceCore.Monitor.IMonitorService
{
    /// <summary>
    /// 用户在线时长service接口
    /// </summary>
    public interface IUserOnlineLogService : IBaseService<UserOnlineLog>
    {
        PagedInfo<UserOnlineLogDto> GetList(UserOnlineLogQueryDto parm);

        Task<UserOnlineLog> AddUserOnlineLog(UserOnlineLog parm, OnlineUsers onlineUsers);

        PagedInfo<UserOnlineLogDto> ExportList(UserOnlineLogQueryDto parm);
    }
}
