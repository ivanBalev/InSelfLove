namespace InSelfLove.Data.Seeding
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using Microsoft.EntityFrameworkCore;

    internal class AppointmentSeeder : ISeeder
    {
        public async Task SeedAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            if (dbContext.Appointments.Count() > 0)
            {
                return;
            }

            var comment = new Appointment
            {
                Id = 1,
                CreatedOn = DateTime.UtcNow.Date.AddDays(1).AddHours(20),
            };

            await dbContext.Appointments.AddAsync(comment);
            await dbContext.SaveChangesAsync();
        }
    }
}
