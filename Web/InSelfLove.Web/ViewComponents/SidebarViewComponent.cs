namespace InSelfLove.Web.ViewComponents
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Articles;
    using InSelfLove.Services.Data.Videos;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Web.ViewComponents.Models.Sidebar;
    using InSelfLove.Web.ViewModels.Home;
    using InSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Mvc;

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

        public async Task<IViewComponentResult> InvokeAsync(bool isArticle, DateTime date)
        {
            var viewModel = new SuggestedViewModel();

            if (!isArticle)
            {
                // TODO: HORRID
                // Suggest more videos only for videos page.
                viewModel.Videos = (await this.videoService
               .GetSideVideos(SidebarItemsCount, date))
               .Select(v => AutoMapperConfig.MapperInstance.Map<Video, VideoPreviewViewModel>(v))
               .ToList();
            }
            else
            {
                // TODO: HORRID
                // Suggest more articles only for articles page.
                viewModel.Articles = (await this.articleService
               .GetSideArticles(SidebarItemsCount, date))
               .Select(a => AutoMapperConfig.MapperInstance.Map<Article, ArticlePreviewViewModel>(a))
               .ToList();
            }

            return this.View(viewModel);
        }
    }
}
