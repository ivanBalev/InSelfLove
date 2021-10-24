namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.CloudinaryService;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.InputModels.Article;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Comment;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Pagination;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticlesController : BaseController
    {
        private const int ArticlesPerPage = 6;
        private readonly IArticleService articleService;
        private readonly ICloudinaryService cloudinaryService;

        public ArticlesController(
            ICloudinaryService cloudinaryService,
            IArticleService articleService)
        {
            this.articleService = articleService;
            this.cloudinaryService = cloudinaryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int page = 1)
        {
            var articles = await this.articleService
                .GetAll(ArticlesPerPage, (page - 1) * ArticlesPerPage)
                .To<ArticlePreviewViewModel>()
                .ToArrayAsync();

            var viewModel = this.CreateIndexViewModel(page, articles);
            return this.View(viewModel);
        }

        [HttpGet]
        [Route("Articles/{slug}")]
        public async Task<IActionResult> Single(string slug)
        {
            var viewModel = AutoMapperConfig.MapperInstance
                .Map<ArticleViewModel>(await this.articleService.GetBySlug(slug));

            this.AdjustCommentsCreatedOn(viewModel.Comments);
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

            string slug = await this.articleService
                .Create(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpGet]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            var model = await this.articleService.GetById(id)
                .To<ArticleEditInputModel>().FirstOrDefaultAsync();
            return this.View(model);
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Edit(ArticleEditInputModel inputModel)
        {
            await this.SetArticlePhoto(inputModel);

            string slug = await this.articleService.Edit(AutoMapperConfig.MapperInstance.Map<Article>(inputModel));
            return this.RedirectToAction("Single", new { slug });
        }

        [HttpPost]
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            await this.articleService.Delete(id);
            return this.Redirect("/");
        }

        // Helper methods
        private void AdjustCommentsCreatedOn(ICollection<CommentViewModel> comments)
        {
            var userTimezone = TimezoneHelper.GetUserWindowsTimezone(this.TimezoneCookieValue);
            if (userTimezone != null)
            {
                foreach (var comment in comments)
                {
                    comment.CreatedOn = TimezoneHelper.ToLocalTime(comment.CreatedOn, this.TimezoneCookieValue);
                }
            }
        }

        private async Task<ArticlesPaginationViewModel> CreateIndexViewModel(int currentPage, ArticlePreviewViewModel[] articles)
        {
            var pagesCount = (int)Math.Ceiling(await this.articleService.GetAll().CountAsync() / (decimal)ArticlesPerPage);

            var viewModel = new ArticlesPaginationViewModel()
            {
                Articles = articles,
                PaginationInfo = new PaginationViewModel
                {
                    CurrentPage = currentPage,
                    PagesCount = pagesCount,
                },
            };
            return viewModel;
        }

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
