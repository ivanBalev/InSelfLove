using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Data.VideoComment;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.VideoComment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BDInSelfLove.Web.InputModels.VideoComment;

namespace BDInSelfLove.Web.Controllers
{
    public class VideoCommentController : BaseController
    {
        private readonly IVideoCommentService videoCommentService;
        private readonly UserManager<ApplicationUser> userManager;

        public VideoCommentController(IVideoCommentService articleCommentService, UserManager<ApplicationUser> userManager)
        {
            this.videoCommentService = articleCommentService;
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(VideoCommentInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = "Error";
                return this.RedirectToAction("All", "Video");
            }

            if (!inputModel.Content.StartsWith('<'))
            {
                inputModel.Content = $"<p>{inputModel.Content}</p>";
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<VideoCommentServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            var commentId = await this.videoCommentService.Create(serviceModel);

            if (commentId == 0)
            {
                this.TempData["Error"] = "Error";
                return this.RedirectToAction("All", "Video");
            }

            // TODO: Send email to admin when new comment added
            return this.RedirectToAction("All", "Video");
        }
    }
}
