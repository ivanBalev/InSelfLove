namespace BDInSelfLove.Web.ViewComponents
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Data.Models;
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
