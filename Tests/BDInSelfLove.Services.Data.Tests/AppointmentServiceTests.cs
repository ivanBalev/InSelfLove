namespace BDInSelfLove.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data;
    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Data.Repositories;
    using BDInSelfLove.Services.Data.Calendar;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Appointment;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Moq;
    using Xunit;

    public class AppointmentServiceTests
    {
        [Fact]
        public async Task GetAllShouldReturnOnlySpecificAppointmentsWithGivenId()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();

            try
            {
                var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                    .UseSqlite(connection)
                    .Options;

                using (var context = new ApplicationDbContext(options))
                {
                    context.Database.EnsureCreated();
                }

                using (var context = new ApplicationDbContext(options))
                {
                    var repository = new EfDeletableEntityRepository<Appointment>(context);

                    var user1 = context.Users.Add(new ApplicationUser { Id = "1" });
                    var user2 = context.Users.Add(new ApplicationUser { Id = "2" });
                    var user3 = context.Users.Add(new ApplicationUser { Id = "3" });

                    await repository.SaveChangesAsync();

                    await repository.AddAsync(new Appointment { UserId = "1", IsApproved = true });
                    await repository.AddAsync(new Appointment { UserId = "2", IsApproved = true });
                    await repository.AddAsync(new Appointment { UserId = "3", IsApproved = true });

                    await repository.SaveChangesAsync();
                }

                using (var context = new ApplicationDbContext(options))
                {
                    var repository = new EfDeletableEntityRepository<Appointment>(context);
                    var appointmentService = new AppointmentService(repository);

                    AutoMapperConfig.RegisterMappings(typeof(AppointmentServiceModel).Assembly);

                    var appointments = await appointmentService.GetAll("1").ToListAsync();

                    Assert.Single(appointments);
                    Assert.Equal("1", appointments[0].UserId);
                }
            }
            finally
            {
                connection.Close();
            }
        }

        [Fact]
        public void GetAllShouldReturnAllAppointmentsWithoutId()
        {
            var queryableData = new List<Appointment>
            {
                new Appointment { Start = DateTime.UtcNow, Id = 1 },
                new Appointment { Start = DateTime.UtcNow, Id = 2 },
                new Appointment { Start = DateTime.UtcNow, Id = 3 },
            }
            .AsQueryable();

            var repository = new Mock<IDeletableEntityRepository<Appointment>>();
            repository.Setup(r => r.All()).Returns(queryableData);

            var appointmentService = new AppointmentService(repository.Object);

            AutoMapperConfig.RegisterMappings(typeof(AppointmentServiceModel).Assembly);

            var appointments = appointmentService.GetAll().ToList();

            Assert.IsType<AppointmentServiceModel>(appointments[0]);
            Assert.Equal(3, appointments.Count());
            Assert.Equal(1, appointments[0].Id);
            Assert.Equal(2, appointments[1].Id);
            Assert.Equal(3, appointments[2].Id);
        }
    }
}
