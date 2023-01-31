namespace InSelfLove.Web.Controllers.Helpers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.Videos;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Web.ViewModels.Article;
    using InSelfLove.Web.ViewModels.Home;
    using InSelfLove.Web.ViewModels.Pagination;
    using InSelfLove.Web.ViewModels.Video;
    using Microsoft.EntityFrameworkCore;

    public class PaginationHelper : BaseController
    {
        private const int ArticlesPerPage = 6;
        private const int VideosPerPage = 6;

        private IArticleService articleService;
        private IVideoService videoService;

        // Constructor for Search controller
        protected PaginationHelper(IArticleService articleService, IVideoService videoService)
        {
            this.articleService = articleService;
            this.videoService = videoService;
        }

        // Costructor for Articles controller
        protected PaginationHelper(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        // Costructor for Videos controller
        protected PaginationHelper(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        protected async Task<ArticlesPaginationViewModel> GetArticlesPreview(
            int page, string searchTerm = null, string actionName = null)
        {
            // Calculate the total number of articles that match
            // (method is used in both Articles & Search controllers)
            var pagesCount = (int)Math.Ceiling(await this.articleService
                                             .GetAll(null, 0, searchTerm)
                                             .CountAsync() / (decimal)ArticlesPerPage);

            // Get all articles for current page
            var articles = await this.articleService
                    .GetAll(ArticlesPerPage, (page - 1) * ArticlesPerPage, searchTerm)
                    .To<ArticlePreviewViewModel>().ToArrayAsync();

            // Return view model
            return new ArticlesPaginationViewModel
            {
                Articles = articles,
                PaginationInfo = this.GetPaginationInfo(page, actionName, pagesCount),
            };
        }

        protected async Task<VideosPaginationViewModel> GetVideosPreview(
            int page, string searchTerm = null, string actionName = null)
        {
            // Calculate the total number of videos that match
            // (method is used in both Videos & Search controllers)
            var pagesCount = (int)Math.Ceiling(await this.videoService
                                      .GetAll(null, 0, searchTerm)
                                      .CountAsync() / (decimal)VideosPerPage);

            // Get all videos for current page
            var videos = await this.videoService
                    .GetAll(VideosPerPage, (page - 1) * VideosPerPage, searchTerm)
                    .To<VideoPreviewViewModel>().ToArrayAsync();

            // Return view model
            return new VideosPaginationViewModel
            {
                Videos = videos,
                PaginationInfo = this.GetPaginationInfo(page, actionName, pagesCount),
            };
        }

        private PaginationViewModel GetPaginationInfo(int page, string actionName, int pagesCount)
        {
            return new PaginationViewModel
            {
                CurrentPage = page,
                PagesCount = pagesCount,

                // Below needed to create anchor tag for each page
                // (< a href="...controller/action(method)?searchTerm=&page=...")
                ControllerName = this.RouteData.Values["controller"].ToString(),
                ActionName = actionName,
            };
        }
    }
}
