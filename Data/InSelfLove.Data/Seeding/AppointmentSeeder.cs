namespace InSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data;
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Helpers;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class AppointmentSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            // Prevent seeding in prod where we may have no available but still, existing appts
            // we don't want dummy appts in prod
            if (dbContext.Appointments.All(x => x.IsDeleted && !x.IsDeleted))
            {
                return;
            }

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var user = dbContext.Users.FirstOrDefault(u => u.UserName.ToLower()
            .Equals(configuration[$"{AppConstants.UserRoleName}:UserName"].ToLower()));
            var admin = dbContext.Users.FirstOrDefault(u => u.UserName.ToLower()
            .Equals(configuration[$"{AppConstants.AdministratorRoleName}:UserName"].ToLower()));

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    UtcStart = DateTime.UtcNow.AddDays(1),
                },
                new Appointment
                {
                    UtcStart = DateTime.UtcNow.AddDays(1).AddHours(1),
                    User = user,
                    IsApproved = true,
                },
                new Appointment
                {
                    UtcStart = DateTime.UtcNow.AddDays(1).AddHours(2),
                    User = user,
                    IsApproved = false,
                },
                new Appointment
                {
                    UtcStart = DateTime.UtcNow.AddDays(1).AddHours(3),
                    User = admin,
                    IsApproved = true,
                },
            };

            await dbContext.Appointments.AddRangeAsync(appointments);
        }
    }
}