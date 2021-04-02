namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.InputModels.Contact;
    using BDInSelfLove.Web.ViewModels;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Localization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeController : BaseController
    {
        private const int IndexArticlesCount = 4;

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
            // Get data
            var serviceData = await this.articleService
               .GetAll(IndexArticlesCount).ToListAsync();

            // Parse
            var lastArticles = serviceData.Select(x =>
            AutoMapperConfig.MapperInstance.Map<BriefArticleInfoViewModel>(x)).ToList();

            var lastVideo = await this.videoService.GetAll(1).FirstOrDefaultAsync();

            var videoisLatest = DateTime.Compare(lastArticles[0].CreatedOn, lastVideo.CreatedOn) == -1;

            // Create view model
            var viewModel = new HomeViewModel
            {
                FeaturedArticle = videoisLatest ? null : lastArticles[0],
                LastArticles = lastArticles.Skip(1).ToList(),
                FeaturedVideo = videoisLatest ? AutoMapperConfig.MapperInstance.Map<VideoViewModel>(lastVideo) : null,
            };

            return this.View(viewModel);
        }

        public IActionResult Contact()
        {
            return this.View();
        }

        public IActionResult About()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Contact(ContactFormInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.ViewData["Error"] = "Error. Please try again.";
                return this.View();
            }

            var rqf = this.Request.HttpContext.Features.Get<IRequestCultureFeature>();
            var culture = rqf.RequestCulture.Culture.Name;

            var adminEmail = (await this.userManager.GetUsersInRoleAsync(GlobalConstants.AdministratorRoleName)).FirstOrDefault().Email;
            var userEmailTextEN = $"<div>Hello, </div> <div></div> <div>Your email has been received.</div><div>Thank you!</div>";
            var userEmailTextBG = $"<div>Здравей, </div> <div></div> <div>Имейлът ти е получен.</div><div>Благодаря!</div>";

            // Send emails to admin and user
            await this.emailSender.SendEmailAsync(inputModel.Email, $"{inputModel.FirstName} {inputModel.LastName}", adminEmail, GlobalConstants.SystemName, $"<div>{inputModel.Message}</div><div>Name: {inputModel.FirstName} {inputModel.LastName}</div><div>Phone: {inputModel.PhoneNumber}</div>");
            await this.emailSender.SendEmailAsync(adminEmail, GlobalConstants.SystemName, inputModel.Email, GlobalConstants.SystemName, culture == "bg" ? userEmailTextBG : userEmailTextEN);

            this.TempData["StatusMessage"] = culture == "bg" ? "Имейлът ти е получен. Благодаря!" : "Your email has been received. Thank you!";
            return this.RedirectToAction("Index");
        }

        [Authorize]
        public IActionResult Appointment()
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
            var context = this.HttpContext;

            return this.View(
                new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}
