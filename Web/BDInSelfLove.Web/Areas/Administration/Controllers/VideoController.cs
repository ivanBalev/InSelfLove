namespace BDInSelfLove.Web.Areas.Administration.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Video;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Videos;
    using BDInSelfLove.Web.Infrastructure.Filters.ActionFilters;
    using BDInSelfLove.Web.ViewModels.Administration.Video;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

    public class VideoController : AdministrationController
    {
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
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<VideoServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            var videoId = await this.videoService.CreateAsync(serviceModel);

           // TODO: Exception handling
            return this.RedirectToAction("Index", "Video", new { area = string.Empty });
        }
    }
}
