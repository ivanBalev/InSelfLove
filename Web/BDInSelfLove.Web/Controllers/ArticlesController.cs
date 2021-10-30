namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Articles;
    using BDInSelfLove.Services.Data.CloudinaryServices;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.InputModels.Article;
    using BDInSelfLove.Web.ViewModels.Article;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticlesController : PreviewAndPaginationHelper
    {
        private readonly ICloudinaryService cloudinaryService;

        public ArticlesController(
            ICloudinaryService cloudinaryService,
            IArticleService articleService)
            : base(articleService)
        {
            this.cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var viewModel = await this.GetArticlesPreviewAndPagination(page);

            return this.View(viewModel);
        }

        [HttpGet]
        [Route("Articles/{slug}")]
        public async Task<IActionResult> Single(string slug)
        {
            var viewModel = AutoMapperConfig.MapperInstance
                .Map<ArticleViewModel>(await this.ArticleService
                .GetBySlug(slug));

            for (int i = 0; i < viewModel.Comments.Count; i++)
            {
                viewModel.Comments[i].CreatedOn = TimezoneHelper.ToLocalTime(
                    viewModel.Comments[i].CreatedOn, this.TimezoneCookieValue);
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
            await this.SetArticlePhoto(inputModel);

            string slug = await this.ArticleService
                .Create(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpGet]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await this.ArticleService.GetById(id)
                .To<ArticleEditInputModel>().FirstOrDefaultAsync();

            return this.View(model);
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(ArticleEditInputModel inputModel)
        {
            await this.SetArticlePhoto(inputModel);

            string slug = await this.ArticleService
                .Edit(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            await this.ArticleService.Delete(id);

            return this.Redirect("/");
        }

        // Helper methods
        private async Task SetArticlePhoto(ArticleCreateInputModel inputModel)
        {
            if (inputModel.Image != null)
            {
                inputModel.ImageUrl = await this.cloudinaryService
                .UploadPicture(inputModel.Image, Guid.NewGuid().ToString());
            }
        }
    }
}
