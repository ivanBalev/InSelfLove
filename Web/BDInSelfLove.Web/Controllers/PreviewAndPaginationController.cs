namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Articles;
    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewModels.Article;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Pagination;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.EntityFrameworkCore;

    public class PreviewAndPaginationController : BaseController
    {
        private const int ArticlesPerPage = 6;
        private const int VideosPerPage = 6;

        private IArticleService articleService;
        private IVideoService videoService;

        public PreviewAndPaginationController(
            IArticleService articleService,
            IVideoService videoService)
        {
            this.articleService = articleService;
            this.videoService = videoService;
        }

        public PreviewAndPaginationController(IArticleService articleService)
        {
            this.articleService = articleService;
        }

        public PreviewAndPaginationController(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        protected IArticleService ArticleService => this.articleService;

        protected IVideoService VideoService => this.videoService;

        protected async Task<ArticlesPaginationViewModel> GetArticlesPreviewAndPagination(int page, string searchTerm = null, string actionName = null)
        {
            var articlePagesCount = (int)Math.Ceiling(await this.articleService.GetAll(null, 0, searchTerm).CountAsync() / (decimal)ArticlesPerPage);

            return new ArticlesPaginationViewModel
            {
                Articles = await this.ArticleService.GetAll(ArticlesPerPage, (page - 1) * ArticlesPerPage, searchTerm)
                    .To<ArticlePreviewViewModel>().ToArrayAsync(),
                PaginationInfo = new PaginationViewModel
                {
                    CurrentPage = page,
                    PagesCount = articlePagesCount,
                    ActionName = actionName,
                },
            };
        }

        protected async Task<VideosPaginationViewModel> GetVideosPreviewAndPagination(int page, string searchTerm = null, string actionName = null)
        {
            var videoPagesCount = (int)Math.Ceiling(await this.videoService.GetAll(null, 0, searchTerm).CountAsync() / (decimal)VideosPerPage);

            return new VideosPaginationViewModel
            {
                Videos = await this.videoService.GetAll(VideosPerPage, (page - 1) * VideosPerPage, searchTerm)
                    .To<VideoPreviewViewModel>().ToArrayAsync(),
                PaginationInfo = new PaginationViewModel
                {
                    CurrentPage = page,
                    PagesCount = videoPagesCount,
                    ActionName = actionName,
                },
            };
        }
    }
}
