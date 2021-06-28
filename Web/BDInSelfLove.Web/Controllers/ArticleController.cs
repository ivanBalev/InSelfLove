namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Pagination;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class ArticleController : BaseController
    {
        private const int ArticlesPerPage = 6;
        private const int SideArticlesCount = 2;
        private const int SideVideosCount = 2;

        private readonly IArticleService articleService;
        private readonly IVideoService videoService;

        public ArticleController(IArticleService articleService, IVideoService videoService)
        {
            this.articleService = articleService;
            this.videoService = videoService;
        }

        public async Task<IActionResult> All(int page = 1)
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

        public async Task<IActionResult> Single(int id)
        {
            var viewModel = await this.GetViewModel(id);
            return this.View(viewModel);
        }

        private async Task<ArticleViewModel> GetViewModel(int id) =>
            AutoMapperConfig.MapperInstance.Map<ArticleViewModel>(await this.articleService.GetById(id));
    }
}
