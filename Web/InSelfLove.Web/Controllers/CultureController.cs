namespace InSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;

    public class CultureController : Controller
    {
        [HttpPost]
        public IActionResult SetCulture(string culture, string returnUrl)
        {
            // Add culture cookie to response
            this.Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) });

            // If returnUrl has Cyrillic letters
            if (Regex.IsMatch(returnUrl, @"\p{IsCyrillic}"))
            {
                // Escape non-ASCII characters in redirect header
                returnUrl = "~/" + string.Join("/", returnUrl.Substring(2).Split("/")
                                    .Select(System.Net.WebUtility.UrlEncode));
            }

            return this.LocalRedirect(returnUrl);
        }
    }
}
