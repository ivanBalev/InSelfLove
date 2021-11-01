namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using Microsoft.EntityFrameworkCore;

    public class AppointmentService : IAppointmentService
    {
        private readonly IDeletableEntityRepository<Appointment> appointmentRepository;

        public AppointmentService(IDeletableEntityRepository<Appointment> appointmentRepository)
        {
            this.appointmentRepository = appointmentRepository;
        }

        public async Task<int> Create(DateTime[] utcSlots, DateTime utcDate)
        {
            // Can't create appointment slot for past date
            if (DateTime.Compare(utcDate.Date, DateTime.UtcNow.Date) < 0)
            {
                throw new ArgumentException(nameof(utcDate));
            }

            var sameDayVacantSlots = await this.appointmentRepository.All()
                .Where(a => DateTime.Compare(utcDate.Date, a.UtcStart.Date) == 0)
                .Where(a => a.UserId == null).ToListAsync();

            // Delete current vacant slots
            foreach (var slot in sameDayVacantSlots)
            {
                this.appointmentRepository.Delete(slot);
            }

            if (utcSlots == null)
            {
                return await this.appointmentRepository.SaveChangesAsync();
            }

            // Create new vacant slots and add to DB
            foreach (var dateTime in utcSlots)
            {
                var appointmentForDB = new Appointment { UtcStart = dateTime };
                await this.appointmentRepository.AddAsync(appointmentForDB);
            }

            return await this.appointmentRepository.SaveChangesAsync();
        }

        public async Task<Appointment> Book(int appointmentId, string appointmentDescription, string userId)
        {
            if (appointmentId < 1 || string.IsNullOrEmpty(appointmentDescription) ||
                string.IsNullOrWhiteSpace(appointmentDescription) || string.IsNullOrEmpty(userId) ||
                string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException();
            }

            var dbAppointment = await this.appointmentRepository.All()
                 .SingleOrDefaultAsync(a => a.Id == appointmentId);

            if (dbAppointment == null)
            {
                throw new ArgumentException(nameof(appointmentId));
            }

            if (dbAppointment.UserId != null)
            {
                throw new UnauthorizedAccessException(nameof(dbAppointment));
            }

            // Update
            dbAppointment.Description = appointmentDescription;
            dbAppointment.UserId = userId;
            this.appointmentRepository.Update(dbAppointment);
            await this.appointmentRepository.SaveChangesAsync();

            return dbAppointment;
        }

        public IQueryable<Appointment> GetAll(string userId, bool userIsAdmin)
        {
            var query = this.appointmentRepository.All();
            if (!userIsAdmin)
            {
                // Take only user's own and upcoming vacant appointments
                query = query.Where(a => a.UserId == userId ||
                                        (a.UserId == null && DateTime.Compare(a.UtcStart, DateTime.UtcNow) > 0));
            }
            else
            {
                // Take only upcoming appointments
                query = query.Where(a => !(a.UserId == null && DateTime.Compare(a.UtcStart, DateTime.UtcNow) <= 0));
            }

            return query;
        }

        public async Task<Appointment> GetById(int id)
        {
            return await this.appointmentRepository.All()
                .SingleOrDefaultAsync(appointment => appointment.Id == id);
        }

        public async Task<int> Delete(Appointment appointment)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment));
            }

            this.appointmentRepository.Delete(appointment);
            return await this.appointmentRepository.SaveChangesAsync();
        }

        public async Task<Appointment> Approve(int appointmentId)
        {
            var appointment = await this.appointmentRepository.All().SingleOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment));
            }

            appointment.IsApproved = true;
            this.appointmentRepository.Update(appointment);
            await this.appointmentRepository.SaveChangesAsync();
            return appointment;
        }

        public async Task<int> Cancel(Appointment appointment)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment));
            }

            appointment.UserId = null;
            appointment.Description = null;
            appointment.IsApproved = false;
            this.appointmentRepository.Update(appointment);
            return await this.appointmentRepository.SaveChangesAsync();
        }
    }
}
