namespace BDInSelfLove.Web.Filters
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc.Filters;

    // Asynchronous filter where before and after are contained within a single method
    public class AddHeaderAsyncActionFilterAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // before action
            if (DateTime.UtcNow.Second % 2 == 0)
            {
                return;
            }

            await next();

            // after action
        }
    }

    // Synchronous Filter before and after action execution
    // ТУК РАБОТИХМЕ НАЙ-МНОГО - ЗАТОВА ИМА НАЙ-МНОГО ПИСАНО КАТО ПРИМЕРИ
    public class AddHeaderActionFilterAttribute : Attribute, IActionFilter
    {
        //private readonly string header;
        //private readonly string value;

        public AddHeaderActionFilterAttribute(/*ADDED USING DEPENDENCY INJECTION*/)
        {
            //this.header = header;
            //this.value = value;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {

        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            // context.HttpContext.Response.Headers.Add(this.header, this.value);
        }
    }
}
