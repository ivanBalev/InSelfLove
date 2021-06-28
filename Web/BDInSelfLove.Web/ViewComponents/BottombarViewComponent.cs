using BDInSelfLove.Services.Data;
using BDInSelfLove.Services.Data.Video;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Web.ViewComponents.Models.Sidebar;
using BDInSelfLove.Web.ViewModels.Home;
using BDInSelfLove.Web.ViewModels.Video;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.ViewComponents
{
    public class BottombarViewComponent : ViewComponent
    {
        private const int SidebarItemsCount = 3;

        private readonly IVideoService videoService;
        private readonly IArticleService articleService;

        public BottombarViewComponent(IVideoService videoService, IArticleService articleService)
        {
            this.videoService = videoService;
            this.articleService = articleService;
        }

        public async Task<IViewComponentResult> InvokeAsync(bool isArticle)
        {
            var viewModel = new SuggestedViewModel();

            if (isArticle)
            {
                // Suggest more videos only for articles page.
                viewModel.Videos = (await this.videoService
               .GetSideVideos(SidebarItemsCount, 0)
               .ToListAsync())
               .Select(a => AutoMapperConfig.MapperInstance.Map<VideoPreviewViewModel>(a))
               .ToList();
            }
            else
            {
                // Suggest more articles only for videos page.
                viewModel.Articles = (await this.articleService
               .GetSideArticles(SidebarItemsCount, 0)
               .ToListAsync())
               .Select(a => AutoMapperConfig.MapperInstance.Map<ArticlePreviewViewModel>(a))
               .ToList();
            }

            return this.View(viewModel);
        }
    }
}
