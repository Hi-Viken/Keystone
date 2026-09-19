using SkyFrpPanel.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Model.System.Dto;

namespace SkyFrpPanel.ServiceCore.Services
{
    /// <summary>
    /// 数据差异日志service接口
    /// </summary>
    public interface ISqlDiffLogService : IBaseService<SqlDiffLog>
    {
        PagedInfo<SqlDiffLogDto> GetList(SqlDiffLogQueryDto parm);

        SqlDiffLog GetInfo(long PId);

        SqlDiffLog AddSqlDiffLog(SqlDiffLog parm);

        int UpdateSqlDiffLog(SqlDiffLog parm);

    }
}
