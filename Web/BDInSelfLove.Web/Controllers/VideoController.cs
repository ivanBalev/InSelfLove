namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewComponents.Models.Video;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class VideoController : BaseController
    {
        private const int VideosPerPage = 3;

        private readonly IVideoService videoService;

        public VideoController(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        //public async Task<IActionResult> All()
        //{
        //    var viewModel = new AllHomeVideoViewModel
        //    {
        //        Videos = await this.videoService
        //      .GetAll()
        //      .To<HomeVideoViewModel>()
        //      .ToListAsync(),
        //    };

        //    return this.View(viewModel);
        //}

        public async Task<IActionResult> All(int page = 1)
        {
            var serviceModel = await this.videoService
                .GetAllPagination(VideosPerPage, (page - 1) * VideosPerPage);

            var pagesCount = (int)Math.Ceiling(this.videoService.GetAll().Count() / (decimal)VideosPerPage);
            var viewModel = new VideoPaginationViewModel()
            {
                Videos = serviceModel.Select(a => AutoMapperConfig.MapperInstance.Map<VideoViewModel>(a))
                .ToList(),
                PagesCount = pagesCount == 0 ? 1 : pagesCount,
                CurrentPage = page,
            };

            return this.View(viewModel);
        }
    }
}
