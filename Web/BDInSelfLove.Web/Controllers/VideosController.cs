namespace BDInSelfLove.Web.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewComponents.Models.Videos;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class VideosController : BaseController
    {
        private readonly IVideoService videoService;

        public VideosController(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new AllHomeVideosViewModel
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
