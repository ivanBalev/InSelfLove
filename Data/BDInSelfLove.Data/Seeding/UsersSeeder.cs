namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Helpers;
    using BDInSelfLove.Data.Models;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class UsersSeeder : ISeeder
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
                UserName = configuration[$"{AppConstants.AdministratorRoleName}:UserName"],
                Email = configuration[$"{AppConstants.AdministratorRoleName}:Email"],
                WindowsTimezoneId = "FLE Standard Time",
            };

            var guest = new ApplicationUser
            {
                UserName = configuration[$"{AppConstants.UserRoleName}:UserName"],
                Email = configuration[$"{AppConstants.UserRoleName}:Email"],
                WindowsTimezoneId = "FLE Standard Time",
            };

            var adminResult = await userManager.CreateAsync(
                admin, configuration[$"{AppConstants.AdministratorRoleName}:Password"]);
            var userResult = await userManager.CreateAsync(
                guest, configuration[$"{AppConstants.UserRoleName}:Password"]);

            if (adminResult.Succeeded && userResult.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, AppConstants.AdministratorRoleName);
                await userManager.AddToRoleAsync(guest, AppConstants.UserRoleName);
            }
        }
    }
}
