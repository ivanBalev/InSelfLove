namespace BDInSelfLove.Services.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;
    using Microsoft.EntityFrameworkCore;

    public class ArticleService : IArticleService
    {
        private const int DefaultArticlesPerPage = 6;

        private readonly IDeletableEntityRepository<Article> articleRepository;

        public ArticleService(IDeletableEntityRepository<Article> articleRepository)
        {
            this.articleRepository = articleRepository;
        }

        public async Task<int> CreateAsync(ArticleServiceModel articleServiceModel)
        {
            var article = AutoMapperConfig.MapperInstance.Map<Article>(articleServiceModel);

            await this.articleRepository.AddAsync(article);
            await this.articleRepository.SaveChangesAsync();

            return article.Id;
        }

        public async Task<bool> Delete(int id)
        {
            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == id);

            if (dbArticle == null)
            {
                throw new ArgumentNullException(nameof(dbArticle));
            }

            this.articleRepository.Delete(dbArticle);
            int result = await this.articleRepository.SaveChangesAsync();

            return result > 0;
        }

        public async Task<int> Edit(ArticleServiceModel articleServiceModel)
        {
            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == articleServiceModel.Id);

            if (dbArticle == null)
            {
                throw new ArgumentNullException(nameof(dbArticle));
            }

            dbArticle.Title = articleServiceModel.Title;
            dbArticle.Content = articleServiceModel.Content;
            dbArticle.ImageUrl = articleServiceModel.ImageUrl;

            this.articleRepository.Update(dbArticle);
            int result = await this.articleRepository.SaveChangesAsync();

            return result;
        }

        public IQueryable<ArticleServiceModel> GetAll(int? latestArticlesCount = null)
        {
            IQueryable<Article> query = this.articleRepository.AllAsNoTracking().OrderByDescending(a => a.CreatedOn);

            if (latestArticlesCount.HasValue)
            {
                query = query.Take(latestArticlesCount.Value);
            }

            return query.To<ArticleServiceModel>();
        }

        public IQueryable<ArticleServiceModel> GetAllPagination(int take = DefaultArticlesPerPage, int skip = 0)
        {
            var articles = this.articleRepository
                               .All()
                               .OrderByDescending(a => a.CreatedOn)
                               .Skip(skip)
                               .Take(take)
                               .To<ArticleServiceModel>();

            return articles;
        }

        public async Task<ArticleServiceModel> GetById(int id)
        {
            var article = await this.articleRepository.All()
               .Where(a => a.Id == id)
               .Include(a => a.User)
               .Include(a => a.ArticleComments)
               .To<ArticleServiceModel>()
               .FirstOrDefaultAsync();

            foreach (var comment in article.ArticleComments)
            {
                comment.SubComments.Clear();
            }

            foreach (var comment in article.ArticleComments)
            {
                if (comment.ParentCommentId != null)
                {
                    var parentComment = article.ArticleComments.SingleOrDefault(x => x.Id == comment.ParentCommentId);
                    parentComment.SubComments.Add(comment);
                }
            }

            article.ArticleComments = article.ArticleComments.Where(c => c.ParentCommentId == null).ToList();

            return article;
        }
    }
}
