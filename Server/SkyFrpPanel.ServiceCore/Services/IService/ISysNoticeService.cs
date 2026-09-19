using SkyFrpPanel.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Model.System.Dto;

namespace SkyFrpPanel.ServiceCore.Services
{
    /// <summary>
    /// 通知公告表service接口
    ///
    /// @author zr
    /// @date 2021-12-15
    /// </summary>
    public interface ISysNoticeService : IBaseService<SysNotice>
    {
        List<SysNotice> GetSysNotices();

        PagedInfo<SysNotice> GetPageList(SysNoticeQueryDto parm);
        PagedInfo<SysNoticeDto> ExportList(SysNoticeQueryDto parm);
    }
}
