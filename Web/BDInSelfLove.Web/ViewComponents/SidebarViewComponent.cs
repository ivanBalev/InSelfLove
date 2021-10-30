namespace BDInSelfLove.Web.ViewComponents
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Articles;
    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewComponents.Models.Sidebar;
    using BDInSelfLove.Web.ViewModels.Home;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class SidebarViewComponent : ViewComponent
    {
        private const int SidebarItemsCount = 3;

        private readonly IVideoService videoService;
        private readonly IArticleService articleService;

        public SidebarViewComponent(IVideoService videoService, IArticleService articleService)
        {
            this.videoService = videoService;
            this.articleService = articleService;
        }

        public async Task<IViewComponentResult> InvokeAsync(int articleId = 0, int videoId = 0)
        {
            var viewModel = new SuggestedViewModel();

            if (videoId != 0)
            {
                // Suggest more videos only for videos page.
                viewModel.Videos = await this.videoService
               .GetSideVideos(SidebarItemsCount, videoId)
               .To<VideoPreviewViewModel>()
               .ToListAsync();
            }
            else
            {
                // Suggest more articles only for articles page.
                viewModel.Articles = await this.articleService
               .GetSideArticles(SidebarItemsCount, articleId)
               .To<ArticlePreviewViewModel>()
               .ToListAsync();
            }

            return this.View(viewModel);
        }
    }
}
