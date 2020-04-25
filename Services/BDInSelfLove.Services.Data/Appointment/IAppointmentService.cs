using BDInSelfLove.Services.Models.Appointment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Services.Data.Calendar
{
    public interface IAppointmentService
    {
        IQueryable<AppointmentServiceModel> GetAll(string userId = null);

        Task<AppointmentServiceModel> GetById(int id);

        Task<int> Create(AppointmentServiceModel appointmentServiceModel);

        Task<bool> Delete(int appointmentId);

        IQueryable<AppointmentServiceModel> GetAllByDate(DateTime date);
    }
}
