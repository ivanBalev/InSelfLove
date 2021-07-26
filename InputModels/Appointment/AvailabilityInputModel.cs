using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Appointment
{
    public class AvailabilityInputModel
    {
        public string Date { get; set; }

        public List<string> TimeSlots { get; set; }
    }
}
