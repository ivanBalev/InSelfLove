
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
            var slotArr = ts.Split(':');
            double hours = double.Parse(slotArr[0]);
            if (slotArr[1] != "00")
            {
                hours += 0.5;
            }

            return Date.AddHours(hours);
        }).ToArray();
    }
}