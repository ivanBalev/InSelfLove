namespace InSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    internal class ArticleSeeder : ISeeder
    {
        public async Task SeedAsync(MySqlDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Articles.Count() > 0)
            {
                return;
            }

            var articles = new List<Article>
            {
                new Article
                {
                    Title = "Test1 Test1",
                    Content = "Test1Content",
                    ImageUrl = "https://source.unsplash.com/random/800x600",
                    Slug = "test1-test1",
                },
                new Article
                {
                    Title = "Test2 Test2",
                    Content = "Test2Content",
                    ImageUrl = "https://source.unsplash.com/random/800x601",
                    Slug = "test2-test2",
                },
                new Article
                {
                    Title = "Test3",
                    Content = "Test3Content",
                    ImageUrl = "https://source.unsplash.com/random/801x600",
                    Slug = "test3",
                },
                new Article
                {
                    Title = "Test4",
                    Content = "Test4Content",
                    ImageUrl = "https://source.unsplash.com/random/800x602",
                    Slug = "test4",
                },
                new Article
                {
                    Title = "Test5",
                    Content = "Test5Content",
                    ImageUrl = "https://source.unsplash.com/random/800x602",
                    Slug = "test5",
                },
                new Article
                {
                    Title = "Test6 Test6",
                    Content = "Test6Content",
                    ImageUrl = "https://source.unsplash.com/random/800x602",
                    Slug = "test6-test6",
                },
                new Article
                {
                    Title = "Test7 Test7",
                    Content = "Test7Content",
                    ImageUrl = "https://source.unsplash.com/random/800x602",
                    Slug = "test7-test7",
                },
            };

            for (int i = 0; i < articles.Count; i++)
            {
                articles[i].CreatedOn = DateTime.UtcNow.AddMinutes(i);
            }

            await dbContext.Articles.AddRangeAsync(articles);
        }
    }
}
