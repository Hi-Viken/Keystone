using SkyFrpPanel.Model;
using SkyFrpPanel.Model.System;
using SkyFrpPanel.Model.System.Dto;

namespace SkyFrpPanel.ServiceCore.Services
{
    public interface ISysTasksQzService : IBaseService<SysTasks>
    {
        PagedInfo<SysTasks> SelectTaskList(TasksQueryDto parm);
        //SysTasksQz GetId(object id);
        int AddTasks(SysTasks parm);
        int UpdateTasks(SysTasks parm);
    }
}
