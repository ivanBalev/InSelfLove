namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;

    public class CultureController : Controller
    {
        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            this.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            // Escape non-ASCII characters in redirect header (returnUrl has Cyrillic letters)
            returnUrl = "~/" + string.Join("/", returnUrl.Substring(2).Split("/")
                                     .Select(s => System.Net.WebUtility.UrlEncode(s)));

            return this.LocalRedirect(returnUrl);
        }
    }
}
