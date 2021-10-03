namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.CloudinaryService;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Article;
    using BDInSelfLove.Web.InputModels.Article;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Pagination;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TimeZoneConverter;

    public class ArticlesController : BaseController
    {
        private const int ArticlesPerPage = 6;
        private const string ArticleCreateError = "Error while creating article. Please try again.";
        private const string ArticleEditError = "Error while editing article. Please try again.";

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IArticleService articleService;
        private readonly ICloudinaryService cloudinaryService;

        public ArticlesController(
            UserManager<ApplicationUser> userManager,
            IArticleService articleService,
            ICloudinaryService cloudinaryService)
        {
            this.userManager = userManager;
            this.articleService = articleService;
            this.cloudinaryService = cloudinaryService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var serviceModel = this.articleService
                .GetAllPagination(ArticlesPerPage, (page - 1) * ArticlesPerPage);

            var pagesCount = (int)Math.Ceiling(this.articleService.GetAll().Count() / (decimal)ArticlesPerPage);
            var viewModel = new ArticlePaginationViewModel()
            {
                Articles = (await serviceModel.ToListAsync())
                           .Select(a => AutoMapperConfig.MapperInstance.Map<ArticlePreviewViewModel>(a))
                           .ToList(),
                PaginationInfo = new PaginationViewModel
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    PagesCount = pagesCount == 0 ? 1 : pagesCount,
                    CurrentPage = page,
                },
            };

            return this.View(viewModel);
        }

        [Route("Articles/{slug}")]
        public async Task<IActionResult> Single(string slug)
        {
            ArticleViewModel viewModel = AutoMapperConfig.MapperInstance
                .Map<ArticleViewModel>(await this.articleService
                .GetBySlug(slug));

            if (this.User.Identity.IsAuthenticated)
            {
                // Convert comment CreatedOn to user local time
                TimeZoneInfo userTimezone = TZConvert.GetTimeZoneInfo(
                    (await this.userManager.GetUserAsync(this.User)).WindowsTimezoneId);
                foreach (var comment in viewModel.Comments)
                {
                    comment.CreatedOn = TimeZoneInfo.ConvertTimeFromUtc(comment.CreatedOn, userTimezone);
                }
            }

            return this.View(viewModel);
        }

        // Admin acces only below
        [HttpGet]
        [Route("Articles/Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Route("Articles/Create")]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Create(ArticleCreateInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.ViewData["Error"] = ArticleCreateError;
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);

            var serviceModel = AutoMapperConfig.MapperInstance.Map<ArticleServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            if (inputModel.Image != null)
            {
                var imageUrl = await this.cloudinaryService.UploadPicture(inputModel.Image, Guid.NewGuid().ToString());
                serviceModel.ImageUrl = imageUrl;
            }

            string slug = await this.articleService.CreateAsync(serviceModel);
            return this.RedirectToAction("Single", new { slug });
        }

        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            var articleServiceModel = await this.articleService.GetById(id);
            var articleEditViewModel = AutoMapperConfig.MapperInstance.Map<ArticleEditInputModel>(articleServiceModel);

            return this.View(articleEditViewModel);
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(ArticleEditInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.ViewData["Error"] = ArticleEditError;
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<ArticleServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            if (inputModel.Image != null)
            {
                var imageUrl = await this.cloudinaryService.UploadPicture(inputModel.Image, inputModel.Title);
                serviceModel.ImageUrl = imageUrl;
            }

            string slug = await this.articleService.Edit(serviceModel);

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            await this.articleService.Delete(id);
            return this.Redirect("/");
        }

    }
}
