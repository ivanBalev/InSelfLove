namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class BaseController : Controller
    {
        private const string TimezoneIANACookieName = "timezoneIANA";
        private const string ConsentCookieName = ".AspNet.Consent";

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Manage timezone
            // Query value received from client only if timezone cookie is nonexistent or doesn't match current timezone
            string timezoneIANAQueryValue = this.HttpContext.Request.Query[TimezoneIANACookieName].ToString();
            string consentCookieValue = this.HttpContext.Request.Cookies[ConsentCookieName];

            // Update/create cookie
            if (timezoneIANAQueryValue != string.Empty && consentCookieValue != null)
            {
                this.HttpContext.Response.Cookies.Delete(TimezoneIANACookieName);
                CookieOptions cookieOptions = new CookieOptions();
                cookieOptions.Expires = DateTime.Now.AddDays(7);
                cookieOptions.SameSite = SameSiteMode.Lax;
                this.HttpContext.Response.Cookies.Append(TimezoneIANACookieName, timezoneIANAQueryValue, cookieOptions);
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
