namespace BDInSelfLove.Web.Infrastructure.ModelBinders
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class YearModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            // Somewhere in the context, we set the key "date" with a certain value
            var stringDateFromRequest = bindingContext.ValueProvider.GetValue("date").FirstOrDefault();

            if(stringDateFromRequest == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
            }
            else
            {
            // We extract our particular data
            var date = DateTime.Parse(stringDateFromRequest);
            bindingContext.Result = ModelBindingResult.Success(date.Year);
            }

            return Task.CompletedTask;
        }
    }
}
