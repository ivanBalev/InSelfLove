
namespace BDInSelfLove.Web.InputModels.Appointment
{
    using System;
    using System.Globalization;
    using System.Linq;

    public class AvailabilityInputModel
    {
        // DateString and TimeSlotsString are sent by client
        // We parse them to C# native date formats within this model
        public string DateString { get; set; } = string.Empty;

        public string[] TimeSlotsString { get; set; } = new string[0];

        // Parse date string
        public DateTime Date => DateTime.ParseExact(DateString, "M-d-yyyy", CultureInfo.InvariantCulture);

        // Parse time slots
        public DateTime[] TimeSlots => TimeSlotsString?.Select(ts =>
        {
            var slotArray = ts.Split(':');
            double hours = double.Parse(slotArray[0]);

            // if slot has minutes
            if (slotArray[1] != "00")
            {
                // Currently working only with 30-minute intervals
                hours += 0.5;
            }

            // Add slot time to the date to complete the appointment
            return Date.AddHours(hours);
        }).ToArray();
    }
}