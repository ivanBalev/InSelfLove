namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper.QueryableExtensions;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Appointment;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;

    public class AppointmentService : IAppointmentService
    {
        private readonly IDeletableEntityRepository<Appointment> appointmentRepository;

        public AppointmentService(IDeletableEntityRepository<Appointment> appointmentRepository)
        {
            this.appointmentRepository = appointmentRepository;
        }

        public IQueryable<AppointmentServiceModel> GetAll(string userId)
        {
            var query = this.appointmentRepository.All().To<AppointmentServiceModel>();

            return userId == GlobalConstants.AdministratorRoleName ? query : query.Where(a => a.UserId == userId);
        }

        public async Task<AppointmentServiceModel> GetById(int id)
        {
            return await this.appointmentRepository.All()
                .To<AppointmentServiceModel>()
                .SingleOrDefaultAsync(appointment => appointment.Id == id);
        }

        public async Task<int> Create(AppointmentServiceModel appointmentServiceModel)
        {
            var appointment = AutoMapperConfig.MapperInstance.Map<Appointment>(appointmentServiceModel);

            await this.appointmentRepository.AddAsync(appointment);
            await this.appointmentRepository.SaveChangesAsync();

            return appointment.Id;
        }

        public async Task<int> SubmitDailyWorkingHours(ICollection<AppointmentServiceModel> unavailableTimeSlots, DateTime date, string adminId)
        {
            // Take all same day entries from db
            var dbCurrentDayAppointments = await this.appointmentRepository.All()
                .Where(a => a.Start.Day == date.Day && a.IsApproved).ToListAsync();

            var usersAppointments = new List<Appointment>();

            for (int index = 0; index < dbCurrentDayAppointments.Count; index++)
            {
                var appointment = dbCurrentDayAppointments[index];

                // Delete those that are created by admin
                if (appointment.UserId == adminId)
                {
                    this.appointmentRepository.Delete(appointment);
                    await this.appointmentRepository.SaveChangesAsync();
                    continue;
                }

                // Store all users' appointments
                usersAppointments.Add(appointment);
            }

            await this.appointmentRepository.SaveChangesAsync();

            // Add new ones only for the timeslots that don't overlap with existing user appointments
            foreach (var unavailableTimeSlot in unavailableTimeSlots)
            {
                if (!usersAppointments.Any(a => a.Start.Hour == unavailableTimeSlot.Start.Hour))
                {
                    var slotForDb = AutoMapperConfig.MapperInstance.Map<Appointment>(unavailableTimeSlot);

                    // TODO: Fix this please
                    slotForDb.IsApproved = true;
                    await this.appointmentRepository.AddAsync(slotForDb);
                }
            }

            var result = await this.appointmentRepository.SaveChangesAsync();

            return result;
        }

        public async Task<AppointmentServiceModel> Delete(int appointmentId)
        {
            var dbAppointment = await this.appointmentRepository.All().FirstOrDefaultAsync(a => a.Id == appointmentId);

            if (dbAppointment == null)
            {
                throw new ArgumentNullException(nameof(dbAppointment));
            }

            this.appointmentRepository.Delete(dbAppointment);
            int result = await this.appointmentRepository.SaveChangesAsync();

            return AutoMapperConfig.MapperInstance.Map<AppointmentServiceModel>(dbAppointment);
        }

        public IQueryable<AppointmentServiceModel> GetAllByDate(DateTime date)
        {
            return this.appointmentRepository.All()
                .Where(a => a.IsApproved &&
                            a.Start.Month == date.Month &&
                            a.Start.Day == date.Day &&
                            a.Start.Year == date.Year)
                .To<AppointmentServiceModel>();
        }

        public IQueryable<AppointmentServiceModel> GetAllForDaysAhead(int daysAhead, string userUserName)
        {
            var query = this.appointmentRepository.All();

            var today = DateTime.Today;

            query = query.Where(a => (a.Start.Date >= today.Date && a.Start.Date <= today.AddDays(daysAhead).Date) || a.User.UserName == userUserName);

            return query.To<AppointmentServiceModel>();
        }

        public async Task<int> Approve(int appointmentId)
        {
            var appointment = await this.appointmentRepository.All().SingleOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment));
            }

            appointment.IsApproved = true;
            this.appointmentRepository.Update(appointment);
            return await this.appointmentRepository.SaveChangesAsync();
        }
    }
}
