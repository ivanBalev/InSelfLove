﻿namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

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
        private readonly UserManager<ApplicationUser> userManager;

        public AppointmentService(
            IDeletableEntityRepository<Appointment> appointmentRepository,
            UserManager<ApplicationUser> userManager)
        {
            this.appointmentRepository = appointmentRepository;
            this.userManager = userManager;
        }

        public async Task<AppointmentServiceModel[]> GetAll(string userId)
        {
            ApplicationUser admin = (await this.userManager.GetUsersInRoleAsync(GlobalConstants.AdministratorRoleName)).FirstOrDefault();
            bool currentUserIsAdmin = admin.Id == userId;
            var query = this.appointmentRepository.All();
            if (!currentUserIsAdmin)
            {
                // Take only user's own and upcoming available appointments
                query = query.Where(a => a.UserId == userId ||
                                        (a.UserId == null && DateTime.Compare(a.UtcStart, DateTime.UtcNow) > 0));
            }
            else
            {
                // Skip past available appointment slots
                query = query.Where(a => !(a.UserId == null && DateTime.Compare(a.UtcStart, DateTime.UtcNow) <= 0));
            }

            return await query.To<AppointmentServiceModel>().ToArrayAsync();
        }

        public async Task<AppointmentServiceModel> GetById(int id)
        {
            return await this.appointmentRepository.All()
                .To<AppointmentServiceModel>()
                .SingleOrDefaultAsync(appointment => appointment.Id == id);
        }

        public async Task<int> Create(List<AppointmentServiceModel> controllerAppointments)
        {
            // Do nothing if collection is empty
            if (controllerAppointments.Count == 0)
            {
                return 0;
            }

            // Delete same day unoccupied slots
            DateTime date = controllerAppointments[0].UtcStart.Date;
            var dbAppointments = await this.appointmentRepository.All()
                .Where(a => DateTime.Compare(date, a.UtcStart.Date) == 0)
                .Where(a => a.UserId == null).ToListAsync();

            foreach (var dbAppointment in dbAppointments)
            {
                this.appointmentRepository.Delete(dbAppointment);
            }

            // Map to DB entities and add to DB
            foreach (var controllerAppointment in controllerAppointments)
            {
                var appointmentForDB = AutoMapperConfig.MapperInstance.Map<Appointment>(controllerAppointment);
                await this.appointmentRepository.AddAsync(appointmentForDB);
            }

            return await this.appointmentRepository.SaveChangesAsync();
        }

        public async Task<int> Book(AppointmentServiceModel clientAppointment)
        {
            // Check if slot is already occupied
            var sameSlotAppointmentFromDB = await this.appointmentRepository.All()
                .FirstOrDefaultAsync(a => DateTime.Compare(a.UtcStart, clientAppointment.UtcStart) == 0);
            if (sameSlotAppointmentFromDB != null)
            {
                return 0;
            }

            // Map to DB entity and add to DB
            var appointmentForDB = AutoMapperConfig.MapperInstance.Map<Appointment>(clientAppointment);
            await this.appointmentRepository.AddAsync(appointmentForDB);
            return await this.appointmentRepository.SaveChangesAsync();
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
                            a.UtcStart.Month == date.Month &&
                            a.UtcStart.Day == date.Day &&
                            a.UtcStart.Year == date.Year)
                .To<AppointmentServiceModel>();
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
