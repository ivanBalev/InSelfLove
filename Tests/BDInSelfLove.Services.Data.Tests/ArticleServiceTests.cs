namespace BDInSelfLove.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data;
    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Data.Repositories;
    using BDInSelfLove.Services.Models.Article;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Moq;
    using Xunit;

    public class ArticleServiceTests : IDisposable
    {
        public ArticleServiceTests()
        {
            MapperInitializer.InitializeMapper();
        }

        private DbConnection Connection { get; set; }

        private DbContextOptions<ApplicationDbContext> ContextOptions { get; set; }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 2)]
        [InlineData(3, 3)]
        public async Task GetByIdShouldReturnCorrectArticle(int input, int expected)
        {
            await this.SetupSqlite();
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
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var article = await articleService.GetById(input);

            Assert.Equal(article?.Id, expected);
        }

        [Fact]
        public async Task GetAllShouldReturnAllArticlesInCorrectOrder()
        {
            await this.SetupSqlite();
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
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            var articles = await articleService.GetAll(count).ToListAsync();

            Assert.Equal(count, articles.Count);
        }

        [Fact]
        public async Task EditShouldExecuteSuccessfully()
        {
            await this.SetupSqlite();
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
            await this.SetupSqlite();
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
            await this.SetupSqlite();
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
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Article>(context);
            var articleService = new ArticleService(repository);

            await Assert.ThrowsAsync<ArgumentNullException>(() => articleService.Delete(articleId));
        }

        [Fact]
        public async Task CreateShouldAddArticleToDatabaseSuccessfully()
        {
            await this.SetupSqlite();
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

        public void Dispose() => this.Connection?.Dispose();

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        private async Task Seed()
        {
            using var context = new ApplicationDbContext(this.ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            await context.Users.AddRangeAsync(this.GetTestUsers());
            await context.SaveChangesAsync();

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

        private ApplicationUser[] GetTestUsers()
        {
            return new ApplicationUser[]
            {
                new ApplicationUser { Id = "1" },
                new ApplicationUser { Id = "2" },
                new ApplicationUser { Id = "3" },
            };
        }

        private async Task SetupSqlite()
        {
            this.ContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseSqlite(CreateInMemoryDatabase())
              .Options;

            this.Connection = RelationalOptionsExtension.Extract(this.ContextOptions).Connection;
            await this.Seed();
        }
    }
}
