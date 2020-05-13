namespace BDInSelfLove.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Data.Common;
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
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Moq;
    using Xunit;

    public class AppointmentServiceTests : IDisposable
    {
        public AppointmentServiceTests()
        {
            MapperInitializer.InitializeMapper();
        }

        private DbConnection Connection { get; set; }

        private DbContextOptions<ApplicationDbContext> ContextOptions { get; set; }

        public void Dispose() => this.Connection?.Dispose();

        [Fact]
        public async Task GetAllShouldReturnAllAppointmentsWithoutParameters()
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            var appointmentsFromDb = await appointmentService.GetAll().ToListAsync();

            var generatedAppointments = this.GetTestAppointments();

            Assert.Equal(generatedAppointments.Length, appointmentsFromDb.Count);

            foreach (var appointment in appointmentsFromDb)
            {
                Assert.Contains(generatedAppointments, a => a.Description == appointment.Description);
            }
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("-1")]
        [InlineData("asd")]
        public async Task GetAllShouldReturnOnlyCurrentUserAppointmentsWithParameters(string userId)
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            var appointmentsFromDb = await appointmentService.GetAll(userId).ToListAsync();

            var generatedAppointments = this.GetTestAppointments().Where(a => a.UserId == userId).ToList();

            Assert.Equal(generatedAppointments.Count, appointmentsFromDb.Count);

            foreach (var appointment in appointmentsFromDb)
            {
                Assert.Contains(generatedAppointments, a => a.Description == appointment.Description);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task GetByIdShouldReturnCorrectAppointment(int appointmentId)
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            var appointment = await appointmentService.GetById(appointmentId);

            Assert.Equal(appointment.Id, appointmentId);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(5)]
        public async Task GetByIdShouldReturnNullWithNonExistentId(int appointmentId)
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            var appointment = await appointmentService.GetById(appointmentId);

            Assert.Null(appointment);
        }

        [Fact]
        public async Task CreateShouldWorkCorrectly()
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            var appointmentId = await appointmentService.Create(new AppointmentServiceModel());

            Assert.True(appointmentId == (this.GetTestAppointments().Length + 1));
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task DeleteShouldWorkCorrectly(int appointmentId)
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            var deleteResult = await appointmentService.Delete(appointmentId);

            Assert.True(deleteResult);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(-2)]
        [InlineData(0)]
        public async Task DeleteShouldThrowExceptionsCorrectly(int appointmentId)
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            await Assert.ThrowsAsync<ArgumentNullException>(() => appointmentService.Delete(appointmentId));
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(-1)]
        [InlineData(0)]
        public async Task GetAllByDateShouldWorkCorrectly(int daysToRemove)
        {
            await this.SetupSqlite();
            using var context = new ApplicationDbContext(this.ContextOptions);

            var repository = new EfDeletableEntityRepository<Appointment>(context);
            var appointmentService = new AppointmentService(repository);

            var date = DateTime.UtcNow.AddDays(daysToRemove * -1);

            var appointmentsFromDb = await appointmentService.GetAllByDate(date).ToListAsync();

            var generatedAppointments = this.GetTestAppointments()
                .Where(a => a.Start.Month == date.Month &&
                            a.Start.Day == date.Day &&
                            a.Start.Year == date.Year)
                .ToList();

            Assert.Equal(appointmentsFromDb.Count, generatedAppointments.Count);

            foreach (var appointment in appointmentsFromDb)
            {
                Assert.Contains(generatedAppointments, a => a.Description == appointment.Description);
            }
        }

        private static DbConnection CreateInMemoryDatabase()
        {
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            return connection;
        }

        private async Task Seed()
        {
            using var context = new ApplicationDbContext(this.ContextOptions);
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            await context.Users.AddRangeAsync(this.GetTestUsers());
            await context.SaveChangesAsync();

            await context.Appointments.AddRangeAsync(this.GetTestAppointments());
            await context.SaveChangesAsync();
        }

        private Appointment[] GetTestAppointments()
        {
            return new Appointment[]
                {
                    new Appointment
                    {
                        Description = "Test1",
                        Start = DateTime.UtcNow.AddDays(-1),
                        UserId = "1",
                        IsApproved = true,
                        Id = 1,
                    },
                    new Appointment
                    {
                        Description = "Test2",
                        Start = DateTime.UtcNow.AddDays(-2),
                        UserId = "2",
                        IsApproved = true,
                        Id = 2,
                    },
                    new Appointment
                    {
                        Description = "Test3",
                        Start = DateTime.UtcNow.AddDays(-3),
                        UserId = "3",
                        IsApproved = true,
                        Id = 3,
                    },
                };
        }

        private ApplicationUser[] GetTestUsers()
        {
            return new ApplicationUser[]
            {
                new ApplicationUser { Id = "1" },
                new ApplicationUser { Id = "2" },
                new ApplicationUser { Id = "3" },
            };
        }

        private async Task SetupSqlite()
        {
            this.ContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
              .UseSqlite(CreateInMemoryDatabase())
              .Options;

            this.Connection = RelationalOptionsExtension.Extract(this.ContextOptions).Connection;
            await this.Seed();
        }
    }
}
