//namespace BDInSelfLove.Services.Data.Tests
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading.Tasks;

//    using BDInSelfLove.Data;
//    using BDInSelfLove.Data.Models;
//    using BDInSelfLove.Data.Repositories;
//    using BDInSelfLove.Services.Data.Articles;
//    using BDInSelfLove.Services.Data.Comments;
//    using Microsoft.EntityFrameworkCore;
//    using Xunit;

//    public class ArticleServiceTests
//    {
//        private ArticleService articleService;

//        public ArticleServiceTests()
//        {
//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                  .UseInMemoryDatabase(Guid.NewGuid().ToString());
//            var dbContext = new ApplicationDbContext(options.Options);
//            var articleRepository = new EfDeletableEntityRepository<Article>(dbContext);
//            var commentRepository = new EfDeletableEntityRepository<Comment>(dbContext);
//            var commentService = new CommentService(commentRepository);
//            var articleService = new ArticleService(commentService, articleRepository);
//            this.articleService = articleService;
//        }

//        // Create
//        [Fact]
//        public async Task CreateReturnsSlug()
//        {
//            var article = new Article()
//            {
//                Title = "TEST Test",
//                ImageUrl = "test",
//                Content = "test",
//                Slug = "test-test",
//            };

//            var slug = await this.articleService.Create(article);
//            Assert.True(slug.Equals(article.Title.ToLower().Replace(' ', '-')));
//        }

//        [Theory]
//        [InlineData(null, "test", "test")]
//        [InlineData("test", null, "test")]
//        [InlineData("test", "test", null)]
//        [InlineData("", "test", "test")]
//        [InlineData("test", "", "test")]
//        [InlineData("test", "test", "")]
//        [InlineData("   ", "test", "test")]
//        [InlineData("test", "   ", "test")]
//        [InlineData("test", "test", "   ")]
//        public async Task CreateIncompleteArticleThrowsArgumentException(string title, string imageUrl, string content)
//        {
//            var article = new Article()
//            {
//                Title = title,
//                ImageUrl = imageUrl,
//                Content = content,
//            };

//            await Assert.ThrowsAsync<ArgumentException>(() => this.articleService.Create(article));
//        }

//        [Fact]
//        public async Task CreateWithSameIdTwiceThrowsArgumentException()
//        {
//            var article = new Article()
//            {
//                Id = 1,
//                Title = "test",
//                ImageUrl = "test",
//                Content = "test",
//                Slug = "test",
//            };

//            var slug = await this.articleService.Create(article);
//            await Assert.ThrowsAsync<ArgumentException>(() => this.articleService.Create(article));
//        }

//        [Fact]
//        public async Task CreateWithNullArgumentThrowsArgumentException()
//        {
//            await Assert.ThrowsAsync<ArgumentException>(() => this.articleService.Create(null));
//        }

//        // GetBySlug
//        [Fact]
//        public async Task GetBySlugReturnsCorrectArticle()
//        {
//            await this.ClearArticles();

//            var article = new Article()
//            {
//                Title = "test",
//                ImageUrl = "test",
//                Content = "test",
//                Slug = "test",
//            };

//            var slug = await this.articleService.Create(article);
//            var dbArticle = await this.articleService.GetBySlug(slug);

//            Assert.Equal(article.Title, dbArticle.Title);
//            Assert.Equal(article.ImageUrl, dbArticle.ImageUrl);
//            Assert.Equal(article.Content, dbArticle.Content);
//        }

//        [Fact]
//        public async Task GetBySlugReturnsCorrectArticleAndComments()
//        {
//            await this.ClearArticles();

//            var article = new Article()
//            {
//                Title = "test",
//                ImageUrl = "test",
//                Content = "test",
//                Slug = "test",
//                Comments = new List<Comment>
//                {
//                    new Comment
//                    {
//                        Content = "test test",
//                        ArticleId = 1,
//                        User = new ApplicationUser
//                        {
//                            UserName = "Pesho",
//                            ProfilePhoto = "test",
//                        },
//                    },
//                    new Comment
//                    {
//                        Content = "test",
//                        ArticleId = 1,
//                        User = new ApplicationUser
//                        {
//                            UserName = "Pesho1",
//                            ProfilePhoto = "test1",
//                        },
//                    },
//                },
//            };

//            var slug = await this.articleService.Create(article);
//            var dbArticle = await this.articleService.GetBySlug(slug);

//            var articleComments = article.Comments.OrderByDescending(c => c.CreatedOn).ToArray();
//            var dbArticleComments = dbArticle.Comments.ToArray();

