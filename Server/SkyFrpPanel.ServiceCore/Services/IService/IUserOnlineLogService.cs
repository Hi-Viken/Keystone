using SkyFrpPanel.Model;
using SkyFrpPanel.Model.Dto;
using SkyFrpPanel.Model.Models;
using SkyFrpPanel.ServiceCore.Signalr;

namespace SkyFrpPanel.ServiceCore.Monitor.IMonitorService
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
