namespace BDInSelfLove.Services.Data.Appointments
{
    using System;
    using System.Collections.Generic;
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

        public async Task<IEnumerable<Appointment>> GetAll(string userId, string adminId)
        {
            var userIsAdmin = userId == adminId;
            var dbQuery = this.appointmentRepository.All();

            if (!userIsAdmin)
            {
                // Include only upcoming available and user's own apptmnts
                dbQuery = dbQuery.Where(x => DateTime.Compare(x.UtcStart, DateTime.UtcNow) > 0 || (x.UserId != null && x.UserId == userId));
            }
            else
            {
                // Include only upcoming available and all occupied apptmnts
                dbQuery = dbQuery.Where(x => DateTime.Compare(x.UtcStart, DateTime.UtcNow) > 0 || x.UserId != null);
            }

            var appointments = await dbQuery.Include(x => x.User).ToArrayAsync();

            // An appointment available to one user is not necessarily available to another
            // so we need to dynamically set availability instead of storing it in db
            this.SetAvailability(appointments, userIsAdmin, userId);
            return appointments;
        }

        public async Task<Appointment> GetById(int id)
        {
            return await this.appointmentRepository.All()
                .Include(x => x.User)
                .SingleOrDefaultAsync(appointment => appointment.Id == id);
        }

        public async Task<int> Create(DateTime[] utcSlots, DateTime utcDate)
        {
            // We need both the individual slots and the date in case
            // an empty slots list has been sent by the admin which
            // means they'd like to cancel all vacant slots for the day

            // Can't create appointment slot for past date
            if (DateTime.Compare(utcDate.Date, DateTime.UtcNow.Date) < 0)
            {
                throw new ArgumentException(nameof(utcDate));
            }

            var sameDayVacantSlots = await this.appointmentRepository.All()
                .Where(a => DateTime.Compare(utcDate.Date, a.UtcStart.Date) == 0 &&
                            a.UserId == null).ToListAsync();

            // Delete all same day vacant slots
            foreach (var slot in sameDayVacantSlots)
            {
                this.appointmentRepository.Delete(slot);
            }

            // Create new vacant slots and add to db
            foreach (var dateTime in utcSlots)
            {
                var appointmentForDB = new Appointment { UtcStart = dateTime };
                await this.appointmentRepository.AddAsync(appointmentForDB);
            }

            return await this.appointmentRepository.SaveChangesAsync();
        }

        public async Task<int> Cancel(Appointment appointment, string userId, string adminId)
        {
            if (appointment == null)
            {
                throw new ArgumentNullException(nameof(appointment));
            }

            var userIsAdmin = userId == adminId;

            // Allow only admin to cancel others' appointments
            if (appointment.UserId != userId && !userIsAdmin)
            {
                throw new ArgumentException(nameof(userIsAdmin));
            }

            // Delete slot if admin cancels unoccupied appointment
            if (appointment.UserId == null && userIsAdmin)
            {
                return await this.Delete(appointment);
            }

            appointment.UserId = null;
            appointment.Description = null;
            appointment.IsApproved = false;
            this.appointmentRepository.Update(appointment);
            return await this.appointmentRepository.SaveChangesAsync();
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

        public async Task<Appointment> Book(int appointmentId, string appointmentDescription, bool isOnSite, string userId)
        {
            // Sanitize description  // Description is null if this is not user's first appointment
            appointmentDescription = Regex.Replace(appointmentDescription ?? string.Empty, "[*'\",_&#^@;]", string.Empty);

            // Get appointment from db
            var dbAppointment = await this.appointmentRepository.All()
                 .SingleOrDefaultAsync(a => a.Id == appointmentId);

            // Get all user's appointments for the day
            var userAppointmentsForDay = await this.appointmentRepository.All()
                                               .Where(x => x.UserId == userId && DateTime.Compare(
                                               dbAppointment.UtcStart.Date, x.UtcStart.Date) == 0).ToListAsync();

            // Only 1 appointment per day allowed
            if (userAppointmentsForDay.Count > 0)
            {
                throw new ArgumentException(nameof(dbAppointment.UtcStart));
            }

            // Appointment has to exist
            if (dbAppointment == null)
            {
                throw new ArgumentException(nameof(appointmentId));
            }

            // Cannot book an already occupied appointment or choose it to be on site when admin hasn't allowed it
            if (dbAppointment.UserId != null || (dbAppointment.CanBeOnSite == false && isOnSite == true))
            {
                throw new UnauthorizedAccessException(nameof(dbAppointment));
            }

            // Update appointment in db
            dbAppointment.Description = appointmentDescription;
            dbAppointment.UserId = userId;
            dbAppointment.IsOnSite = isOnSite;
            this.appointmentRepository.Update(dbAppointment);
            await this.appointmentRepository.SaveChangesAsync();

            return dbAppointment;
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

        public async Task Occupy(int id, string adminId)
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
        }

        private void SetAvailability(Appointment[] appointments, bool userIsAdmin, string userId)
        {
            if (!userIsAdmin)
            {
                // Each regular user gets only 1 appointment per day
                // Get appointments grouped by day
                var grouped = appointments.GroupBy(x => x.UtcStart.Date).ToList();

                foreach (var group in grouped)
                {
                    var occupiedByUser = group.FirstOrDefault(x => userId != null && x.UserId == userId);

                    // if user already has an appointment booked for the day
                    // && group has more than 1 slot
                    if (occupiedByUser != null && group.Count() > 1)
                    {
                        // Mark the rest for the day as unavailable
                        foreach (var item in group.Where(x => x.Id != occupiedByUser.Id))
                        {
                            item.IsUnavailable = true;
                        }
                    }
                }
            }

            foreach (var appt in appointments)
            {
                // If user is admin or appt is user's own
                if (userIsAdmin || (userId != null && userId == appt.UserId))
                {
                    // return full data
                    continue;
                }

                // Appointment is another user's
                if (appt.UserId != null)
                {
                    // Mark appt as unavailable
                    appt.IsUnavailable = true;

                    // Clear other user's data
                    appt.User = null;
                    appt.UserId = null;
                }
            }
        }
    }
}
