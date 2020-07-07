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
                    UserId = adminUser.Id,
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/WTH1Q0dqjnE",
                    UserId = adminUser.Id,
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/TmRhhn-YFBc",
                    UserId = adminUser.Id,
                },
                new Video
                {
                    Url = "https://www.youtube.com/embed/xxR7rcR2dXY",
                    UserId = adminUser.Id,
                },
            };

            await dbContext.Videos.AddRangeAsync(videos);
        }
    }
}
