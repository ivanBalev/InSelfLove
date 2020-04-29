namespace BDInSelfLove.Web.ViewModels.Appointment
{
    using System;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Appointment;

    public class AppointmentViewModel : IMapFrom<AppointmentServiceModel>
    {
        public int Id { get; set; }

        public DateTime Start { get; set; }
    }
}
