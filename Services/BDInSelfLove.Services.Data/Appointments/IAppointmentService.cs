namespace BDInSelfLove.Services.Data.Appointments
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface IAppointmentService
    {
        Task<int> Create(DateTime[] utcSlots, DateTime utcDate);

        IQueryable<Appointment> GetAll(bool userIsAdmin, string userId);

        Task<Appointment> Book(int appointmentId, string appointmentDescription, string userId);

        Task<Appointment> GetById(int id);

        Task<int> Delete(Appointment appointment);

        Task<Appointment> Approve(int id);

        Task<Appointment> Occupy(int id, string adminId);

        Task<int> Cancel(Appointment appointment);
    }
}
