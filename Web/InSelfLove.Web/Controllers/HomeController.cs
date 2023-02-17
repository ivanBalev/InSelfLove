namespace InSelfLove.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Data.Recaptcha;
    using InSelfLove.Services.Data.Videos;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Services.Messaging;
    using InSelfLove.Web.InputModels.Contact;
    using InSelfLove.Web.ViewModels.Home;
    using InSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Localization;
    using Microsoft.Extensions.Logging;

    public class HomeController : Controller
    {
        private const string SuccessMessage = "Success";
        private const string UserEmailBody = "UserEmailBody";
        private const string AdminEmailBodyTemplate = "AdminEmailBodyTemplate";
        private const string ErrorMessage = "Error";

        private readonly IStringLocalizer<HomeController> localizer;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IArticleService articleService;
        private readonly IRecaptchaService recaptchaService;
        private readonly IVideoService videoService;
        private readonly IEmailSender emailSender;
        private readonly ILogger logger;

        public HomeController(
            ILogger<HomeController> logger,
            IEmailSender emailSender,
            IVideoService videoService,
            IArticleService articleService,
            IRecaptchaService recaptchaService,
            UserManager<ApplicationUser> userManager,
            IStringLocalizer<HomeController> localizer)
        {
            this.articleService = articleService;
            this.recaptchaService = recaptchaService;
            this.videoService = videoService;
            this.emailSender = emailSender;
            this.userManager = userManager;
            this.localizer = localizer;
            this.logger = logger;
        }

        public static int IndexVideosCount => 3;

        public static int IndexArticlesCount => 5;

        public async Task<IActionResult> Index()
        {
            // Gather data
            var lastArticles = await this.articleService.GetAll(IndexArticlesCount)
                               .To<ArticlePreviewViewModel>().ToListAsync();
            var lastVideos = await this.videoService.GetAll(IndexVideosCount)
                             .To<VideoPreviewViewModel>().ToListAsync();

            // Create view model
            var viewModel = new HomeViewModel(lastArticles, lastVideos);

            return this.View(viewModel);
        }

        public IActionResult Contacts()
        {
            return this.View();
        }

        // JS Fetch request
        [HttpPost]
        public async Task<IActionResult> Contacts([FromBody] ContactFormInputModel userInfo)
        {
            // Validate input
            if (!this.ModelState.IsValid)
            {
                // Return status message partial for client to append to page
                return this.View("_StatusMessagePartial", this.localizer[ErrorMessage].ToString());
            }

            // Assert data is sent by actual user
            string verificationErrors = await this.recaptchaService.VerifyAsync(
                                        userInfo.RecaptchaToken, userInfo.RecaptchaExpectedAction);

            // If user is bot
            if (!string.IsNullOrEmpty(verificationErrors))
            {
                // Log error & return bad request
                this.logger.LogWarning(verificationErrors);
                return this.BadRequest();
            }

            // Submit contact form & confirm to user
            await this.SubmitContactForm(userInfo);
            return this.View("_StatusMessagePartial", this.localizer[SuccessMessage].ToString());
        }

        public IActionResult About()
        {
            return this.View();
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        // Prevent client from caching the response
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View();
        }

        // Helper methods
        private async Task SubmitContactForm(ContactFormInputModel userInfo)
        {
            string adminEmail = (await this.userManager.GetUsersInRoleAsync(
                                AppConstants.AdministratorRoleName)).FirstOrDefault().Email;

            // Email bodies are stored in resource file
            // Not ideal but does the job at least for now

            // Send email to admin
            await this.emailSender.SendEmailAsync(
                                   from: userInfo.Email,
                                   fromName: $"{userInfo.FirstName} {userInfo.LastName}",
                                   to: adminEmail,
                                   subject: AppConstants.SystemName,
                                   htmlContent: string.Format(
                                                this.localizer[AdminEmailBodyTemplate],
                                                userInfo.Message,
                                                userInfo.FirstName,
                                                userInfo.LastName,
                                                userInfo.PhoneNumber));

            // Send email to user
            await this.emailSender.SendEmailAsync(
                from: adminEmail,
                fromName: AppConstants.SystemName,
                to: userInfo.Email,
                subject: AppConstants.SystemName,
                htmlContent: this.localizer[UserEmailBody]);
        }
    }
}
