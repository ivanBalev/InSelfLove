namespace BDInSelfLove.Web.ViewComponents
{
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewComponents.Models.Videos;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class HomeVideosViewComponent : ViewComponent
    {
        private const int HomeVideosCount = 3;

        private readonly IVideoService videoService;

        public HomeVideosViewComponent(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var viewModel = new AllHomeVideosViewModel
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
