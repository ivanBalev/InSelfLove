namespace BDInSelfLove.Services.Data.Articles
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
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
            // Validate input
            if (article == null ||
               string.IsNullOrEmpty(article.Title) || string.IsNullOrWhiteSpace(article.Title) ||
               string.IsNullOrEmpty(article.Content) || string.IsNullOrWhiteSpace(article.Content) ||
               string.IsNullOrEmpty(article.ImageUrl) || string.IsNullOrWhiteSpace(article.ImageUrl))
            {
                throw new ArgumentException(nameof(article));
            }

            // Create new article
            await this.articleRepository.AddAsync(article);
            await this.articleRepository.SaveChangesAsync();

            // Return slug to controller for redirect
            return article.Slug;
        }

        public async Task<string> Edit(Article article)
        {
            // Validate input
            if (article == null ||
               string.IsNullOrEmpty(article.Title) || string.IsNullOrWhiteSpace(article.Title) ||
               string.IsNullOrEmpty(article.Content) || string.IsNullOrWhiteSpace(article.Content) ||
               string.IsNullOrEmpty(article.ImageUrl) || string.IsNullOrWhiteSpace(article.ImageUrl))
            {
                throw new ArgumentException(nameof(article));
            }

            // Get article, make sure it's unique & throw error if it doesn't exist
            var dbArticle = await this.articleRepository.All().SingleOrDefaultAsync(a => a.Id == article.Id);
            if (dbArticle == null)
            {
                throw new ArgumentException(nameof(dbArticle));
            }

            // Update article
            dbArticle.Title = article.Title;
            dbArticle.Content = article.Content;
            dbArticle.ImageUrl = article.ImageUrl;
            dbArticle.PreviewImageUrl = article.PreviewImageUrl;
            dbArticle.Slug = article.Slug;

            // Make sure date is valid
            // If no value is given to DateTime from client
            // It defaults to 01.01.0001
            if (article.CreatedOn.Year > 1)
            {
                dbArticle.CreatedOn = article.CreatedOn;
            }

            // Update article in db
            this.articleRepository.Update(dbArticle);
            await this.articleRepository.SaveChangesAsync();

            // Return slug to controller for redirect
            return dbArticle.Slug;
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

        // Method is used in both Articles & Search controller
        public IQueryable<Article> GetAll(int? take = null, int skip = 0, string searchString = null)
        {
            var query = this.articleRepository.All();

            // If client is searching for particular phrases (Request coming from Search controller)
            if (!string.IsNullOrEmpty(searchString) && !string.IsNullOrWhiteSpace(searchString))
            {
                // Leave only meaningful words
                var searchItems = SearchHelper.GetSearchItems(searchString);

                // Build query with articles which content or title matches the search terms
                query = query.Search(x => x.Content.ToLower(), x => x.Title.ToLower()).Containing(searchItems);
            }

            // Order data & skip if using search pagination
            query = query.OrderByDescending(c => c.CreatedOn).Skip(skip);

            // if take has no value, then skip will be 0 as well => no pagination used
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

        public async Task<Article> GetBySlug(string slug, string userTimezone)
        {
            var article = await this.articleRepository.All()
                .Where(a => a.Slug.Equals(slug.ToLower()))
                .Select(x => new Article
                {
                    // Ensure query efficiency by picking only relevant data
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

                        // Instantiate list for ArrangeCommentHierarchy
                        SubComments = new List<Comment>(),
                    })),
                })
                .FirstOrDefaultAsync();

            // No need to proceed further if article does not exist
            if (article == null)
            {
                return null;
            }

            // Create comments hierarchy tree and order by date
            // We get a unidimensional list of comments from db
            // Its nesting needs to be sorted out for the client
            article.Comments = this.commentService.ArrangeCommentHierarchy(article.Comments);

            // Adjust comments CreatedOn to user's local time
            foreach (var comment in article.Comments)
            {
                comment.CreatedOn = TimezoneHelper.ToLocalTime(comment.CreatedOn, userTimezone);
            }

            return article;
        }

        // Returns recommendation for other articles the user might want to check out
        public async Task<IList<Article>> GetSideArticles(int articlesCount, DateTime date)
        {
            // TODO: would really prefer to do randomization in db rather than in memory.
            // At present, it seems it requires doing changes to the dbContext class that
            // would entail changes for all other methods in the current service
            // Namely not creating a table in memory that corresponds to the one in sql db
            // Which would enable the queries to be executed. At present, they're being
            // mixed with the automatically-generated query by EF.

            // Take articles older than current one
            var articles = await this.articleRepository.All()
               .Where(a => DateTime.Compare(a.CreatedOn, date) < 0)
               .OrderByDescending(a => a.CreatedOn)
               .Take(articlesCount)
               .ToListAsync();

            // If they're not enough (article is one of the older ones)
            if (articles.Count < articlesCount)
            {
                var additionalArticlesNeeded = articlesCount - articles.Count;

                // Get the number of articles needed from the newer ones
                articles.AddRange(await this.articleRepository.All()
               .Where(a => DateTime.Compare(a.CreatedOn, date) > 0)
               .OrderBy(a => a.CreatedOn)
               .Take(additionalArticlesNeeded)
               .ToListAsync());
            }

            return articles;
        }
    }
}
