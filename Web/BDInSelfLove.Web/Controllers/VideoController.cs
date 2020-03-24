namespace BDInSelfLove.Web.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewComponents.Models.Video;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class VideoController : BaseController
    {
        private readonly IVideoService videoService;

        public VideoController(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        public async Task<IActionResult> All()
        {
            var viewModel = new AllHomeVideoViewModel
            {
                Videos = await this.videoService
              .GetAll()
              .To<HomeVideoViewModel>()
              .ToListAsync(),
            };

            return this.View(viewModel);
        }
    }
}
