namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.ViewComponents.Models.Video;
    using BDInSelfLove.Web.ViewModels.Pagination;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class VideoController : BaseController
    {
        private const int VideosPerPage = 6;

        private readonly IVideoService videoService;

        public VideoController(IVideoService videoService)
        {
            this.videoService = videoService;
        }

        public async Task<IActionResult> Single(int id)
        {
            var viewModel = AutoMapperConfig.MapperInstance
                .Map<VideoViewModel>(await this.videoService
                .GetById(id));

            return this.View(viewModel);
        }

        public async Task<IActionResult> All(int page = 1)
        {
            var serviceModel = await this.videoService
                .GetAllPagination(VideosPerPage, (page - 1) * VideosPerPage);

            var pagesCount = (int)Math.Ceiling(this.videoService.GetAll().Count() / (decimal)VideosPerPage);
            var viewModel = new VideoPaginationViewModel()
            {
                Videos = serviceModel.Select(a => AutoMapperConfig.MapperInstance.Map<VideoPreviewViewModel>(a))
                .ToList(),
                PaginationInfo = new PaginationViewModel
                {
                    ControllerName = this.ControllerContext.RouteData.Values["controller"].ToString(),
                    PagesCount = pagesCount == 0 ? 1 : pagesCount,
                    CurrentPage = page,
                },
            };

            return this.View(viewModel);
        }
    }
}
