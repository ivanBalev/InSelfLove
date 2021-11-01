namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface IAppointmentService
    {
        Task<int> Create(DateTime[] utcSlots, DateTime utcDate);

        IQueryable<Appointment> GetAll(string userId, bool userIsAdmin);

        Task<Appointment> Book(int appointmentId, string appointmentDescription, string userId);

        Task<Appointment> GetById(int id);

        Task<int> Delete(Appointment appointment);

        Task<Appointment> Approve(int id);

        Task<int> Cancel(Appointment appointment);
    }
}
