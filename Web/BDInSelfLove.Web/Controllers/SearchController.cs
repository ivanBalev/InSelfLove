using BDInSelfLove.Services.Data;
using BDInSelfLove.Services.Data.Search;
using BDInSelfLove.Services.Data.Video;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Web.ViewModels.Article;
using BDInSelfLove.Web.ViewModels.Home;
using BDInSelfLove.Web.ViewModels.Search;
using BDInSelfLove.Web.ViewModels.Video;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Controllers
{
    public class SearchController : BaseController
    {
        private const int ArticlesPerPage = 6;
        private const int VideosPerPage = 3;


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
                    CurrentPage = 1,
                    PagesCount = articlePagesCount,
                    Articles = serviceModel.Articles.Select(a => AutoMapperConfig.MapperInstance.Map<BriefArticleInfoViewModel>(a)).ToList(),
                },
                VideosPagination = new VideoPaginationViewModel
                {
                    CurrentPage = 1,
                    PagesCount = videoPagesCount,
                    Videos = serviceModel.Videos.Select(v => AutoMapperConfig.MapperInstance.Map<VideoViewModel>(v)).ToList(),
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
                Articles = serviceModel.Articles.Select(a => AutoMapperConfig.MapperInstance.Map<BriefArticleInfoViewModel>(a)).ToList(),
                PagesCount = pagesCount == 0 ? 1 : pagesCount,
                CurrentPage = page,
            };

            return this.View(viewModel);
        }

        [Route("api/Search/Video")]
        public async Task<IActionResult> Video(int page, string searchTerm)
        {
            var serviceModel = await this.searchService
                .GetVideos(searchTerm, VideosPerPage, (page - 1) * VideosPerPage);

            var pagesCount = (int)Math.Ceiling(serviceModel.VideosCount / (decimal)VideosPerPage);

            var viewModel = new VideoPaginationViewModel
            {
                Videos = serviceModel.Videos.Select(v => AutoMapperConfig.MapperInstance.Map<VideoViewModel>(v)).ToList(),
                PagesCount = pagesCount == 0 ? 1 : pagesCount,
                CurrentPage = page,
            };

            return this.View(viewModel);
        }
    }
}
