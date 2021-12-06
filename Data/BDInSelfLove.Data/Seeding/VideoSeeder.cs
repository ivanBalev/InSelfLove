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
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/WTH1Q0dqjnE",
                    Title = "Test2 Test2",
                    AssociatedTerms = "hora",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/TmRhhn-YFBc",
                    Title = "Test3 Test3",
                    AssociatedTerms = "hora",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/xxR7rcR2dXY",
                    Title = "Test4 Test4",
                    AssociatedTerms = "hora",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/WTH1Q0dqjnE",
                    Title = "Test5 Test5",
                    AssociatedTerms = "hora",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/TmRhhn-YFBc",
                    Title = "Test6 Test6",
                    AssociatedTerms = "hora",
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/xxR7rcR2dXY",
                    Title = "Test7 Test7",
                    AssociatedTerms = "hora",
                },
            };

            await dbContext.Videos.AddRangeAsync(videos);
        }
    }
}