//            for (int i = 0; i < articleComments.Length; i++)
//            {
//                Assert.Equal(articleComments[i].Content, dbArticleComments[i].Content);
//                Assert.Equal(articleComments[i].CreatedOn, dbArticleComments[i].CreatedOn);
//                Assert.Equal(articleComments[i].User.UserName, dbArticleComments[i].User.UserName);
//                Assert.Equal(articleComments[i].User.ProfilePhoto, dbArticleComments[i].User.ProfilePhoto);
//            }
//        }

//        [Theory]
//        [InlineData("does-not-exist")]
//        [InlineData("   ")]
//        [InlineData("---")]
//        [InlineData(null)]
//        public async Task GetBySlugReturnsNullWhenRequestingNonExistentArticle(string slug)
//        {
//            await this.ClearArticles();

//            var nonExistentArticle = await this.articleService.GetBySlug(slug);

//            Assert.Null(nonExistentArticle);
//        }

//        // Edit
//        [Fact]
//        public async Task EditThrowsArgumentExceptionWhenGivenNullArticle()
//        {
//            await this.ClearArticles();

//            await Assert.ThrowsAsync<ArgumentException>(() => this.articleService.Edit(null));
//        }

//        [Fact]
//        public async Task EditThrowsArgumentExceptionWhenGivenNonExistentArticle()
//        {
//            await this.ClearArticles();
//            var article = new Article { Id = 1 };

//            await Assert.ThrowsAsync<ArgumentException>(() => this.articleService.Edit(article));
//        }

//        [Theory]
//        [InlineData("", "", "")]
//        [InlineData(null, null, null)]
//        [InlineData("", "test", "test")]
//        [InlineData("test", "", "test")]
//        [InlineData("test", "test", "")]
//        [InlineData(" ", "test", "test")]
//        [InlineData("test", " ", "test")]
//        [InlineData("test", "test", " ")]
//        public async Task EditThrowsArgumentExceptionWhenGivenInvalidArticle(string title, string imageUrl, string content)
//        {
//            await this.ClearArticles();

//            var article = new Article()
//            {
//                Title = "test",
//                ImageUrl = "test",
//                Content = "test",
//                Slug = "test",
//            };

//            var slug = await this.articleService.Create(article);
//            var invalidArticle = await this.articleService.GetBySlug(slug);

//            invalidArticle.Title = title;
//            invalidArticle.ImageUrl = imageUrl;
//            invalidArticle.Content = content;

//            await Assert.ThrowsAsync<ArgumentException>(() => this.articleService.Edit(invalidArticle));
//        }

//        [Fact]
//        public async Task EditUpdatesEntityAndReturnsSlug()
//        {
//            await this.ClearArticles();

//            var article = new Article()
//            {
//                Title = "test",
//                ImageUrl = "test",
//                Content = "test",
//                Slug = "test",
//            };

//            var slug = await this.articleService.Create(article);
//            var updatedArticle = await this.articleService.GetBySlug(slug);

//            updatedArticle.Title = "test1 TEST1";
//            updatedArticle.ImageUrl = "test1";
//            updatedArticle.Content = "test1";
//            updatedArticle.Slug = "test1-test1";

//            await this.articleService.Edit(updatedArticle);
//            var dbUpdatedArticle = await this.articleService.GetBySlug(updatedArticle.Slug);

//            Assert.Equal(dbUpdatedArticle.Title, updatedArticle.Title);
//            Assert.Equal(dbUpdatedArticle.ImageUrl, updatedArticle.ImageUrl);
//            Assert.Equal(dbUpdatedArticle.Content, updatedArticle.Content);
//        }

//        // Delete
//        [Fact]
//        public async Task DeleteIsSuccessfulWithValidId()
//        {
//            await this.ClearArticles();

//            var article = new Article()
//            {
//                Title = "test",
//                ImageUrl = "test",
//                Content = "test",
//                Slug = "test",
//            };

//            var slug = await this.articleService.Create(article);
//            var dbArticle = await this.articleService.GetBySlug(slug);
//            var result = await this.articleService.Delete(dbArticle.Id);

//            Assert.Equal(1, result);
//        }

//        [Fact]
//        public async Task DeleteThrowsArgumentExceptionWithInvaildId()
//        {
//            await this.ClearArticles();

//            await Assert.ThrowsAsync<ArgumentException>(() => this.articleService.Delete(1));
//        }

//        // GetAll
//        [Fact]
//        public async Task GetAllWithNoParametersReturnsAllArticles()
//        {
//            await this.ClearArticles();
//            var articles = (await this.SeedData()).OrderByDescending(x => x.CreatedOn).ToList();
//            var dbArticles = await this.articleService.GetAll().ToListAsync();

//            Assert.Equal(articles.Count, dbArticles.Count);

