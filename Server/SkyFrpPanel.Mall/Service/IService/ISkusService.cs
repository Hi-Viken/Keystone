using SkyFrpPanel.Mall.Model;
using SkyFrpPanel.Mall.Model.Dto;

namespace SkyFrpPanel.Mall.Service.IService
{
    /// <summary>
    /// 商品库存service接口
    /// </summary>
    public interface ISkusService : IBaseService<Skus>
    {
        PagedInfo<SkusDto> GetList(ShoppingSkusQueryDto parm);
        Skus GetInfo(long SkuId);
        List<Skus> GetSkus(long ProductId);
        Skus AddShoppingSkus(Skus parm);
        long UpdateShoppingSkus(Skus parm);
    }
}
