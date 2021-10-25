namespace BDInSelfLove.Services.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface IArticleService
    {
        IQueryable<Article> GetAll(int? take = null, int skip = 0, string searchString = null);

        Task<string> Create(Article articleServiceModel);

        IQueryable<Article> GetById(int id);

        Task<Article> GetBySlug(string slug);

        Task<string> Edit(Article productServiceModel);

        Task<int> Delete(int id);

        IQueryable<Article> GetSideArticles(int articlesCount, int articleId = 0);
    }
}