//            for (int i = 0; i < articles.Count; i++)
//            {
//                Assert.Equal(articles[i].Title, dbArticles[i].Title);
//                Assert.Equal(articles[i].ImageUrl, dbArticles[i].ImageUrl);
//                Assert.Equal(articles[i].Content, dbArticles[i].Content);
//            }
//        }

//        [Theory]
//        [InlineData(1, 1)]
//        [InlineData(1, 2)]
//        public async Task GetAllSkipsAndTakesCorrectly(int take, int skip)
//        {
//            await this.ClearArticles();
//            var articles = (await this.SeedData()).OrderByDescending(x => x.CreatedOn).Skip(skip).Take(take).ToList();
//            var dbArticles = await this.articleService.GetAll(take, skip).ToListAsync();

//            Assert.Equal(articles.Count, dbArticles.Count);

//            foreach (var article in articles)
//            {
//                Assert.Contains(dbArticles, a => a.Title.Equals(article.Title));
//                Assert.Contains(dbArticles, a => a.ImageUrl.Equals(article.ImageUrl));
//                Assert.Contains(dbArticles, a => a.Content.Equals(article.Content));
//            }
//        }

//        [Theory]
//        [InlineData("test")]
//        [InlineData("test1")]
//        [InlineData("test2")]
//        [InlineData("test2 test1")]
//        public async Task GetAllSearchesCorrectly(string searchString)
//        {
//            await this.ClearArticles();
//            var articles = await this.SeedData();
//            var dbArticles = await this.articleService.GetAll(null, 0, searchString).ToListAsync();

//            var filteredArticles = new List<Article>();
//            string[] searchTermsArray = SearchHelper.GetSearchItems(searchString);

//            foreach (var term in searchTermsArray)
//            {
//                var articlesThatIncludeTerm = articles.Where(a => a.Title.Contains(term) || a.Content.Contains(term));
//                filteredArticles.AddRange(articlesThatIncludeTerm);
//            }

//            filteredArticles = filteredArticles.Distinct().OrderByDescending(x => x.CreatedOn).ToList();

//            Assert.Equal(filteredArticles.Count, dbArticles.Count);

//            foreach (var filteredArticle in filteredArticles)
//            {
//                Assert.Contains(dbArticles, a => a.Title.Equals(filteredArticle.Title));
//                Assert.Contains(dbArticles, a => a.ImageUrl.Equals(filteredArticle.ImageUrl));
//                Assert.Contains(dbArticles, a => a.Content.Equals(filteredArticle.Content));
//            }
//        }

//        // GetById
//        [Theory]
//        [InlineData(1)]
//        [InlineData(2)]
//        [InlineData(3)]
//        public async Task GetByIdReturnsCorrectArticle(int id)
//        {
//            await this.ClearArticles();
//            await this.SeedData();

//            var dbArticle = await this.articleService.GetById(id).FirstOrDefaultAsync();
//            Assert.Equal(dbArticle.Id, id);
//        }

//        [Fact]
//        public async Task GetByIdReturnsNullWithInvalidId()
//        {
//            await this.ClearArticles();

//            Assert.Null(await this.articleService.GetById(123).FirstOrDefaultAsync());
//        }

//        // GetSideArticles
//        [Theory]
//        [InlineData(1, 1)]
//        [InlineData(2, 2)]
//        [InlineData(3, 1)]
//        public async Task GetSideArticlesReturnsCorrectData(int id, int count)
//        {
//            //await this.ClearArticles();
//            //await this.SeedData();

//            //var dbArticles = await this.articleService.GetSideArticles(count, id).ToListAsync();

//            //Assert.True(!dbArticles.Any(a => a.Id == id));
//            //Assert.True(dbArticles.Count == count);
//        }

//        private async Task ClearArticles()
//        {
//            var allEntries = await this.articleService.GetAll().ToListAsync();

//            foreach (var article in allEntries)
//            {
//                await this.articleService.Delete(article.Id);
//            }
//        }

//        private async Task<List<Article>> SeedData()
//        {
//            List<Article> articles = new List<Article>
//            {
//                new Article()
//                {
//                    Title = "test",
//                    ImageUrl = "test",
//                    Content = "test",
//                    Slug = "test",
//                },
//                new Article()
//                {
//                    Title = "test1",
//                    ImageUrl = "test1",
//                    Content = "test1",
//                    Slug = "test1",
//                },
//                new Article()
//                {
//                    Title = "test2",
//                    ImageUrl = "test2",
//                    Content = "test2",
//                    Slug = "test2",
//                },
//            };

//            foreach (var article in articles)
//            {
//                await this.articleService.Create(article);
//            }

//            return articles;
//        }
//    }
//}
