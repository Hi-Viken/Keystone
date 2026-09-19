using SkyFrpPanel.Model.System;
using SkyFrpPanel.Model.System.Dto;

namespace SkyFrpPanel.ServiceCore.Services
{
    public interface ISysPermissionService
    {
        public List<string> GetRolePermission(SysUserDto user);
        public List<string> GetMenuPermission(SysUserDto user);
    }
}
