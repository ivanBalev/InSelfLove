namespace BDInSelfLove.Services.Data
{
    using System;
    using System.Collections.Generic;
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

        public async Task<string> CreateAsync(ArticleServiceModel articleServiceModel)
        {
            var article = AutoMapperConfig.MapperInstance.Map<Article>(articleServiceModel);

            await this.articleRepository.AddAsync(article);
            await this.articleRepository.SaveChangesAsync();

            return article.Title.ToLower().Replace(' ', '-');
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

        public async Task<string> Edit(ArticleServiceModel articleServiceModel)
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

            return dbArticle.Title.ToLower().Replace(' ', '-');
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

        public IQueryable<ArticlePreviewServiceModel> GetAllPagination(int take = DefaultArticlesPerPage, int skip = 0)
        {
            var articles = this.articleRepository
                               .All()
                               .OrderByDescending(a => a.CreatedOn)
                               .Skip(skip)
                               .Take(take)
                               .To<ArticlePreviewServiceModel>();

            return articles;
        }

        public async Task<ArticleServiceModel> GetById(int id)
        {
            var article = await this.articleRepository.All()
               .Where(a => a.Id == id)
               .Include(a => a.User)
               .Include(a => a.Comments.OrderByDescending(c => c.CreatedOn))
               .To<ArticleServiceModel>()
               .FirstOrDefaultAsync();

            foreach (var comment in article.Comments)
            {
                comment.SubComments.Clear();
            }

            foreach (var comment in article.Comments)
            {
                if (comment.ParentCommentId != null)
                {
                    var parentComment = article.Comments.SingleOrDefault(x => x.Id == comment.ParentCommentId);
                    parentComment.SubComments.Add(comment);
                }
            }

            article.Comments = article.Comments.Where(c => c.ParentCommentId == null).ToList();

            return article;
        }

        public async Task<ArticleServiceModel> GetBySlug(string slug)
        {
            var article = await this.articleRepository.All()
               .Where(a => a.Title.ToLower() == slug.Replace('-', ' '))
               .Include(a => a.User)
               .Include(a => a.Comments.OrderByDescending(c => c.CreatedOn))
               .To<ArticleServiceModel>()
               .FirstOrDefaultAsync();

            foreach (var comment in article.Comments)
            {
                comment.SubComments.Clear();
            }

            foreach (var comment in article.Comments)
            {
                if (comment.ParentCommentId != null)
                {
                    var parentComment = article.Comments.SingleOrDefault(x => x.Id == comment.ParentCommentId);
                    parentComment.SubComments.Add(comment);
                }
            }

            article.Comments = article.Comments.Where(c => c.ParentCommentId == null).ToList();

            return article;
        }

        public IQueryable<ArticlePreviewServiceModel> GetSideArticles(int articlesCount, int articleId = 0)
        {
            var articles = this.articleRepository.All()
               .Where(a => a.Id != articleId)
               .OrderByDescending(a => a.CreatedOn)
               .Take(articlesCount)
               .To<ArticlePreviewServiceModel>();

            return articles;
        }
    }
}
