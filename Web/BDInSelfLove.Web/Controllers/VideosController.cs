namespace BDInSelfLove.Web.Controllers
{
    using System.Threading.Tasks;
    using System.Web;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Helpers;
    using BDInSelfLove.Services.Data.Videos;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.Controllers.Helpers;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.InputModels.Video;
    using BDInSelfLove.Web.ViewModels.Video;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class VideosController : PaginationHelper
    {
        private readonly IVideoService videoService;

        public VideosController(
            IVideoService videoService)
            : base(videoService)
        {
            this.videoService = videoService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var viewModel = await this.GetVideosPreview(page);

            return this.View(viewModel);
        }

        [Route("Videos/{slug}")]
        public async Task<IActionResult> Single(string slug)
        {
            slug = HttpUtility.UrlDecode(slug);
            var viewModel = AutoMapperConfig.MapperInstance
                .Map<VideoViewModel>(await this.videoService
                .GetBySlug(slug));

            if (viewModel == null)
            {
                return this.NotFound();
            }

            for (int i = 0; i < viewModel?.Comments.Count; i++)
            {
                viewModel.Comments[i].CreatedOn = TimezoneHelper.ToLocalTime(
                    viewModel.Comments[i].CreatedOn, this.UserTimezoneIdFromCookie);
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
