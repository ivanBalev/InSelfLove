namespace InSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.DependencyInjection;

    internal class VideoSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Videos.Count() > 0)
            {
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminUser = await userManager.FindByNameAsync("admin");

            var videos = new List<Video>
            {
                new Video
                {
                    Url = "https://www.youtube.com/embed/C0ZsDVZgl_g",
                    Title = "Test1",
                    AssociatedTerms = "hora",
                    Slug = "test1",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/WTH1Q0dqjnE",
                    Title = "Test2 Test2",
                    AssociatedTerms = "hora",
                    Slug = "test2-test2",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/TmRhhn-YFBc",
                    Title = "Test3 Test3",
                    AssociatedTerms = "hora",
                    Slug = "test3-test3",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/xxR7rcR2dXY",
                    Title = "Test4 Test4",
                    AssociatedTerms = "hora",
                    Slug = "test4-test4",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/WTH1Q0dqjnE",
                    Title = "Test5 Test5",
                    AssociatedTerms = "hora",
                    Slug = "test5-test5",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/TmRhhn-YFBc",
                    Title = "Test6 Test6",
                    AssociatedTerms = "hora",
                    Slug = "test6-test6",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/xxR7rcR2dXY",
                    Title = "Test7 Test7",
                    AssociatedTerms = "hora",
                    Slug = "test7-test7",
                },
            };

            await dbContext.Videos.AddRangeAsync(videos);
        }
    }
}
