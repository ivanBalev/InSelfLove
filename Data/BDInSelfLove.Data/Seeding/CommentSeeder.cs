namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Internal;
    using Microsoft.Extensions.DependencyInjection;

    public class CommentSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Comments.Any())
            {
                return;
            }

            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var firstPostId = (await dbContext.Posts.FirstOrDefaultAsync()).Id;
            var adminUser = await userManager.FindByNameAsync("admin");

            var comments = new List<Comment>
            {
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,

                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,

                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                },
            };

            dbContext.Comments.AddRange(comments);
            await dbContext.SaveChangesAsync();
            var firstCommentId = (await dbContext.Comments.FirstOrDefaultAsync()).Id;

            var subComments = new List<Comment>
            {
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                   ParentCommentId = firstCommentId,
                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                   ParentCommentId = firstCommentId,
                },
                new Comment
                {
                   Content = @"Lorem ipsum dolor sit amet, consectetur adipisicing elit. Repudiandae dicta accusantium explicabo nihil nemo veritatis eius? Et incidunt nam voluptatum, aspernatur tempora doloribus perspiciatis repellat. Laboriosam exercitationem dolorem numquam veniam.
                               Ab alias odio ea voluptatibus error. Eum nisi, iure quaerat soluta dolor, inventore ab esse quo atque, magnam aperiam. Sint eum possimus modi nihil repudiandae consectetur dolorum quibusdam ipsa placeat.
                               Ipsa natus praesentium nostrum tempore eligendi ut nihil cumque, molestiae facilis odio magnam modi dicta delectus minima soluta dolore recusandae expedita error quo iste repellendus velit! Adipisci dolorem ducimus delectus.
                               Laborum",
                   UserId = adminUser.Id,
                   ParentPostId = firstPostId,
                   ParentCommentId = firstCommentId,
                },
            };

            await dbContext.Comments.AddRangeAsync(subComments);
        }
    }
}
