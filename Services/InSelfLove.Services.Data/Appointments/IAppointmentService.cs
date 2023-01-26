namespace InSelfLove.Services.Data.Appointments
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;

    public interface IAppointmentService
    {
        Task<int> Create(DateTime[] timeSlots, DateTime utcDate, string adminTimezone);

        Task<IEnumerable<Appointment>> GetAll(string userId, string adminId, string userTimezone);

        Task<Appointment> Book(int appointmentId, string appointmentDescription, bool isOnSite, string userId);

        Task<Appointment> GetById(int id);

        Task<int> Delete(Appointment appointment);

        Task<Appointment> Approve(int id);

        Task Occupy(int id, string adminId);

        Task<Appointment> SetOnSite(int apptId, bool canBeOnSite);

        Task<int> Cancel(Appointment appointment, string userId, string adminId);
    }
}
