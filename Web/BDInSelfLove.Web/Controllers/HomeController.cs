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
        private const string BGCulture = "bg";
        private const string ContactsUserEmailTextEN = "<div>Hello, </div> <div></div> <div>Your email has been received.</div><div>Thank you!</div>";
        private const string ContactsUserEmailTextBG = "<div>Здравей, </div> <div></div> <div>Имейлът ти е получен.</div><div>Благодаря!</div>";
        private const string ContactsStatusMessageEN = "Your email has been received. Thank you!";
        private const string ContactsStatusMessageBG = "Имейлът ти е получен. Благодаря!";
        private const string ContactsAdminEmailText = "<div>{0}</div><div>Name: {1} {2}</div><div>Phone: {3}</div>";

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
                htmlContent: string.Format(ContactsAdminEmailText, userInfo.Message, userInfo.FirstName, userInfo.LastName, userInfo.PhoneNumber));

            // Send email to user
            await this.emailSender.SendEmailAsync(
                from: adminEmail,
                fromName: GlobalValues.SystemName,
                to: userInfo.Email,
                subject: GlobalValues.SystemName,
                htmlContent: culture == BGCulture ? ContactsUserEmailTextBG : ContactsUserEmailTextEN);

            this.TempData["StatusMessage"] = culture == BGCulture ? ContactsStatusMessageBG : ContactsStatusMessageEN;
        }
    }
}
