using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Infrastructure.ModelBinders
{
    public class AppointmentDateBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string[] timeSlots = bindingContext.ValueProvider.GetValue("TimeSlots").ToArray();

            if (timeSlots == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                List<DateTime> parsedDates = new List<DateTime>();
                foreach (var date in timeSlots)
                {
                    parsedDates.Add(DateTime.ParseExact(date, "MM-dd-yyyy H:mm", CultureInfo.InvariantCulture));
                }

                bindingContext.Result = ModelBindingResult.Success(parsedDates);
            }

            return Task.CompletedTask;
        }
    }
}
