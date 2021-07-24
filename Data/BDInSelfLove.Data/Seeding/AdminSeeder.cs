namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class AdminSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            if (userManager.Users.Any())
            {
                return;
            }

            var admin = new ApplicationUser
            {
                UserName = configuration["Admin:UserName"],
                Email = configuration["Admin:Email"],
            };

            var adminPassword = configuration["Admin:Password"];

            var result = await userManager.CreateAsync(admin, adminPassword);

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, GlobalValues.AdministratorRoleName);
            }
        }
    }
}
