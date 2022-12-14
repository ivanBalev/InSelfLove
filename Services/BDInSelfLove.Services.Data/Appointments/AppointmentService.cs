namespace BDInSelfLove.Services.Data.Appointments
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
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

            var sameDaySlos = await this.appointmentRepository.All()
                .Where(a => DateTime.Compare(utcDate.Date, a.UtcStart.Date) == 0).ToListAsync();

            var sameDayVacantSlots = sameDaySlos
                .Where(a => a.UserId == null).ToList();

            // Delete current vacant slots
            foreach (var slot in sameDayVacantSlots)
            {
                this.appointmentRepository.Delete(slot);
            }

            if (utcSlots == null)
            {
                return await this.appointmentRepository.SaveChangesAsync();
            }

            // Create new vacant slots and add to db
            foreach (var dateTime in utcSlots)
            {
                var appointmentForDB = new Appointment { UtcStart = dateTime };
                await this.appointmentRepository.AddAsync(appointmentForDB);
            }

            return await this.appointmentRepository.SaveChangesAsync();
        }

        public async Task<Appointment> Book(int appointmentId, string appointmentDescription, bool isOnSite, string userId)
        {
            if (appointmentId < 1 || string.IsNullOrEmpty(userId) ||
                string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException();
            }

            appointmentDescription = Regex.Replace(appointmentDescription, "[*'\",_&#^@;]", string.Empty);

            var dbAppointment = await this.appointmentRepository.All()
                 .SingleOrDefaultAsync(a => a.Id == appointmentId);

            var userAppointmentsForDay = await this.appointmentRepository.All()
                                           .Where(x => x.UserId == userId && DateTime.Compare(dbAppointment.UtcStart.Date, x.UtcStart.Date) == 0).ToListAsync();

            if (userAppointmentsForDay.Count > 0)
            {
                throw new ArgumentException(nameof(dbAppointment.UtcStart));
            }

            if (dbAppointment == null)
            {
                throw new ArgumentException(nameof(appointmentId));
            }

            if (dbAppointment.UserId != null || (dbAppointment.CanBeOnSite == false && isOnSite == true))
            {
                throw new UnauthorizedAccessException(nameof(dbAppointment));
            }

            // Update
            dbAppointment.Description = appointmentDescription;
            dbAppointment.UserId = userId;
            dbAppointment.IsOnSite = isOnSite;
            this.appointmentRepository.Update(dbAppointment);
            await this.appointmentRepository.SaveChangesAsync();

            return dbAppointment;
        }

        public IQueryable<Appointment> GetAll(bool userIsAdmin, string userId = "-1")
        {
            return this.appointmentRepository.All();
        }

        public async Task<Appointment> GetById(int id)
        {
            return await this.appointmentRepository.All()
                .Include(x => x.User)
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
            var appointment = await this.appointmentRepository.All()
                .Include(a => a.User).SingleOrDefaultAsync(a => a.Id == appointmentId);

            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment));
            }

            appointment.IsApproved = true;
            this.appointmentRepository.Update(appointment);
            await this.appointmentRepository.SaveChangesAsync();
            return appointment;
        }

        public async Task<Appointment> SetOnSite(int apptId, bool canBeOnSite)
        {
            var appt = await this.appointmentRepository.All().FirstOrDefaultAsync(x => x.Id == apptId);

            if (appt == null)
            {
                throw new ArgumentNullException(nameof(appt));
            }

            appt.CanBeOnSite = canBeOnSite;

            this.appointmentRepository.Update(appt);
            await this.appointmentRepository.SaveChangesAsync();

            return appt;
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

        public async Task<Appointment> Occupy(int id, string adminId)
        {
            var appointment = await this.appointmentRepository.All().SingleOrDefaultAsync(a => a.Id == id);

            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment));
            }

            appointment.UserId = adminId;
            appointment.IsApproved = true;
            this.appointmentRepository.Update(appointment);
            await this.appointmentRepository.SaveChangesAsync();
            return appointment;
        }
    }
}
