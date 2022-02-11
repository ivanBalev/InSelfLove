namespace BDInSelfLove.Web.Controllers
{
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Articles;
    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.InputModels.Contact;
    using BDInSelfLove.Web.ViewModels;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;

    public class HomeController : Controller
    {
        private const int IndexItemsCount = 3;
        private const string StatusMessage = "StatusMessage";
        private const string UserEmailBody = "UserEmailBody";
        private const string AdminEmailBodyTemplate = "AdminEmailBodyTemplate";

        private readonly IStringLocalizer<HomeController> localizer;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IArticleService articleService;
        private readonly IVideoService videoService;
        private readonly IEmailSender emailSender;

        public HomeController(
            IEmailSender emailSender,
            IVideoService videoService,
            IArticleService articleService,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<HomeController> localizer)
        {
            this.articleService = articleService;
            this.videoService = videoService;
            this.emailSender = emailSender;
            this.userManager = userManager;
            this.localizer = localizer;
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
            string adminEmail = (await this.userManager.GetUsersInRoleAsync(GlobalValues.AdministratorRoleName)).FirstOrDefault().Email;

            // Send email to admin
            await this.emailSender.SendEmailAsync(
                from: userInfo.Email,
                fromName: $"{userInfo.FirstName} {userInfo.LastName}",
                to: adminEmail,
                subject: GlobalValues.SystemName,
                htmlContent: string.Format(this.localizer[AdminEmailBodyTemplate], userInfo.Message, userInfo.FirstName, userInfo.LastName, userInfo.PhoneNumber));

            // Send email to user
            await this.emailSender.SendEmailAsync(
                from: adminEmail,
                fromName: GlobalValues.SystemName,
                to: userInfo.Email,
                subject: GlobalValues.SystemName,
                htmlContent: this.localizer[UserEmailBody]);

            this.TempData["StatusMessage"] = this.localizer[StatusMessage];
        }
    }
}
