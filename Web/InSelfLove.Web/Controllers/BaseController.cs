namespace InSelfLove.Web.Controllers
{
    using Microsoft.AspNetCore.Mvc;

    public class BaseController : Controller
    {
        public string UserTimezoneIdFromCookie => this.HttpContext.Request.Cookies["timezoneIANA"];
    }
}
