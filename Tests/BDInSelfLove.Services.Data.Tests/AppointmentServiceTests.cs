namespace BDInSelfLove.Services.Data.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Data.Repositories;
    using BDInSelfLove.Services.Data.Calendar;
    using Microsoft.EntityFrameworkCore;
    using Xunit;

    public class AppointmentServiceTests
    {
        private const string CurrentUserId = "1";
        private const string OtherUserId = "2";

        private EfDeletableEntityRepository<Appointment> appointmentRepository;
        private AppointmentService appointmentService;

        public AppointmentServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                           .UseInMemoryDatabase(Guid.NewGuid().ToString());
            var dbContext = new ApplicationDbContext(options.Options);
            var appointmentRepository = new EfDeletableEntityRepository<Appointment>(dbContext);
            var appointmentService = new AppointmentService(appointmentRepository);
            this.appointmentRepository = appointmentRepository;
            this.appointmentService = appointmentService;
        }

        // Create
        [Fact]
        public async Task CreateWorksCorrectlyWithValidData()
        {
            var date = DateTime.UtcNow.Date.AddDays(1);
            var slots = new DateTime[]
            {
                date.AddHours(1),
            };

            var result = await this.appointmentService.Create(slots, date);
            var allAppointments = await this.appointmentRepository.All().ToListAsync();

            Assert.True(result > 0);
            Assert.True(allAppointments.Count == 1);
        }

        [Fact]
        public async Task CreateThrowsArgumentExceptionWhenGivenPastDate()
        {
            var date = DateTime.UtcNow.Date.AddDays(-1);
            await Assert.ThrowsAsync<ArgumentException>(() => this.appointmentService.Create(null, date));
        }

        [Fact]
        public async Task CreateWorksCorrectlyWithExistingAppointmentSlotsForSameDay()
        {
            var oldSameDayAppointmentSlots = await this.SeedData();

            var date = oldSameDayAppointmentSlots[0].Date;
            var slots = new DateTime[]
            {
                date.AddHours(12),
            };

            var result = await this.appointmentService.Create(slots, date);
            var dbAppointments = await this.appointmentRepository.All().ToListAsync();

            Assert.True(result > 0);
            Assert.Single(dbAppointments);
            Assert.True(DateTime.Compare(slots[0], dbAppointments[0].UtcStart) == 0);
        }

        [Fact]
        public async Task CreateRemovesPreviousSameDaySlotsWithInputOfNullSlotsCollection()
        {
            await this.SeedData();
            var date = DateTime.UtcNow.Date.AddDays(1);
            Assert.True(await this.appointmentService.Create(null, date) > 0);
        }

        // Book
        [Fact]
        public async Task BookWorksCorrectlyWithValidData()
        {
            await this.ClearAppointments();
            await this.SeedData();

            var appointment = await this.appointmentService.Book(1, "test", "testUser");

            Assert.Equal(1, appointment.Id);
            Assert.Equal("test", appointment.Description);
            Assert.Equal("testUser", appointment.UserId);
        }

        [Theory]
        [InlineData(0, "test", "test")]
        [InlineData(1, "", "test")]
        [InlineData(1, "test", "")]
        [InlineData(1, null, "test")]
        [InlineData(1, "test", null)]
        [InlineData(1, "   ", "test")]
        [InlineData(1, "test", "   ")]
        public async Task BookThrowsArgumentExceptionsWithInvalidInput(int appointmentId, string description, string userId)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
            this.appointmentService.Book(appointmentId, description, userId));
        }

        [Fact]
        public async Task BookThrowsUnauthorizedAccessExceptionCorrectly()
        {
            await this.ClearAppointments();
            await this.SeedData();

            var appointment = await this.appointmentService.Book(1, "test", "testUser");

            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            this.appointmentService.Book(1, "test", "userTest"));
        }

        // GetAll
        [Fact]
        public async Task GetAllReturnsOnlyFutureAvailableAndPastBookedByCurrentUser()
        {
            await this.ClearAppointments();
            var seededAppointments = await this.CreateDummyAppointments();

            var dbAppointments = await this.appointmentService.GetAll(CurrentUserId, false).ToListAsync();

            var expectedAppointments = seededAppointments.Where(a => a.UserId == CurrentUserId ||
                                        (a.UserId == null && DateTime.Compare(a.UtcStart, DateTime.UtcNow) > 0))
                                        .ToList();

            foreach (var expectedAppointment in expectedAppointments)
            {
                Assert.Contains(dbAppointments, a => a.Description.Equals(expectedAppointment.Description));
            }

            Assert.Equal(dbAppointments.Count, expectedAppointments.Count);
        }

        [Fact]
        public async Task GetAllReturnsOnlyFutureAvailableAndBookedForAdmin()
        {
            await this.ClearAppointments();

            var seededAppointments = await this.CreateDummyAppointments();

            var dbAppointments = await this.appointmentService.GetAll(CurrentUserId, true).ToListAsync();

            var expectedAppointments = seededAppointments.Where(a => !(a.UserId == null &&
                                        DateTime.Compare(a.UtcStart, DateTime.UtcNow) <= 0))
                                        .ToList();

            foreach (var expectedAppointment in expectedAppointments)
            {
                Assert.Contains(dbAppointments, a => a.Description.Equals(expectedAppointment.Description));
            }

            Assert.Equal(dbAppointments.Count, expectedAppointments.Count);
        }

        // GetById
        [Fact]
        public async Task GetByIdWorksCorrectly()
        {
            await this.ClearAppointments();
            var dummyAppointments = await this.CreateDummyAppointments();

            var firstDbAppointment = await this.appointmentService.GetById(1);

            Assert.True(firstDbAppointment.Description.Equals(dummyAppointments[0].Description));
        }

        [Fact]
        public async Task GetByIdReturnsNullWithNonExistentId()
        {
            await this.ClearAppointments();

            var appointment = await this.appointmentService.GetById(1);

            Assert.Null(appointment);
        }

        // Delete
        [Fact]
        public async Task DeleteTorwsArgumentNullExceptionWithNullInput()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.appointmentService.Delete(null));
        }

        [Fact]
        public async Task DeleteWorksCorrectlyWithValidData()
        {
            await this.ClearAppointments();
            var seededData = await this.SeedData();

            var firstAppointment = await this.appointmentService.GetById(1);
            var result = await this.appointmentService.Delete(firstAppointment);

            var allDbAppointments = await this.appointmentRepository.All().ToListAsync();

            Assert.True(result > 0);
            Assert.True(allDbAppointments.Count == seededData.Length - 1);
            Assert.DoesNotContain(allDbAppointments, a => a.Id == firstAppointment.Id);
        }

        // Appprove
        [Fact]
        public async Task ApproveThrowsArgumentNullExceptionWithNonExistentId()
        {
            await this.ClearAppointments();

            await Assert.ThrowsAsync<ArgumentNullException>(() => this.appointmentService.Approve(1));
        }

        [Fact]
        public async Task ApproveWorksCorrectlyWithValidData()
        {
            await this.ClearAppointments();
            await this.SeedData();

            var appointment = await this.appointmentService.Approve(1);
            Assert.True(appointment.IsApproved);
        }

        // Cancel
        [Fact]
        public async Task CancelThrowsArgumentNullExceptionWithInvalidData()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => this.appointmentService.Cancel(null));
        }

        [Fact]
        public async Task CancelWorksCorrectlyWithValidData()
        {
            await this.ClearAppointments();
            await this.SeedData();

            var appointment = await this.appointmentService.GetById(1);
            var result = await this.appointmentService.Cancel(appointment);

            Assert.True(result > 0);
        }

        private async Task ClearAppointments()
        {
            var allEntries = await this.appointmentRepository.All().ToListAsync();

            foreach (var appointment in allEntries)
            {
                this.appointmentRepository.HardDelete(appointment);
            }

            await this.appointmentRepository.SaveChangesAsync();
        }

        private async Task<DateTime[]> SeedData()
        {
            var appointmentsDate = DateTime.UtcNow.Date.AddDays(1);

            DateTime[] appointmentSlots = new DateTime[]
            {
                DateTime.UtcNow.Date.AddDays(1).AddHours(1),
                DateTime.UtcNow.Date.AddDays(1).AddHours(2),
                DateTime.UtcNow.Date.AddDays(1).AddHours(3),
                DateTime.UtcNow.Date.AddDays(1).AddHours(4),
                DateTime.UtcNow.Date.AddDays(1).AddHours(5),
            };

            await this.appointmentService.Create(appointmentSlots, appointmentsDate);

            return appointmentSlots;
        }

        private async Task<List<Appointment>> CreateDummyAppointments()
        {
            var appointments = new List<Appointment>
            {
                new Appointment
                {
                    Description = "pastAvailable",
                    UtcStart = DateTime.UtcNow.Date.AddDays(-10),
                },
                new Appointment
                {
                    UserId = CurrentUserId,
                    Description = "pastBookedByCurrentUser",
                    UtcStart = DateTime.UtcNow.Date.AddDays(-10),
                },
                new Appointment
                {
                    UserId = CurrentUserId,
                    Description = "pastBookedByOtherUser",
                    UtcStart = DateTime.UtcNow.Date.AddDays(-10),
                },
                new Appointment
                {
                    Description = "futureAvailable",
                    UtcStart = DateTime.UtcNow.Date.AddDays(10),
                },
                new Appointment
                {
                    UserId = CurrentUserId,
                    Description = "futureBookedByCurrentUser",
                    UtcStart = DateTime.UtcNow.Date.AddDays(10),
                },
                new Appointment
                {
                    UserId = CurrentUserId,
                    Description = "futureBookedByOtherUser",
                    UtcStart = DateTime.UtcNow.Date.AddDays(10),
                },
            };

            foreach (var appointment in appointments)
            {
                await this.appointmentRepository.AddAsync(appointment);
            }

            await this.appointmentRepository.SaveChangesAsync();

            return appointments;
        }
    }
}
