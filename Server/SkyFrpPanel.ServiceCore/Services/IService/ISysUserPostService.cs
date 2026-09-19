using SkyFrpPanel.Model.System;
using SkyFrpPanel.Model.System.Model;

namespace SkyFrpPanel.ServiceCore.Services
{
    public interface ISysUserPostService
    {
        public void InsertUserPost(SysUser user);

        public List<long> GetUserPostsByUserId(long userId);

        public string GetPostsStrByUserId(long userId);
        bool Delete(long userId);
    }
}
