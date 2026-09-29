using Keystone.Model.System;
using Keystone.Model.System.Model;

namespace Keystone.ServiceCore.Services
{
    public interface ISysUserPostService
    {
        public void InsertUserPost(SysUser user);

        public List<long> GetUserPostsByUserId(long userId);

        public string GetPostsStrByUserId(long userId);
        bool Delete(long userId);
    }
}
