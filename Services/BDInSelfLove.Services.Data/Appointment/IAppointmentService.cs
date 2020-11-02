namespace BDInSelfLove.Services.Data.Calendar
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Models.Appointment;

    public interface IAppointmentService
    {
        IQueryable<AppointmentServiceModel> GetAll(string userId = null);

        Task<AppointmentServiceModel> GetById(int id);

        Task<int> Create(AppointmentServiceModel appointmentServiceModel);

        Task<AppointmentServiceModel> Delete(int appointmentId);

        IQueryable<AppointmentServiceModel> GetAllByDate(DateTime date);
    }
}
