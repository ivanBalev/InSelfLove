namespace BDInSelfLove.Services.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Articles;

    public interface IArticleService
    {
        IQueryable<ArticleServiceModel> GetAll(int? count = null);

        Task<int> CreateAsync(ArticleServiceModel articleServiceModel);
    }
}
