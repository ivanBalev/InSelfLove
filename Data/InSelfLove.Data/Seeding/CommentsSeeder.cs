namespace InSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using Microsoft.EntityFrameworkCore;

    internal class CommentsSeeder : ISeeder
    {
        public async Task SeedAsync(MySqlDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Comments.Count() > 0)
            {
                return;
            }

            var articleId = 6;

            var usersIds = await dbContext.Users.Select(u => u.Id).ToListAsync();
            var comment = new Comment
            {
                ArticleId = articleId,
                UserId = usersIds.FirstOrDefault(),
                Content = "comment 1",
            };

            await dbContext.Comments.AddAsync(comment);
            await dbContext.SaveChangesAsync();

            var subcomment = new Comment
            {
                ArticleId = articleId,
                UserId = usersIds.LastOrDefault(),
                Content = "comment 2",
                ParentCommentId = 1,
            };

            await dbContext.Comments.AddAsync(subcomment);
            await dbContext.SaveChangesAsync();

            var subcommentLevel2 = new Comment
            {
                ArticleId = articleId,
                UserId = usersIds.FirstOrDefault(),
                Content = "comment 3",
                ParentCommentId = 2,
            };

            await dbContext.Comments.AddAsync(subcommentLevel2);
            await dbContext.SaveChangesAsync();

            var subcommentLevel3 = new Comment
            {
                ArticleId = articleId,
                UserId = usersIds.LastOrDefault(),
                Content = "comment 4",
                ParentCommentId = 2,
            };

            await dbContext.Comments.AddAsync(subcommentLevel3);
            await dbContext.SaveChangesAsync();
        }
    }
}
