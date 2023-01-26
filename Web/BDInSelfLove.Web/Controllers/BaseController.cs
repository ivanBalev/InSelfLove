namespace BDInSelfLove.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
        // TODO: Custom Middleware?
        public string UserTimezoneIdFromCookie => this.HttpContext.Request.Cookies["timezoneIANA"];
    }
}
