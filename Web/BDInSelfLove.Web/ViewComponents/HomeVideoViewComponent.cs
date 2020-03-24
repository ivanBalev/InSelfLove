namespace BDInSelfLove.Web.ViewComponents
{
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewComponents.Models.Video;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeVideoViewComponent : ViewComponent
    {
        private const int HomeVideosCount = 3;

        private readonly IVideoService videoService;

        public HomeVideoViewComponent(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new AllHomeVideoViewModel
            {
                Videos = await this.videoService
              .GetAll(HomeVideosCount)
              .To<HomeVideoViewModel>()
              .ToListAsync(),
            };

            return this.View(viewModel);
        }
    }
}
