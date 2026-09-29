using Keystone.Model.System;
using Keystone.Model.System.Dto;

namespace Keystone.ServiceCore.Services
{
    public interface ISysPermissionService
    {
        public List<string> GetRolePermission(SysUserDto user);
        public List<string> GetMenuPermission(SysUserDto user);
    }
}
