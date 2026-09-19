using SkyFrpPanel.Model.Content;
using SkyFrpPanel.Model.Content.Dto;

namespace SkyFrpPanel.Service.Content.IService
{
    public interface IArticleCategoryService : IBaseService<ArticleCategory>
    {
        PagedInfo<ArticleCategoryDto> GetList(ArticleCategoryQueryDto parm);
        List<ArticleCategoryDto> GetTreeList(ArticleCategoryQueryDto parm);
        int AddArticleCategory(ArticleCategory parm);

        int PlusJoinNum(int categoryId);
        int ReduceJoinNum(int categoryId);
    }
}
