namespace BDInSelfLove.Services.Data.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Data.Repositories;
    using BDInSelfLove.Services.Data.Tests.Common.Seeders;
    using BDInSelfLove.Services.Models.Article;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class ArticleServiceTests : SqliteSetup
    {
        public ArticleServiceTests()
        {
            MapperInitializer.InitializeMapper();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        public async Task GetByIdShouldReturnCorrectArticle(int input, int expected)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var article = await articleService.GetById(input);

            Assert.IsType<ArticleServiceModel>(article);
            Assert.Equal(article.Id, expected);
        }

        [Theory]
        [InlineData(4, null)]
        [InlineData(5, null)]
        [InlineData(6, null)]
        public async Task GetByIdShouldReturnNullWithNonExistingId(int input, int? expected)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var article = await articleService.GetById(input);

            Assert.Equal(article?.Id, expected);
        }

        [Fact]
        public async Task GetAllShouldReturnAllArticlesInCorrectOrder()
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var articlesFromDb = await articleService.GetAll().ToListAsync();

            var generatedArticles = this.GetTestArticles();

            Assert.Equal(generatedArticles.Length, articlesFromDb.Count);
            for (var i = 0; i < generatedArticles.Length; i++)
            {
                Assert.Equal(generatedArticles[i].Id, articlesFromDb[articlesFromDb.Count - 1 - i].Id);
            }
        }

        [Theory]
        [InlineData(2)]
        public async Task GetAllShouldReturnOnlyRequestedNumberOfArticles(int count)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var articles = await articleService.GetAll(count).ToListAsync();

            Assert.Equal(count, articles.Count);
        }

        [Fact]
        public async Task EditShouldExecuteSuccessfully()
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var newArticleObj = new ArticleServiceModel { Id = 1, Content = "Test", ImageUrl = "Test", Title = "Test" };

            var result = await articleService.Edit(newArticleObj);
            var updatedArticle = repository.All().FirstOrDefault(a => a.Id == 1);

            Assert.Equal(1, result);
            Assert.Equal("Test", updatedArticle.Title);
            Assert.Equal("Test", updatedArticle.Content);
            Assert.Equal("Test", updatedArticle.ImageUrl);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(6)]
        public async Task EditShouldThrowExceptionsSuccessfully(int articleId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var newArticleObj = new ArticleServiceModel { Id = articleId, Content = "Test", ImageUrl = "Test", Title = "Test" };

            await Assert.ThrowsAsync<ArgumentNullException>(() => articleService.Edit(newArticleObj));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DeleteShouldExecuteSuccessfully(int articleId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var deleteResult = await articleService.Delete(articleId);
            Assert.True(deleteResult);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(6)]
        public async Task DeleteShouldThrowExceptionsSuccessfully(int articleId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            await Assert.ThrowsAsync<ArgumentNullException>(() => articleService.Delete(articleId));
        }

        [Fact]
        public async Task CreateShouldAddArticleToDatabaseSuccessfully()
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var articleToAdd = new ArticleServiceModel
            {
                Title = "Test",
                Content = "Test",
                ImageUrl = "Test",
                UserId = "1",
            };

            var articleId = await articleService.CreateAsync(articleToAdd);
            Assert.True(articleId != 0);
        }

        private async Task SeedDatabase()
        {
            using var context = new ApplicationDbContext(this.ContextOptions);

            await context.Users.AddRangeAsync(UserCreator.GetTestUsers());
            await context.Articles.AddRangeAsync(this.GetTestArticles());
            await context.SaveChangesAsync();
        }

        private Article[] GetTestArticles()
        {
            return new Article[]
                {
                    new Article
                    {
                        Title = "Test1",
                        Content = "Test1Content",
                        ImageUrl = "Test1ImageUrl",
                        UserId = "1",
                        Id = 1,
                    },
                    new Article
                    {
                        Title = "Test2",
                        Content = "Test2Content",
                        ImageUrl = "Test2ImageUrl",
                        UserId = "2",
                        Id = 2,
                    },
                    new Article
                    {
                        Title = "Test3",
                        Content = "Test3Content",
                        ImageUrl = "Test3ImageUrl",
                        UserId = "3",
                        Id = 3,
                    },
                };
        }
    }
}
