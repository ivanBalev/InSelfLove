using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Middlewares
{
    public class RedirectToGoogleIfNotHttps
    {
        private readonly RequestDelegate next;

        public RedirectToGoogleIfNotHttps(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if(!context.Request.IsHttps)
            {
                context.Response.StatusCode = 307;
                context.Response.Headers["Location"] = "https://google.com";
            }
            else
            {
                await this.next(context);
            }
        }
    }
}
