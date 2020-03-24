namespace BDInSelfLove.Services.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Article;

    public interface IArticleService
    {
        IQueryable<ArticleServiceModel> GetAll(int? count = null);

        Task<int> CreateAsync(ArticleServiceModel articleServiceModel);

        Task<ArticleServiceModel> GetById(int id);

        Task<int> Edit(ArticleServiceModel productServiceModel);

        Task<bool> Delete(int id);
    }
}
