namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Pagination;
    using BDInSelfLove.Web.ViewModels.Search;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class SearchController : BaseController
    {
        private const int ArticlesPerPage = 6;
        private const int VideosPerPage = 3;
        private const string ArticleActionName = "Article";
        private const string VideoActionName = "Video";

        private IArticleService articleService;
        private IVideoService videoService;

        public SearchController(
            IArticleService articleService,
            IVideoService videoService)
        {
            this.articleService = articleService;
            this.videoService = videoService;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            var viewModel = new IndexSearchViewModel
            {
                SearchTerm = searchTerm,
                ArticlesPagination = await this.GetArticlesViewModel(searchTerm, 0),
                VideosPagination = await this.GetVideosViewModel(searchTerm, 0),
            };

            return this.View(viewModel);
        }

        [Route("api/Search/Article")]
        public async Task<IActionResult> Article(int page, string searchTerm)
        {
            var viewModel = await this.GetArticlesViewModel(searchTerm, page);

            return this.View("_ArticlesAllPartial", viewModel);
        }

        [Route("api/Search/Video")]
        public async Task<IActionResult> Video(int page, string searchTerm)
        {
            var viewModel = await this.GetVideosViewModel(searchTerm, page);

            return this.View("_VideosAllPartial", viewModel);
        }

        private async Task<ArticlesPaginationViewModel> GetArticlesViewModel(string searchTerm, int page)
        {
            var articlePagesCount = (int)Math.Ceiling(await this.articleService.GetAll().CountAsync() / (decimal)ArticlesPerPage);

            return new ArticlesPaginationViewModel
            {
                Articles = await this.articleService.GetAll(ArticlesPerPage, (page - 1) * ArticlesPerPage, searchTerm)
                    .To<ArticlePreviewViewModel>().ToArrayAsync(),
                PaginationInfo = new PaginationViewModel
                {
                    PagesCount = articlePagesCount,
                    CurrentPage = 1,
                    ActionName = ArticleActionName,
                },
            };
        }

        private async Task<VideosPaginationViewModel> GetVideosViewModel(string searchTerm, int page)
        {
            var videoPagesCount = (int)Math.Ceiling(await this.videoService.GetAll().CountAsync() / (decimal)VideosPerPage);

            return new VideosPaginationViewModel
            {
                Videos = await this.videoService.GetAll(VideosPerPage, (page - 1) * VideosPerPage, searchTerm)
                    .To<VideoPreviewViewModel>().ToArrayAsync(),
                PaginationInfo = new PaginationViewModel
                {
                    CurrentPage = 1,
                    PagesCount = videoPagesCount,
                    ActionName = VideoActionName,
                },
            };
        }
    }
}
