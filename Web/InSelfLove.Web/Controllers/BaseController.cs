namespace InSelfLove.Web.Controllers
{
    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Helpers;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using System.Threading.Tasks;

    public class BaseController : Controller
    {
        public string UserTimezoneIdFromCookie => this.HttpContext.Request.Cookies["timezoneIANA"];
    }
}
