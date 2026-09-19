using SkyFrpPanel.Mall.Model;
using SkyFrpPanel.Mall.Model.Dto;

namespace SkyFrpPanel.Mall.Service.IService
{
    /// <summary>
    /// 商品管理service接口
    /// </summary>
    public interface IProductService : IBaseService<Product>
    {
        PagedInfo<ProductDto> GetList(ShoppingProductQueryDto parm);
        ProductDto GetInfo(long ProductId);
        Product AddShoppingProduct(ProductDto parm);
        Product UpdateShoppingProduct(ProductDto parm);
        long UpdateInfo(ProductDto dto);
        PagedInfo<ProductDto> ExportList(ShoppingProductQueryDto parm);
    }
}
