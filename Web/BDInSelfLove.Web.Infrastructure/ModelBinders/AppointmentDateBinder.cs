namespace BDInSelfLove.Web.Infrastructure.ModelBinders
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class AppointmentDateBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            string[] timeSlots = bindingContext.ValueProvider.GetValue("timeSlots").ToArray();
            string date = bindingContext.ValueProvider.GetValue("date").ToString();

            if (timeSlots == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                List<DateTime> parsedDates = new List<DateTime>();
                foreach (var slot in timeSlots)
                {
                    var currentSlot = date + " " + slot;
                    parsedDates.Add(DateTime.ParseExact(currentSlot, "MM-dd-yyyy H:mm", CultureInfo.InvariantCulture));
                }

                bindingContext.Result = ModelBindingResult.Success(parsedDates);
            }

            return Task.CompletedTask;
        }
    }
}
