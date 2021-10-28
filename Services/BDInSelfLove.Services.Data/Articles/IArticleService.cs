namespace BDInSelfLove.Services.Data.Articles
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface IArticleService
    {
        Task<string> Create(Article article);

        Task<string> Edit(Article article);

        Task<int> Delete(int id);

        IQueryable<Article> GetAll(int? take = null, int skip = 0, string searchString = null);

        IQueryable<Article> GetById(int id);

        Task<Article> GetBySlug(string slug);

        IQueryable<Article> GetSideArticles(int articlesCount, int articleId = 0);
    }
}
