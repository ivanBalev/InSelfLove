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
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Comments.Count() > 0)
            {
                return;
            }

           
        }
    }
}
