namespace BDInSelfLove.Web.Areas.Administration.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Videos;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.InputModels.Administration.Video;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class VideoController : AdministrationController
    {
        private const string VideoCreateError = "Error creating video. Please try again.";

        private readonly UserManager<ApplicationUser> userManager;
        private readonly IVideoService videoService;

        public VideoController(UserManager<ApplicationUser> userManager, IVideoService videoService)
        {
            this.userManager = userManager;
            this.videoService = videoService;
        }

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
