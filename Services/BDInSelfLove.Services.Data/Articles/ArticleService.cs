namespace BDInSelfLove.Services.Data
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Articles;

    public class ArticleService : IArticleService
    {
        private readonly IDeletableEntityRepository<Article> articlesRepository;

        public ArticleService(IDeletableEntityRepository<Article> articlesRepository)
        {
            this.articlesRepository = articlesRepository;
        }

        public async Task<int> CreateAsync(ArticleServiceModel articleServiceModel)
        {
            var article = AutoMapperConfig.MapperInstance.Map<Article>(articleServiceModel);

            await this.articlesRepository.AddAsync(article);
            var result = await this.articlesRepository.SaveChangesAsync();

            return article.Id;
        }

        public IQueryable<ArticleServiceModel> GetAll(int? latestArticlesCount = null)
        {
            IQueryable<Article> query = this.articlesRepository.AllAsNoTracking().OrderByDescending(a => a.CreatedOn);

            if (latestArticlesCount.HasValue)
            {
                query = query.Take(latestArticlesCount.Value);
            }

            return query.To<ArticleServiceModel>();
        }
    }
}
