using Keystone.Model;
using Keystone.Model.System;
using Keystone.Model.System.Dto;

namespace Keystone.ServiceCore.Services
{
    public interface ISysTasksQzService : IBaseService<SysTasks>
    {
        PagedInfo<SysTasks> SelectTaskList(TasksQueryDto parm);
        //SysTasksQz GetId(object id);
        int AddTasks(SysTasks parm);
        int UpdateTasks(SysTasks parm);
    }
}
