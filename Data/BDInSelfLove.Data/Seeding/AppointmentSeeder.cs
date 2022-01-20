namespace BDInSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    internal class AppointmentSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Appointments.Count() > 0)
            {
                return;
            }

            var configuration = serviceProvider.GetRequiredService<IConfiguration>();


            var user = dbContext.Users.FirstOrDefault(u => u.UserName.ToLower()
            .Equals(configuration[$"{GlobalValues.UserRoleName}:UserName"].ToLower()));

            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(1),
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(1).AddHours(1),
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(1).AddHours(2),
                    User = user,
                    IsApproved = true,
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(1).AddHours(3),
                    User = user,
                    IsApproved = false,
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(1).AddHours(4),
                    User = user,
                    IsApproved = false,
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(2),
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(2).AddHours(1),
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(3),
                },
                new Appointment
                {
                    UtcStart = GlobalValues.WorkDayStartUTC.AddDays(3).AddHours(1),
                },
            };

            await dbContext.Appointments.AddRangeAsync(appointments);
        }
    }
}
