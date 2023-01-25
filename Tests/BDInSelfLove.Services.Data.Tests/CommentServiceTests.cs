//namespace BDInSelfLove.Services.Data.Tests
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Threading.Tasks;

//    using BDInSelfLove.Data;
//    using BDInSelfLove.Data.Models;
//    using BDInSelfLove.Data.Repositories;
//    using BDInSelfLove.Services.Data.Comments;
//    using Microsoft.EntityFrameworkCore;
//    using Xunit;

//    public class CommentServiceTests
//    {
//        private EfDeletableEntityRepository<Comment> commentRepository;
//        private CommentService commentService;

//        public CommentServiceTests()
//        {
//            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
//                .UseInMemoryDatabase(Guid.NewGuid().ToString());
//            var dbContext = new ApplicationDbContext(options.Options);
//            var commentRepository = new EfDeletableEntityRepository<Comment>(dbContext);
//            var commentService = new CommentService(commentRepository);
//            this.commentRepository = commentRepository;
//            this.commentService = commentService;
//        }

//        // Create
//        [Fact]
//        public async Task CreateWorksCorrectlyWithValidData()
//        {
//            var comment = new Comment
//            {
//                UserId = Guid.NewGuid().ToString(),
//                Content = "test",
//                ArticleId = 1,
//            };

//            var comment1 = new Comment
//            {
//                UserId = Guid.NewGuid().ToString(),
//                Content = "test",
//                VideoId = 1,
//            };

//            var commentId = await this.commentService.Create(comment);
//            var comment1Id = await this.commentService.Create(comment1);

//            Assert.Equal(1, commentId);
//            Assert.Equal(2, comment1Id);
//        }

//        [Theory]
//        [InlineData("", "test", 1)]
//        [InlineData("test", "", 1)]
//        [InlineData("test", "test", null)]
//        [InlineData(null, "test", 1)]
//        [InlineData("test", null, 1)]
//        [InlineData("   ", "test", 1)]
//        [InlineData("test", "   ", 1)]

//        public async Task CreateThrowsArgumentExceptionWithInvalidData(string userId, string content, int? articleId)
//        {
//            var comment = new Comment
//            {
//                UserId = userId,
//                Content = content,
//                ArticleId = articleId,
//            };

//            await Assert.ThrowsAsync<ArgumentException>(() => this.commentService.Create(comment));
//        }

//        // SetCommentDepth
//        [Fact]
//        public async Task SetCommentDepth()
//        {
//            await this.ClearComments();
//            var comments = await this.SeedData();

//            var comment = new Comment
//            {
//                UserId = Guid.NewGuid().ToString(),
//                Content = "test",
//                ArticleId = 1,
//                ParentCommentId = comments.FirstOrDefault(c => c.Content == "thirdLevelSubcomment").Id,
//            };

//            await this.commentService.SetCommentDepth(comment);

//            Assert.Equal(comment.ParentCommentId, comments.FirstOrDefault(c => c.Content == "firstLevelSubcomment").Id);
//        }

//        // Edit
//        [Fact]
//        public async Task EditWorksCorrectlyWithValidData()
//        {
//            await this.ClearComments();
//            await this.SeedData();

//            var comment = await this.commentService.GetById(1).FirstOrDefaultAsync();
//            comment.Content = "Edited";
//            var result = await this.commentService.Edit(comment, comment.UserId);

//            Assert.Equal(1, result);
//        }

//        [Fact]
//        public async Task EditReturnsZeroWithInvalidData()
//        {
//            await this.ClearComments();
//            await this.SeedData();

//            var comment = await this.commentService.GetById(1).FirstOrDefaultAsync();
//            comment.Content = "Edited";
//            var result = await this.commentService.Edit(comment, Guid.NewGuid().ToString());
//            var result1 = await this.commentService.Edit(null, null);
//            var result2 = await this.commentService.Edit(comment, string.Empty);
//            var result3 = await this.commentService.Edit(comment, "   ");

//            comment.Id = int.MaxValue;
//            var result4 = await this.commentService.Edit(comment, comment.UserId);
//            Assert.Equal(0, result);
//            Assert.Equal(0, result1);
//            Assert.Equal(0, result2);
//            Assert.Equal(0, result3);
//            Assert.Equal(0, result4);
//        }

//        // Delete
//        [Fact]
//        public async Task DeleteWorksFineWithAdmin()
//        {
//            await this.ClearComments();
//            var comments = await this.SeedData();

//            var mainComment = comments.FirstOrDefault(c => c.Content == "mainComment");

//            var result = await this.commentService.Delete(mainComment.Id, null, true);
//            var commentsInRepo = await this.commentRepository.All().ToListAsync();
//            Assert.True(result > 0);
//            Assert.True(commentsInRepo.Count == 0);
//        }

//        [Fact]
//        public async Task DeleteWorksFineWithUserCreatorOfComment()
//        {
//            await this.ClearComments();
//            var comments = await this.SeedData();

