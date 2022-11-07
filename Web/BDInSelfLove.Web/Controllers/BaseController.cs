namespace BDInSelfLove.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
		// TODO: Custom Middleware?
        public string TimezoneIdFromCookie => this.HttpContext.Request.Cookies["timezoneIANA"];
    }
}
