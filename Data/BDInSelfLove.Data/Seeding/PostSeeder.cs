namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.DependencyInjection;

    public class PostSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Posts.Any())
            {
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var firstCategoryId = (await dbContext.Categories.FirstOrDefaultAsync()).Id;
            var adminUser = await userManager.FindByNameAsync("admin");

            var posts = new List<Post>
            {
                new Post
                {
                    Title = "The relationship between mental illness and ageing",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },
                new Post
                {
                    Title = "An analysis regarding the possibility of applying capital punishment for sex offenders",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },
                new Post
                {
                    Title = "Is there a link between bullied teenagers and law problems?",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },
                new Post
                {
                    Title = "Insecurity of own sexuality is what triggers homophobes?",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },
                new Post
                {
                    Title = "Quitting smoking through hypnosis?",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },
                new Post
                {
                    Title = "Is morality influenced by harsh laws?",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },
                new Post
                {
                    Title = "A link between mental health and child obesity",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },
                new Post
                {
                    Title = "Are later mental health issues related to childhood trauma?",
                    Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                                Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                                Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                                Laborum",
                    UserId = adminUser.Id,
                    CategoryId = firstCategoryId,
                },

            };

            await dbContext.Posts.AddRangeAsync(posts);
        }
    }
}
