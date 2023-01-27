namespace InSelfLove.Web.Controllers
{
    using System.Threading.Tasks;

    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.Videos;
    using InSelfLove.Web.Controllers.Helpers;
    using InSelfLove.Web.ViewModels.Search;
    using Microsoft.AspNetCore.Mvc;

    public class SearchController : PaginationHelper
    {
        private const string ArticleActionName = "Article";
        private const string VideoActionName = "Video";

        public SearchController(IArticleService articleService, IVideoService videoService)
            : base(articleService, videoService)
        {
        }

        public async Task<IActionResult> Index(string searchTerm)
        {
            // Get view model
            // Action names refer to the 2 methods below -
            // for creating the href links in pagination
            var viewModel = new IndexSearchViewModel
            {
                SearchTerm = searchTerm,
                ArticlesPagination = await this.GetArticlesPreview(1, searchTerm, ArticleActionName),
                VideosPagination = await this.GetVideosPreview(1, searchTerm, VideoActionName),
            };
            return this.View(viewModel);
        }

        // Fetch request
        [Route($"api/Search/{ArticleActionName}")]
        public async Task<IActionResult> Article(int page, string searchTerm)
        {
            var viewModel = await this.GetArticlesPreview(page, searchTerm, ArticleActionName);
            return this.View("_ArticlesAllPartial", viewModel);
        }

        // Fetch request
        [Route($"api/Search/{VideoActionName}")]
        public async Task<IActionResult> Video(int page, string searchTerm)
        {
            var viewModel = await this.GetVideosPreview(page, searchTerm, VideoActionName);
            return this.View("_VideosAllPartial", viewModel);
        }
    }
}
