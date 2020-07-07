namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.DependencyInjection;

    internal class ArticleSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Articles.Count() > 0)
            {
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminUser = await userManager.FindByNameAsync("admin");

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Test1",
                    Content = "Test1Content",
                    ImageUrl = "https://source.unsplash.com/random/800x600",
                    UserId = adminUser.Id,
                },
                new Article
                {
                    Title = "Test2",
                    Content = "Test2Content",
                    ImageUrl = "https://source.unsplash.com/random/800x601",
                    UserId = adminUser.Id,
                },
                new Article
                {
                    Title = "Test3",
                    Content = "Test3Content",
                    ImageUrl = "https://source.unsplash.com/random/801x600",
                    UserId = adminUser.Id,
                },
                new Article
                {
                    Title = "Test4",
                    Content = "Test4Content",
                    ImageUrl = "https://source.unsplash.com/random/800x602",
                    UserId = adminUser.Id,
                },
            };

            await dbContext.Articles.AddRangeAsync(articles);
        }
    }
}
