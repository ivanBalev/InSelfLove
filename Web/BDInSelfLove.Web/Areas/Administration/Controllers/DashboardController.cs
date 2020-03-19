namespace BDInSelfLove.Web.Areas.Administration.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Articles;
    using BDInSelfLove.Services.Models.Videos;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.ViewModels.Administration.Dashboard.Articles;
    using BDInSelfLove.Web.ViewModels.Administration.Dashboard.Videos;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class DashboardController : AdministrationController
    {
        private const string CreateErrorMessage = "{0} could not be created. Please try again.";

        private readonly IArticleService articlesService;
        private readonly IVideoService videosService;
        private readonly UserManager<ApplicationUser> userManager;

        public DashboardController(IArticleService articlesService, IVideoService videosService,
            UserManager<ApplicationUser> userManager)
        {
            this.articlesService = articlesService;
            this.videosService = videosService;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult Articles()
        {
            return this.View();
        }

        [HttpPost]
        public async Task<IActionResult> Articles(CreateArticleInputModel createArticleInputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(createArticleInputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);

            var articleServiceModel = AutoMapperConfig.MapperInstance.Map<ArticleServiceModel>(createArticleInputModel);
            articleServiceModel.User = null;
            articleServiceModel.UserId = user.Id;

            var postId = await this.articlesService.CreateAsync(articleServiceModel);

            if (postId == 0)
            {
                this.TempData["Error"] = string.Format(CreateErrorMessage, "Article");

                return this.View(createArticleInputModel);
            }

            return this.RedirectToAction("Single", "Articles", new { area = string.Empty, id = postId });
        }

        [HttpGet]
        public IActionResult Videos()
        {
            return this.View();
        }

        [HttpPost]
        [YoutubeLinkActionFilter]
        public async Task<IActionResult> Videos(CreateVideoInputModel createVideoInputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(createVideoInputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);

            var videoServiceModel = AutoMapperConfig.MapperInstance.Map<VideoServiceModel>(createVideoInputModel);
            videoServiceModel.User = null;
            videoServiceModel.UserId = user.Id;

            var videoId = await this.videosService.CreateAsync(videoServiceModel);

            if (videoId == 0)
            {
                this.TempData["Error"] = string.Format(CreateErrorMessage, "Video");

                return this.View(createVideoInputModel);
            }

            return this.RedirectToAction("Index", "Videos", new { area = string.Empty });
        }
    }
}
