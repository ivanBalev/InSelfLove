using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace BDInSelfLove.Web.InputModels.Appointment
{
    public class AvailabilityInputModel
    {
        public string DateString { get; set; }

        public string[] TimeSlotsString { get; set; }

        public DateTime Date => DateTime.ParseExact(DateString, "MM-dd-yyyy", CultureInfo.InvariantCulture);

        public DateTime[] TimeSlots => TimeSlotsString?.Select(ts =>
        {
            // We work only with 00 minutes currently
            double hours = double.Parse(ts.Split(':')[0]);
            return Date.AddHours(hours);
        }).ToArray();
    }
}