namespace BDInSelfLove.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
        protected string TimezoneCookieValue => this.HttpContext.Request.Cookies["timezoneIANA"];
    }
}
