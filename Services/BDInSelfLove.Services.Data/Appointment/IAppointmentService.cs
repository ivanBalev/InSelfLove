namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Models.Appointment;

    public interface IAppointmentService
    {
        IQueryable<Appointment> GetAll(string userId, bool userIsAdmin);

        Task<Appointment> Book(int appointmentId, string appointmentDescription, string userId);

        Task<Appointment> GetById(int id);

        Task<int> Create(DateTime[] appointmentSlots, DateTime appointmentsDate);

        Task<int> Delete(Appointment appointment);

        Task<Appointment> Approve(int appointmentId);

        Task<int> Cancel(Appointment appointment);
    }
}
