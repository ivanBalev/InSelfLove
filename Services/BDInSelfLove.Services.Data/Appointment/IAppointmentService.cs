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

        Task<int> Book(DateTime utcStart, string appointmentDescription, string userId);

        Task<AppointmentServiceModel> GetById(int id);

        Task<int> Create(List<AppointmentServiceModel> appointmentServiceModels);

        Task<AppointmentServiceModel> Delete(int appointmentId);

        Task<int> Approve(int appointmentId);

        Task<int> Cancel(int id);
    }
}
