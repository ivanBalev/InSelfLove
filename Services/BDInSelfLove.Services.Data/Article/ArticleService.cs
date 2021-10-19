namespace BDInSelfLove.Services.Data
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.CommentService;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;
    using Microsoft.EntityFrameworkCore;

    public class ArticleService : IArticleService
    {
        private readonly ICommentService commentService;
        private readonly IDeletableEntityRepository<Article> articleRepository;

        public ArticleService(
            ICommentService commentService,
            IDeletableEntityRepository<Article> articleRepository)
        {
            this.articleRepository = articleRepository;
            this.commentService = commentService;
        }

        public async Task<string> Create(Article article)
        {
            await this.articleRepository.AddAsync(article);
            await this.articleRepository.SaveChangesAsync();
            return article.Title.ToLower().Replace(' ', '-');
        }

        public async Task<int> Delete(int id)
        {
            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == id);
            this.articleRepository.Delete(dbArticle);
            return await this.articleRepository.SaveChangesAsync();
        }

        public async Task<string> Edit(Article articleServiceModel)
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

        public IQueryable<Article> GetAll(int? take = null, int skip = 0)
        {
            var query = this.articleRepository.All().OrderByDescending(a => a.CreatedOn).Skip(skip);

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }

        public IQueryable<Article> GetById(int id)
        {
            return this.articleRepository.All().Where(a => a.Id == id);
        }

        public async Task<ArticleServiceModel> GetBySlug(string slug)
        {
            var article = await this.articleRepository.All()
               .Where(a => a.Title.ToLower() == slug.Replace('-', ' '))
               .To<ArticleServiceModel>()
               .FirstOrDefaultAsync();

            article.Comments = this.commentService.ArrangeCommentHierarchy(article.Comments);
            return article;
        }

        public IQueryable<Article> GetSideArticles(int articlesCount, int articleId = 0)
        {
            var articles = this.articleRepository.All()
               .Where(a => a.Id != articleId)
               .OrderByDescending(a => a.CreatedOn)
               .Take(articlesCount);

            return articles;
        }
    }
}
