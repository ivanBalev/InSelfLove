namespace BDInSelfLove.Web.Infrastructure.Filters.ActionFilters
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.Filters;

    public class YoutubeLinkActionFilterAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var obj = context.ActionArguments.FirstOrDefault().Value;
            var value = obj.GetType().GetProperties().FirstOrDefault()
                .GetValue(obj);

            var replacedValue = ((string)value).Replace("watch?v=", "embed/").Split('&')[0].ToString();

            obj.GetType().GetProperties().FirstOrDefault().SetValue(obj, replacedValue);

            await next();
        }
    }
}
