using BDInSelfLove.Services.Data.Search;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Web.ViewModels.Article;
using BDInSelfLove.Web.ViewModels.Home;
using BDInSelfLove.Web.ViewModels.Pagination;
using BDInSelfLove.Web.ViewModels.Search;
using BDInSelfLove.Web.ViewModels.Video;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Controllers
{
    public class SearchController : BaseController
    {
        private const int ArticlesPerPage = 6;
        private const int VideosPerPage = 3;
        private const string ArticleControllerName = "Article";
        private const string VideoControllerName = "Video";

        private ISearchService searchService;

        public SearchController(ISearchService searchService)
        {
            this.searchService = searchService;
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm) || string.IsNullOrWhiteSpace(searchTerm) || searchTerm.All(ch => ch == ' '))
            {
                return this.BadRequest();
            }

            var serviceModel = await this.searchService.Index(searchTerm);

            var articlePagesCount = (int)Math.Ceiling(serviceModel.ArticlesCount / (decimal)ArticlesPerPage);
            var videoPagesCount = (int)Math.Ceiling(serviceModel.VideosCount / (decimal)VideosPerPage);

            var viewModel = new IndexSearchViewModel
            {
                SearchTerm = searchTerm,
                ArticlesPagination = new ArticlePaginationViewModel
                {
                    PaginationInfo = new PaginationViewModel
                    {
                        ControllerName = ArticleControllerName,
                        PagesCount = articlePagesCount,
                        CurrentPage = 1,
                    },
                    Articles = serviceModel.Articles.Select(a => AutoMapperConfig.MapperInstance.Map<ArticlePreviewViewModel>(a)).ToList(),
                },
                VideosPagination = new VideoPaginationViewModel
                {
                    Videos = serviceModel.Videos.Select(v => AutoMapperConfig.MapperInstance.Map<VideoPreviewViewModel>(v)).ToList(),
                    PaginationInfo = new PaginationViewModel
                    {
                        ControllerName = VideoControllerName,
                        CurrentPage = 1,
                        PagesCount = videoPagesCount,
                    },
                },
            };

            return this.View(viewModel);
        }

        [Route("api/Search/Article")]
        public async Task<IActionResult> Article(int page, string searchTerm)
        {
            var serviceModel = await this.searchService
                .GetArticles(searchTerm, ArticlesPerPage, (page - 1) * ArticlesPerPage);

            var pagesCount = (int)Math.Ceiling(serviceModel.ArticlesCount / (decimal)ArticlesPerPage);

            var viewModel = new ArticlePaginationViewModel
            {
                Articles = serviceModel.Articles.Select(a => AutoMapperConfig.MapperInstance.Map<ArticlePreviewViewModel>(a)).ToList(),
                PaginationInfo = new PaginationViewModel
                {
                    ControllerName = ArticleControllerName,
                    PagesCount = pagesCount == 0 ? 1 : pagesCount,
                    CurrentPage = page,
                },
            };

            return this.View("_ArticlesAllPartial", viewModel);
        }

        [Route("api/Search/Video")]
        public async Task<IActionResult> Video(int page, string searchTerm)
        {
            var serviceModel = await this.searchService
                .GetVideos(searchTerm, VideosPerPage, (page - 1) * VideosPerPage);

            var pagesCount = (int)Math.Ceiling(serviceModel.VideosCount / (decimal)VideosPerPage);

            var viewModel = new VideoPaginationViewModel
            {
                Videos = serviceModel.Videos.Select(v => AutoMapperConfig.MapperInstance.Map<VideoPreviewViewModel>(v)).ToList(),
                PaginationInfo = new PaginationViewModel
                {
                    ControllerName = VideoControllerName,
                    PagesCount = pagesCount == 0 ? 1 : pagesCount,
                    CurrentPage = page,
                },
            };

            return this.View("_VideosAllPartial", viewModel);
        }
    }
}
