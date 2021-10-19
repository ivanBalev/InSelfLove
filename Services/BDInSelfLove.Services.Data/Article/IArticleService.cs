namespace BDInSelfLove.Services.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Models.Article;

    public interface IArticleService
    {
        IQueryable<Article> GetAll(int? take = null, int skip = 0);

        Task<string> Create(Article articleServiceModel);

        IQueryable<Article> GetById(int id);

        Task<ArticleServiceModel> GetBySlug(string slug);

        Task<string> Edit(Article productServiceModel);

        Task<int> Delete(int id);

        IQueryable<Article> GetSideArticles(int articlesCount, int articleId = 0);
    }
}
