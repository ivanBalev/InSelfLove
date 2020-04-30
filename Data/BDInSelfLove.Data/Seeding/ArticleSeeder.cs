﻿namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.DependencyInjection;

    internal class ArticleSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Articles.Any())
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
                    ImageUrl = "https://source.unsplash.com/random/300x200",
                    UserId = adminUser.Id,
                },
                new Article
                {
                    Title = "Test2",
                    Content = "Test2Content",
                    ImageUrl = "https://source.unsplash.com/random/300x201",
                    UserId = adminUser.Id,
                },
                new Article
                {
                    Title = "Test3",
                    Content = "Test3Content",
                    ImageUrl = "https://source.unsplash.com/random/300x199",
                    UserId = adminUser.Id,
                },
                new Article
                {
                    Title = "Test4",
                    Content = "Test4Content",
                    ImageUrl = "https://source.unsplash.com/random/301x200",
                    UserId = adminUser.Id,
                },
            };

            await dbContext.Articles.AddRangeAsync(articles);
        }
    }
}
