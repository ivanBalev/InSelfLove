namespace InSelfLove.Web.Controllers
{
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Data.Videos;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Web.Controllers.Helpers;
    using InSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using InSelfLove.Web.InputModels.Video;
    using InSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class VideosController : PaginationHelper
    {
        private readonly IVideoService videoService;

        public VideosController(IVideoService videoService)
            : base(videoService)
        {
            this.videoService = videoService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            return this.View(await this.GetVideosPreview(page));
        }

        [Route("Videos/{slug}")]
        public async Task<IActionResult> Single(string slug)
        {
            // Get info for client & create view model
            var viewModel = AutoMapperConfig.MapperInstance.Map<VideoViewModel>(
                await this.videoService.GetBySlug(slug, this.UserTimezoneIdFromCookie));

            // Return 404 if video doesn't exist
            if (viewModel == null)
            {
                return this.NotFound();
            }

            return this.View(viewModel);
        }

        // Admin access only below
        [HttpGet]
        [Route("Videos/Create")]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public IActionResult Create()
        {
            return this.View();
        }

        [HttpPost]
        [Route("Videos/Create")]
        [YoutubeLinkActionFilter]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Create(CreateVideoInputModel inputModel)
        {
            string slug = await this.videoService
                .Create(AutoMapperConfig.MapperInstance.Map<Video>(inputModel));

            return this.RedirectToAction("Single", new { slug });
        }

        [HttpGet]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Edit(int id)
        {
            // Get video & map to view/input model
            var model = await this.videoService.GetById(id)
                .To<EditVideoInputModel>().FirstOrDefaultAsync();

            return this.View(model);
        }

        [HttpPost]
        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Edit(EditVideoInputModel inputModel)
        {
            string slug = await this.videoService
                .Edit(AutoMapperConfig.MapperInstance.Map<Video>(inputModel));

            return this.RedirectToAction("Single", new { slug });
        }

        [Authorize(Roles = AppConstants.AdministratorRoleName)]
        public async Task<IActionResult> Delete(int id)
        {
            await this.videoService.Delete(id);
            return this.Redirect("/");
        }
    }
}
