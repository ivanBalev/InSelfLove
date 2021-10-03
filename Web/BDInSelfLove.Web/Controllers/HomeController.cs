namespace BDInSelfLove.Web.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.InputModels.Contact;
    using BDInSelfLove.Web.ViewModels;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TimeZoneConverter;

    public class HomeController : BaseController
    {
        private const int IndexItemsCount = 4;
        private const string TimezoneIANACookieName = "timezoneIANA";

        private readonly IArticleService articleService;
        private readonly IVideoService videoService;
        private readonly IEmailSender emailSender;
        private readonly UserManager<ApplicationUser> userManager;

        public HomeController(
            IArticleService articleService,
            IVideoService videoService,
            IEmailSender emailSender,
            UserManager<ApplicationUser> userManager)
        {
            this.articleService = articleService;
            this.videoService = videoService;
            this.emailSender = emailSender;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var lastArticles = await this.articleService.GetAll(IndexItemsCount).To<ArticlePreviewViewModel>().ToListAsync();
            var lastVideos = await this.videoService.GetAll(IndexItemsCount).To<VideoPreviewViewModel>().ToListAsync();
            var viewModel = new HomeViewModel(lastArticles, lastVideos);
            return this.View(viewModel);
        }

        public IActionResult Contacts()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Contacts(ContactFormInputModel userInfo)
        {
            if (!this.ModelState.IsValid)
            {
                this.ViewData["Error"] = "Error. Please try again.";
                return this.View();
            }
            await this.SubmitContactForm(userInfo);
            return this.RedirectToAction("Index");
        }

        public IActionResult About()
        {
            return this.View();
        }

        [Authorize]
        public async Task<IActionResult> Appointments()
        {
            await this.UpdateUserTimezone();
            return this.View();
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }

        // Helper methods
        private async Task UpdateUserTimezone()
        {
            // Query value received from client only if timezone cookie is nonexistent or doesn't match current timezone
            string timezoneIANAQueryValue = this.HttpContext.Request.Query[TimezoneIANACookieName].ToString();

            // Update user db timezone if query value differs from db value
            if (timezoneIANAQueryValue != string.Empty)
            {
                var user = await this.userManager.GetUserAsync(this.User);
                string timezoneWindowsId = TZConvert.GetTimeZoneInfo(timezoneIANAQueryValue).Id;

                if (user.WindowsTimezoneId == null ||
                    user.WindowsTimezoneId.ToLower().CompareTo(timezoneWindowsId.ToLower()) != 0)
                {
                    user.WindowsTimezoneId = timezoneWindowsId;
                    await this.userManager.UpdateAsync(user);
                }
            }
        }

        private async Task SubmitContactForm(ContactFormInputModel userInfo)
        {
            string culture = this.Request.HttpContext.Features.Get<IRequestCultureFeature>().RequestCulture.Culture.Name;
            string adminEmail = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault().Email;

            // Send email to admin
            await this.emailSender.SendEmailAsync(
                from: userInfo.Email,
                fromName: $"{userInfo.FirstName} {userInfo.LastName}",
                to: adminEmail,
                subject: GlobalValues.SystemName,
                htmlContent: string.Format(GlobalValues.ContactsAdminEmailText, userInfo.Message, userInfo.FirstName, userInfo.LastName, userInfo.PhoneNumber));

            // Send email to user
            await this.emailSender.SendEmailAsync(
                from: adminEmail,
                fromName: GlobalValues.SystemName,
                to: userInfo.Email,
                subject: GlobalValues.SystemName,
                htmlContent: culture == "bg" ? GlobalValues.ContactsUserEmailTextBG : GlobalValues.ContactsUserEmailTextEN);

            this.TempData["StatusMessage"] = culture == "bg" ? GlobalValues.ContactsStatusMessageBG : GlobalValues.ContactsStatusMessageEN;
        }
    }
}