//            var mainComment = comments.FirstOrDefault(c => c.Content == "mainComment");

//            var result = await this.commentService.Delete(mainComment.Id, mainComment.UserId, false);
//            var commentsInRepo = await this.commentRepository.All().ToListAsync();
//            Assert.True(result > 0);
//            Assert.True(commentsInRepo.Count == 0);
//        }

//        [Fact]
//        public async Task DeleteDoesNotWorkWithRandomUser()
//        {
//            await this.ClearComments();
//            var comments = await this.SeedData();

//            var mainComment = comments.FirstOrDefault(c => c.Content == "mainComment");

//            var result = await this.commentService.Delete(mainComment.Id, Guid.NewGuid().ToString(), false);
//            var commentsInRepo = await this.commentRepository.All().ToListAsync();
//            Assert.Equal(0, result);
//            Assert.True(commentsInRepo.Count == comments.Count);
//        }

//        // ArrangeCommentHierarchy
//        [Fact]
//        public async Task ArrangeCommentHierarchyWorksFine()
//        {
//            await this.ClearComments();
//            var comments = await this.SeedData();

//            var firstLevelSubcomment1 = new Comment
//            {
//                ParentCommentId = comments.FirstOrDefault(c => c.Content == "mainComment").Id,
//                UserId = Guid.NewGuid().ToString(),
//                Content = "firstLevelSubcomment1",
//                ArticleId = 1,
//            };

//            await this.commentService.Create(firstLevelSubcomment1);

//            var allDbComments = (await this.commentRepository.AllAsNoTracking().ToListAsync())
//                .Select(c =>
//                {
//                    c.SubComments = new List<Comment>();
//                    return c;
//                })
//                .ToList();
//            allDbComments = this.commentService.ArrangeCommentHierarchy(allDbComments).ToList();

//            // Latest first level subcomment is first in list
//            var firstLevelSubcommentsAreOrderedCorrectly = allDbComments[0].SubComments[0]
//                .Content.Equals(firstLevelSubcomment1.Content);
//            var firstLevelSubcommentsCount = allDbComments[0].SubComments.Count;

//            // Latest second level subcomment is first in list
//            var secondLevelSubcommentsAreOrderedCorrectly = allDbComments[0].SubComments
//                .FirstOrDefault(c => c.Content.Equals("firstLevelSubcomment"))
//                .SubComments[0].Content.Equals("thirdLevelSubcomment");
//            var secondLevelSubcommentsCount = allDbComments[0].SubComments
//                .FirstOrDefault(c => c.Content.Equals("firstLevelSubcomment"))
//                .SubComments.Count;

//            // Hierarchy is correct
//            Assert.Single(allDbComments);
//            Assert.True(firstLevelSubcommentsAreOrderedCorrectly);
//            Assert.True(firstLevelSubcommentsCount == 2);
//            Assert.True(secondLevelSubcommentsAreOrderedCorrectly);
//            Assert.True(secondLevelSubcommentsCount == 2);
//        }

//        private async Task ClearComments()
//        {
//            var allEntries = await this.commentRepository.All().ToListAsync();

//            foreach (var comment in allEntries)
//            {
//                this.commentRepository.HardDelete(comment);
//            }

//            await this.commentRepository.SaveChangesAsync();
//        }

//        private async Task<List<Comment>> SeedData()
//        {
//            var mainComment = new Comment
//            {
//                UserId = "1",
//                Content = "mainComment",
//                ArticleId = 1,
//            };
//            var mainCommentId = await this.commentService.Create(mainComment);

//            var firstLevelSubcomment = new Comment
//            {
//                ParentCommentId = mainCommentId,
//                UserId = "2",
//                Content = "firstLevelSubcomment",
//                ArticleId = 1,
//            };
//            var firstLevelSubcommentId = await this.commentService.Create(firstLevelSubcomment);

//            var secondLevelSubcomment = new Comment
//            {
//                ParentCommentId = firstLevelSubcommentId,
//                UserId = "3",
//                Content = "secondLevelSubcomment",
//                ArticleId = 1,
//            };
//            var secondLevelSubcommentId = await this.commentService.Create(secondLevelSubcomment);

//            // Third level subcomments don't actully exist as per service logic
//            var thirdLevelSubcomment = new Comment
//            {
//                ParentCommentId = secondLevelSubcommentId,
//                UserId = "4",
//                Content = "thirdLevelSubcomment",
//                ArticleId = 1,
//            };
//            await this.commentService.Create(thirdLevelSubcomment);

//            // Prevent EF from tracking
//            firstLevelSubcomment.SubComments = new List<Comment>
//            {
//                thirdLevelSubcomment,
//                secondLevelSubcomment,
//            };

//            mainComment.SubComments = new List<Comment>
//            {
//                firstLevelSubcomment,
//            };

//            return new List<Comment>
//            {
//                mainComment,
//                firstLevelSubcomment,
//                secondLevelSubcomment,
//                thirdLevelSubcomment,
//            };
//        }
//    }
//}
