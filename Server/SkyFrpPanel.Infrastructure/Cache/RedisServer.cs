using CSRedis;
using SkyFrpPanel.Infrastructure;

namespace SkyFrpPanel.Common.Cache
{
    public class RedisServer
    {
        public static CSRedisClient Cache = null!;
        public static CSRedisClient Session = null!;

        public static void Initalize()
        {
            Cache = new CSRedisClient(AppSettings.GetConfig("RedisServer:Cache"));
            Session = new CSRedisClient(AppSettings.GetConfig("RedisServer:Session"));
        }
    }
}
