using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Appointment;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Appointment
{
    public class AppointmentViewModel : IMapFrom<AppointmentServiceModel>
    {
        public int Id { get; set; }

        public DateTime Start { get; set; }
    }
}
