namespace BDInSelfLove.Web.ViewModels.Appointment
{
    using System;
    using System.Collections.Generic;

    public class AppointmentIndexViewModel
    {
        public DateTime WorkdayStart { get; set; }

        public DateTime WorkdayEnd { get; set; }

        public IEnumerable<AppointmentViewModel> Appointments { get; set; }
    }
}
