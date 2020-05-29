namespace BDInSelfLove.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data;
    using BDInSelfLove.Data.Repositories;
    using BDInSelfLove.Services.Data.Category;
    using BDInSelfLove.Services.Data.Category.CategorySorting;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Data.Tests.Common.Seeders;
    using BDInSelfLove.Services.Models.Category;
    using BDInSelfLove.Web.InputModels.Forum.Category;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Xunit;

    public class CategoryServiceTests : SqliteSetup
    {
        public CategoryServiceTests()
        {
            MapperInitializer.InitializeMapper();
        }

        [Fact]
        public async Task GetHomeCategoryInfo_ShouldReturnCorrect_NumberOfCategoriesAndPosts()
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            var categoriesFromDb = await categoryService.GetHomeCategoryInfo().ToListAsync();
            var expectedCategories = this.GetTestCategories();
            var expectedPosts = this.GetTestPosts();

            Assert.True(expectedCategories.Length == categoriesFromDb.Count);

            var postsCountForFirstCategory = categoriesFromDb.First().PostsCount;
            var expectedPostsCountForFirstCategory = expectedPosts
                .Where(p => p.CategoryId == categoriesFromDb.First().Id).Count();

            Assert.True(postsCountForFirstCategory == expectedPostsCountForFirstCategory);
        }

        [Fact]
        public async Task GetHomeCategoryInfo_ShouldReturnCorrect_CategoryNamesAndLastPosts()
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            var categoriesFromDb = await categoryService.GetHomeCategoryInfo().ToListAsync();
            var expectedCategories = await repository.All().Include(c => c.Posts).ToListAsync();

            for (int i = 0; i < categoriesFromDb.Count; i++)
            {
                Assert.Equal(categoriesFromDb[i].Name, expectedCategories[i].Name);

                var actualLastPost = expectedCategories[i].Posts.OrderByDescending(p => p.CreatedOn).FirstOrDefault();
                Assert.Equal(categoriesFromDb[i].LastPost?.Title, actualLastPost?.Title);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsAllPosts(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            var newPostId = this.GetTestPosts().Length + 1;
            await context.Posts.AddAsync(new BDInSelfLove.Data.Models.Post
            {
                CreatedOn = DateTime.UtcNow.AddDays(-66),
                CategoryId = categoryId,
            });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.AllPosts,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.DateCreated,
            };

            var categoryWithSortedPosts = await categoryService.GetById(categoryId, sortingModel);
            var expectedPosts = context.Posts.Where(p => p.CategoryId == categoryId);

            Assert.Equal(expectedPosts.Count(), categoryWithSortedPosts.Posts.Count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsCorrectPosts_ForCurrentDay(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            var newPostId = this.GetTestPosts().Length + 1;
            await context.Posts.AddAsync(new BDInSelfLove.Data.Models.Post
            {
                CreatedOn = DateTime.UtcNow.AddDays(-66),
                CategoryId = categoryId,
                Id = newPostId,
            });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Day,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.DateCreated,
            };

            var categoryWithSortedPosts = await categoryService.GetById(categoryId, sortingModel);
            var expectedPosts = this.GetTestPosts().Where(p => p.CategoryId == categoryId);

            Assert.Equal(expectedPosts.Count(), categoryWithSortedPosts.Posts.Count);
            Assert.DoesNotContain(categoryWithSortedPosts.Posts, p => p.Id == newPostId);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsCorrectPosts_ForCurrentMonth(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            await context.Posts.AddRangeAsync(
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(-66),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(66),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow,
                    CategoryId = categoryId,
                });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Month,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.DateCreated,
            };

            var categoryWithSortedPosts = await categoryService.GetById(categoryId, sortingModel);
            var expectedPosts = await context.Posts.Where(p => p.CreatedOn.Month == DateTime.UtcNow.Month &&
                                                         p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                         p.CategoryId == categoryId)
                                                         .ToListAsync();

            Assert.Equal(expectedPosts.Count(), categoryWithSortedPosts.Posts.Count);
            Assert.True(categoryWithSortedPosts.Posts.All(p => expectedPosts.Any(x => x.Id == p.Id)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsCorrectPosts_ForCurrentYear(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            await context.Posts.AddRangeAsync(
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(-366),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(366),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow,
                    CategoryId = categoryId,
                });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Year,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.DateCreated,
            };

            var categoryWithSortedPosts = await categoryService.GetById(categoryId, sortingModel);
            var expectedPosts = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                               .ToListAsync();

            Assert.Equal(expectedPosts.Count(), categoryWithSortedPosts.Posts.Count);
            Assert.True(categoryWithSortedPosts.Posts.All(p => expectedPosts.Any(x => x.Id == p.Id)));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsPosts_InCorrectOrder_ByDateCreatedDescending(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            await context.Posts.AddRangeAsync(
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(-366),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(366),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow,
                    CategoryId = categoryId,
                });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Year,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.DateCreated,
            };

            var categoryWithSortedPosts = await categoryService.GetById(categoryId, sortingModel);
            var expectedPosts = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderByDescending(p => p.CreatedOn)
                                                   .ToListAsync();

            Assert.Equal(expectedPosts.First().Id, categoryWithSortedPosts.Posts.First().Id);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsPosts_InCorrectOrder_ByDateCreatedAscending(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            await context.Posts.AddRangeAsync(
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(-366),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow.AddDays(366),
                    CategoryId = categoryId,
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CreatedOn = DateTime.UtcNow,
                    CategoryId = categoryId,
                });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Year,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Ascending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.DateCreated,
            };

            var categoryWithSortedPosts = await categoryService.GetById(categoryId, sortingModel);
            var expectedPosts = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderBy(p => p.CreatedOn)
                                                   .ToListAsync();

            Assert.Equal(expectedPosts.First().Id, categoryWithSortedPosts.Posts.First().Id);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsPosts_InCorrectOrder_ByAuthor_DescendingAndAscending(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            await context.Posts.AddRangeAsync(
                new BDInSelfLove.Data.Models.Post
                {
                    CategoryId = categoryId,
                    UserId = "1",
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CategoryId = categoryId,
                    UserId = "1",
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CategoryId = categoryId,
                    UserId = "1",
                });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Year,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.Author,
            };

            var categoryWithSortedPostsDescending = await categoryService.GetById(categoryId, sortingModel);
            var expectedPostsDescending = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderByDescending(p => p.User.UserName)
                                                   .ToListAsync();

            sortingModel.OrderingCriterion = CategorySortingValues.OrderingCriteria.Ascending;

            var categoryWithSortedPostsAscending = await categoryService.GetById(categoryId, sortingModel);
            var expectedPostsAscending = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderBy(p => p.User.UserName)
                                                   .ToListAsync();

            Assert.Equal(expectedPostsDescending.First().Id, categoryWithSortedPostsDescending.Posts.First().Id);
            Assert.Equal(expectedPostsAscending.First().Id, categoryWithSortedPostsAscending.Posts.First().Id);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsPosts_InCorrectOrder_ByReplies_DescendingAndAscending(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            await context.Comments.AddAsync(
                new BDInSelfLove.Data.Models.Comment
                {
                    ParentPostId = 1,
                    Id = 4,
                    UserId = "1",
                    Content = "Test4",
                });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Year,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.Replies,
            };

            var categoryWithSortedPostsDescending = await categoryService.GetById(categoryId, sortingModel);
            var expectedPostsDescending = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderByDescending(p => p.Comments.Count)
                                                   .ToListAsync();

            sortingModel.OrderingCriterion = CategorySortingValues.OrderingCriteria.Ascending;

            var categoryWithSortedPostsAscending = await categoryService.GetById(categoryId, sortingModel);
            var expectedPostsAscending = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderBy(p => p.Comments.Count)
                                                   .ToListAsync();

            Assert.Equal(expectedPostsDescending.First().Id, categoryWithSortedPostsDescending.Posts.First().Id);
            Assert.Equal(expectedPostsAscending.First().Id, categoryWithSortedPostsAscending.Posts.First().Id);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public async Task GetById_ReturnsPosts_InCorrectOrder_ByTopic_DescendingAndAscending(int categoryId)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            await context.Posts.AddAsync(
                new BDInSelfLove.Data.Models.Post
                {
                    CategoryId = 1,
                    Id = 4,
                    UserId = "1",
                    Title = "Test4",
                });
            await context.SaveChangesAsync();

            var sortingModel = new CategorySortingInputModel
            {
                CategoryId = categoryId,
                TimeCriterion = CategorySortingValues.TimeCriteria.Year,
                OrderingCriterion = CategorySortingValues.OrderingCriteria.Descending,
                GroupingCriterion = CategorySortingValues.GroupingCriteria.Topic,
            };

            var categoryWithSortedPostsDescending = await categoryService.GetById(categoryId, sortingModel);
            var expectedPostsDescending = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderByDescending(p => p.Title)
                                                   .ToListAsync();

            sortingModel.OrderingCriterion = CategorySortingValues.OrderingCriteria.Ascending;

            var categoryWithSortedPostsAscending = await categoryService.GetById(categoryId, sortingModel);
            var expectedPostsAscending = await context.Posts.Where(p => p.CreatedOn.Year == DateTime.UtcNow.Year &&
                                                               p.CategoryId == categoryId)
                                                   .OrderBy(p => p.Title)
                                                   .ToListAsync();

            Assert.Equal(expectedPostsDescending.First().Title, categoryWithSortedPostsDescending.Posts.First().Title);
            Assert.Equal(expectedPostsAscending.First().Title, categoryWithSortedPostsAscending.Posts.First().Title);
        }

        [Fact]
        public async Task Create_ShouldWorkCorrectly()
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            var categoryServiceModel = new CategoryServiceModel
            {
                Name = "Test4",
                UserId = "1",
            };

            var result = await categoryService.Create(categoryServiceModel);
            Assert.True(result != 0);
        }

        [Fact]
        public async Task Create_ShouldThrowExceptionsCorrectly()
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            var categoryServiceModel = new CategoryServiceModel
            {
                Name = "Test4",
                UserId = "1",
                Id = 1,
            };

            await Assert.ThrowsAsync<DbUpdateException>(() => categoryService.Create(categoryServiceModel));
        }

        [Theory]
        [InlineData("Test", 3)]
        [InlineData("Test1", 1)]
        [InlineData("Test2", 1)]
        [InlineData("Test3", 1)]
        [InlineData("just some random text", 0)]
        public async Task Search_ShouldWorkCorrectly(string searchTerm, int expectedResultsCount)
        {
            this.SetupSqlite();
            await this.SeedDatabase();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category>(context);
            var categoryService = this.GetCategoryService(repository);

            var resultsCount = (await categoryService.Search(searchTerm).ToListAsync()).Count;
            Assert.Equal(resultsCount, expectedResultsCount);
        }

        private async Task SeedDatabase()
        {
            using var context = new ApplicationDbContext(this.ContextOptions);

            await context.Users.AddRangeAsync(UserCreator.GetTestUsers());
            await context.Categories.AddRangeAsync(this.GetTestCategories());
            await context.Posts.AddRangeAsync(this.GetTestPosts());
            await context.Comments.AddRangeAsync(this.GetTestComments());
            await context.SaveChangesAsync();
        }

        private BDInSelfLove.Data.Models.Comment[] GetTestComments()
        {
            return new BDInSelfLove.Data.Models.Comment[]
            {
                new BDInSelfLove.Data.Models.Comment
                {
                    ParentPostId = 1,
                    Id = 1,
                    UserId = "1",
                    Content = "Test1",
                },
                new BDInSelfLove.Data.Models.Comment
                {
                    ParentPostId = 1,
                    Id = 2,
                    UserId = "1",
                    Content = "Test2",
                },
                new BDInSelfLove.Data.Models.Comment
                {
                    ParentPostId = 2,
                    Id = 3,
                    UserId = "1",
                    Content = "Test3",
                },
            };
        }

        private BDInSelfLove.Data.Models.Post[] GetTestPosts()
        {
            return new BDInSelfLove.Data.Models.Post[]
            {
                new BDInSelfLove.Data.Models.Post
                {
                    CategoryId = 1,
                    Id = 1,
                    UserId = "1",
                    Title = "Test1",
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CategoryId = 1,
                    Id = 2,
                    UserId = "1",
                    Title = "Test2",
                },
                new BDInSelfLove.Data.Models.Post
                {
                    CategoryId = 2,
                    Id = 3,
                    UserId = "2",
                    Title = "Test3",
                },
            };
        }

        private BDInSelfLove.Data.Models.Category[] GetTestCategories()
        {
            return new BDInSelfLove.Data.Models.Category[]
                {
                    new BDInSelfLove.Data.Models.Category
                    {
                        Name = "CategoryTest1",
                        Description = "Test1",
                        UserId = "1",
                        Id = 1,
                    },
                    new BDInSelfLove.Data.Models.Category
                    {
                        Name = "CategoryTest2",
                        Description = "Test2",
                        UserId = "2",
                        Id = 2,
                    },
                    new BDInSelfLove.Data.Models.Category
                    {
                        Name = "CategoryTest3",
                        Description = "Test3",
                        UserId = "3",
                        Id = 3,
                    },
                };
        }

        private CategoryService GetCategoryService(EfDeletableEntityRepository<BDInSelfLove.Data.Models.Category> categoryRepository)
        {
            var commentServiceMock = new Mock<ICommentService>();
            var categoryService = new CategoryService(
                categoryRepository,
                commentServiceMock.Object);

            return categoryService;
        }
    }
}
