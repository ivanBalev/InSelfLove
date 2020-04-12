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
            var date = bindingContext.ValueProvider.GetValue("date").FirstOrDefault();

            if (date == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
                var formattedDate = string.Join(" ", date.Split(' ').Skip(1).Take(3));
                var parsedDate = DateTime.ParseExact(formattedDate, "MMM dd yyyy", CultureInfo.InvariantCulture);
                bindingContext.Result = ModelBindingResult.Success(parsedDate);
            }

            return Task.CompletedTask;
        }
    }
}
