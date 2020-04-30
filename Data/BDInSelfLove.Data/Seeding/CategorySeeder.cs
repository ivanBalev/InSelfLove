namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.DependencyInjection;

    public class CategorySeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Categories.Any())
            {
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminUser = await userManager.FindByNameAsync("admin");

            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Clinical Psychology",
                    Description = @"This treatment-oriented branch of psychology deals with scientific ways of handling psychological problems. Also called counselling psychology or psychotherapy, it focuses on the prevention, understanding and cure of psychological issues by way of psycho-therapeutic treatment.",
                    UserId = adminUser.Id,
                },
                new Category
                {
                    Name = "Biopsychology",
                    Description = @"This branch of psychology looks at the role the brain and neurotransmitters play in influencing our thoughts, feelings and behaviours. It combines neuroscience and the study of psychology.",
                    UserId = adminUser.Id,
                },
                new Category
                {
                    Name = "Educational Psychology",
                    Description = @"Educational psychology is the scientific study of human behaviour in an educational setting and, as such, it deals with issues such as learning disorders, adolescence behaviours, and so on. These studies focus primarily on the different developmental stages of children and teenagers.",
                    UserId = adminUser.Id,
                },
                new Category
                {
                    Name = "Cognitive Psychology",
                    Description = @"The branch of psychology that deals with mental processes, such as thoughts, memory and problem solving, is called cognitive psychology. In essence, it is concerned with the perception and problem-solving capability of the brain.",
                    UserId = adminUser.Id,
                },
                new Category
                {
                    Name = "Forensic Psychology",
                    Description = @"The application of psychology to law making, law enforcement, the examination of witnesses, and the treatment of the criminal is the job of the forensic psychologist. Also known as legal psychology, this branch of psychology is not dissimilar to cognitive and clinical psychology, but involves a thorough understanding of the law.",
                    UserId = adminUser.Id,
                },
                new Category
                {
                    Name = "Social Psychology",
                    Description = @"Focussed on the psychological aspects of individuals within a community environment, community psychology explores characteristics such as interdependence, adaptation, diplomacy, empowerment, social justice, and so on. It is also referred to as critical psychology.",
                    UserId = adminUser.Id,
                },
            };

            await dbContext.Categories.AddRangeAsync(categories);
        }
    }
}
