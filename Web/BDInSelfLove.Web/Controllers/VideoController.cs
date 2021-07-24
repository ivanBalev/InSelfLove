namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Videos;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.InputModels.Video;
    using BDInSelfLove.Web.ViewModels.Pagination;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class VideoController : BaseController
    {
        private const int VideosPerPage = 6;
        private const string VideoCreateError = "Error creating video. Please try again.";

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IVideoService videoService;

        public VideoController(IVideoService videoService, UserManager<ApplicationUser> userManager)
        {
            this.videoService = videoService;
            this.userManager = userManager;
        }

        public async Task<IActionResult> Single(int id)
        {
            // TODO: convert comment times to user local IF COOKIE EXISTS
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

        // Admin access only below
        [Authorize(Roles = GlobalValues.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [YoutubeLinkActionFilter]
        public async Task<IActionResult> Create(CreateVideoInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.ViewData["Error"] = VideoCreateError;
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<VideoServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            await this.videoService.CreateAsync(serviceModel);

            return this.RedirectToAction("All", "Video", new { area = string.Empty });
        }

        public async Task<IActionResult> Delete(int id)
        {
            await this.videoService.Delete(id);

            return this.Redirect("/");
        }
    }
}
