namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Appointment;

    public interface IAppointmentService
    {
        IQueryable<AppointmentServiceModel> GetAll(string userId = null);

        Task<AppointmentServiceModel> GetById(int id);

        Task<int> Create(AppointmentServiceModel appointmentServiceModel);

        Task<AppointmentServiceModel> Delete(int appointmentId);

        IQueryable<AppointmentServiceModel> GetAllByDate(DateTime date);

        Task<int> SubmitDailyWorkingHours(ICollection<AppointmentServiceModel> availableTimeSlots, DateTime date, string adminId);

        IQueryable<AppointmentServiceModel> GetAllForDaysAhead(int daysAhead, string userUserName);

        Task<int> Approve(int appointmentId);
    }
}
