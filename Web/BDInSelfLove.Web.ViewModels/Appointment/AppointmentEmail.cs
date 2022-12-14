using System;

namespace BDInSelfLove.Web.ViewModels.Appointment
{
    public class AppointmentEmail
    {
        // TODO: for now string, later enum
        public string Status { get; set; }

        public DateTime Start { get; set; }

        public string Description { get; set; }

        public bool IsOnSite { get; set; }
    }
}
