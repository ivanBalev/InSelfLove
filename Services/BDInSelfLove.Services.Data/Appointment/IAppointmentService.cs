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
        Task<AppointmentServiceModel[]> GetAll(string userId);

        Task<int> Book(AppointmentServiceModel clientAppointment);

        Task<AppointmentServiceModel> GetById(int id);

        Task<int> Create(List<AppointmentServiceModel> appointmentServiceModels);

        Task<AppointmentServiceModel> Delete(int appointmentId);

        IQueryable<AppointmentServiceModel> GetAllByDate(DateTime date);

        Task<int> Approve(int appointmentId);
    }
}
