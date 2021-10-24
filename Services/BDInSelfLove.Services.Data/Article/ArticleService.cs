namespace BDInSelfLove.Services.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.CommentService;
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

        public IQueryable<Article> GetAll(int? take = null, int skip = 0, string searchString = null)
        {
            var query = this.articleRepository.All();

            if (searchString != null)
            {
                var searchItems = SearchHelpers.GetSearchItems(searchString);
                foreach (var item in searchItems)
                {
                    // TODO: need to check why I needed this
                    var tempItem = item;

                    query = query.Where(a =>
                    a.Content.ToLower().Contains(tempItem) ||
                    a.Title.ToLower().Contains(tempItem));
                }
            }

            query = query.Distinct().OrderByDescending(a => a.CreatedOn).Skip(skip);

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
            var article = await this.articleRepository.All()
                .Where(a => a.Title.ToLower().Equals(slug.Replace('-', ' ')))
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
