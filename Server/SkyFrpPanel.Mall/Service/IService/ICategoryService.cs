using SkyFrpPanel.Mall.Model;
using SkyFrpPanel.Mall.Model.Dto;

namespace SkyFrpPanel.Mall.Service.IService
{
    public interface ICategoryService : IBaseService<Category>
    {
        PagedInfo<CategoryDto> GetList(ShoppingCategoryQueryDto parm);
        List<CategoryDto> GetTreeList(ShoppingCategoryQueryDto parm);
        int AddCategory(Category parm);
        PagedInfo<CategoryDto> ExportList(ShoppingCategoryQueryDto parm);
    }
}
