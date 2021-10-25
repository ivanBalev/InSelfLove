namespace BDInSelfLove.Web.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Web.ViewModels.Search;
    using Microsoft.AspNetCore.Mvc;

    public class SearchController : PreviewAndPaginationHelper
    {
        private const string ArticleActionName = "Article";
        private const string VideoActionName = "Video";

        public SearchController(
            IArticleService articleService,
            IVideoService videoService)
            : base(articleService, videoService)
        {
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            var viewModel = new IndexSearchViewModel
            {
                SearchTerm = searchTerm,
                ArticlesPagination = await this.GetArticlesPreviewAndPagination(1, searchTerm, ArticleActionName),
                VideosPagination = await this.GetVideosPreviewAndPagination(1, searchTerm, VideoActionName),
            };
            return this.View(viewModel);
        }

        [Route("api/Search/Article")]
        public async Task<IActionResult> Article(int page, string searchTerm)
        {
            var viewModel = await this.GetArticlesPreviewAndPagination(page, searchTerm, ArticleActionName);
            return this.View("_ArticlesAllPartial", viewModel);
        }

        [Route("api/Search/Video")]
        public async Task<IActionResult> Video(int page, string searchTerm)
        {
            var viewModel = await this.GetVideosPreviewAndPagination(page, searchTerm, VideoActionName);
            return this.View("_VideosAllPartial", viewModel);
        }
    }
}
