using SkyFrpPanel.Infrastructure.Attribute;
using SkyFrpPanel.Model.social;
using SkyFrpPanel.Service.Social.IService;

namespace SkyFrpPanel.Service.Social
{
    [AppService(ServiceType = typeof(ISocialFansInfoService), ServiceLifetime = LifeTime.Transient)]
    public class SocialFansInfoService : BaseService<SocialFansInfo>, ISocialFansInfoService
    {
    }
}
