namespace BDInSelfLove.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
        public string TimezoneIdFromCookie => this.HttpContext.Request.Cookies["timezoneIANA"];
    }
}
