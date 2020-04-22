using BDInSelfLove.Data;
using BDInSelfLove.Data.Models;
using BDInSelfLove.Data.Repositories;
using BDInSelfLove.Services.Data.Calendar;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Appointment;
using BDInSelfLove.Web.ViewModels.Appointment;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BDInSelfLove.Services.Data.Tests
{
    public class AppointmentServiceTests
    {
        [Fact]
        public async Task GetAllShouldReturnAllAppointmentsWithNoUserId()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var repository = new EfDeletableEntityRepository<Appointment>(new ApplicationDbContext(options.Options));

            await repository.AddAsync(new Appointment { Start = DateTime.UtcNow });
            await repository.AddAsync(new Appointment { Start = DateTime.UtcNow });
            await repository.AddAsync(new Appointment { Start = DateTime.UtcNow });

            await repository.SaveChangesAsync();

            var appointmentService = new AppointmentService(repository);

            AutoMapperConfig.RegisterMappings(typeof(AppointmentServiceModel).Assembly);

            var appointmentsCount = appointmentService.GetAll().Count();

            Assert.Equal(3, appointmentsCount);
        }

        [Fact]
        public async Task GetAllShouldReturnOnlySpecificAppointments()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var repository = new EfDeletableEntityRepository<Appointment>(new ApplicationDbContext(options.Options));

            await repository.AddAsync(new Appointment {UserId = "1", IsApproved = true });
            await repository.AddAsync(new Appointment {UserId = "2", IsApproved = true });
            await repository.AddAsync(new Appointment {UserId = "3", IsApproved = true });

            await repository.SaveChangesAsync();

            var appointmentService = new AppointmentService(repository);

            AutoMapperConfig.RegisterMappings(typeof(AppointmentServiceModel).Assembly);

            var appointmentsCount = await appointmentService.GetAll().FirstOrDefaultAsync();

            Assert.Equal(appointmentsCount.UserId, "1");
        }

    }
}
