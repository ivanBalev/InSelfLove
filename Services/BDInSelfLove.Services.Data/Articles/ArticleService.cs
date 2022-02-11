namespace BDInSelfLove.Services.Data.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Comments;
    using Microsoft.EntityFrameworkCore;
    using NinjaNye.SearchExtensions;

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
            if (article == null ||
               string.IsNullOrEmpty(article.Title) || string.IsNullOrWhiteSpace(article.Title) ||
               string.IsNullOrEmpty(article.Content) || string.IsNullOrWhiteSpace(article.Content) ||
               string.IsNullOrEmpty(article.ImageUrl) || string.IsNullOrWhiteSpace(article.ImageUrl))
            {
                throw new ArgumentException(nameof(article));
            }

            await this.articleRepository.AddAsync(article);
            await this.articleRepository.SaveChangesAsync();
            return article.Title.ToLower().Replace(' ', '-');
        }

        public async Task<string> Edit(Article article)
        {
            if (article == null ||
               string.IsNullOrEmpty(article.Title) || string.IsNullOrWhiteSpace(article.Title) ||
               string.IsNullOrEmpty(article.Content) || string.IsNullOrWhiteSpace(article.Content) ||
               string.IsNullOrEmpty(article.ImageUrl) || string.IsNullOrWhiteSpace(article.ImageUrl))
            {
                throw new ArgumentException(nameof(article));
            }

            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == article.Id);

            if (dbArticle == null)
            {
                throw new ArgumentException(nameof(dbArticle));
            }

            dbArticle.Title = article.Title;
            dbArticle.Content = article.Content;
            dbArticle.ImageUrl = article.ImageUrl;
            dbArticle.Slug = article.Slug;

            this.articleRepository.Update(dbArticle);
            await this.articleRepository.SaveChangesAsync();

            return dbArticle.Title.ToLower().Replace(' ', '-');
        }

        public async Task<int> Delete(int id)
        {
            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == id);

            if (dbArticle == null)
            {
                throw new ArgumentException(nameof(dbArticle));
            }

            this.articleRepository.Delete(dbArticle);
            return await this.articleRepository.SaveChangesAsync();
        }

        public IQueryable<Article> GetAll(int? take = null, int skip = 0, string searchString = null)
        {
            var query = this.articleRepository.All();

            if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrWhiteSpace(searchString))
            {
                var searchItems = SearchHelper.GetSearchItems(searchString);
                query = query.Search(x => x.Content.ToLower(), x => x.Title.ToLower()).Containing(searchItems);
            }

            query = query.OrderByDescending(c => c.CreatedOn.Date)
                .ThenByDescending(c => c.CreatedOn.TimeOfDay).Skip(skip);

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

        public async Task<Article> GetBySlug(string slug)
        {
            if (slug == null)
            {
                return null;
            }

            var article = await this.articleRepository.All()
                .Where(a => a.Slug.Equals(slug.ToLower()))
                .Select(x => new Article
                {
                    Id = x.Id,
                    Title = x.Title,
                    CreatedOn = x.CreatedOn,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    Comments = new List<Comment>(x.Comments.Select(c => new Comment
                    {
                        Id = c.Id,
                        Content = c.Content,
                        UserId = c.UserId,
                        User = new ApplicationUser
                        {
                            UserName = c.User.UserName,
                            ProfilePhoto = c.User.ProfilePhoto,
                        },
                        ArticleId = c.ArticleId,
                        ParentCommentId = c.ParentCommentId,
                        CreatedOn = c.CreatedOn,
                        SubComments = new List<Comment>(),
                    })),
                })
                .FirstOrDefaultAsync();

            if (article == null)
            {
                return null;
            }

            article.Comments = this.commentService.ArrangeCommentHierarchy(article.Comments);
            return article;
        }

        public IQueryable<Article> GetSideArticles(int articlesCount, int articleId = 0)
        {
            var articles = this.articleRepository.All()
               .Where(a => a.Id != articleId)
               .OrderByDescending(c => c.CreatedOn.Date)
               .ThenByDescending(c => c.CreatedOn.TimeOfDay)
               .Take(articlesCount);

            return articles;
        }
    }
}
