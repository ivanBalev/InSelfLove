
namespace BDInSelfLove.Web.InputModels.Appointment
{
    using System;
    using System.Globalization;
    using System.Linq;

    public class AvailabilityInputModel
    {
        public string DateString { get; set; } = string.Empty;

        public string[] TimeSlotsString { get; set; } = new string[0];

        public DateTime Date => DateTime.ParseExact(DateString, "M-d-yyyy", CultureInfo.InvariantCulture);

        public DateTime[] TimeSlots => TimeSlotsString?.Select(ts =>
        {
            // We work only with 00 minutes currently
            double hours = double.Parse(ts.Split(':')[0]);
            return Date.AddHours(hours);
        }).ToArray();
    }
}