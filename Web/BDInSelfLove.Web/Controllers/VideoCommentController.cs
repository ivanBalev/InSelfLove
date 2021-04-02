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
using Microsoft.EntityFrameworkCore;
using BDInSelfLove.Common;

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
                return this.RedirectToAction("All", "Video");
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

        [HttpPost]
        [Authorize]
        [Route("api/EditVideoComment")]
        public async Task<ActionResult> Edit(VideoCommentEditInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            // Check if creator is same as editor
            var dbCommentUserId = await this.videoCommentService.GetById(inputModel.Id)
                .Select(c => c.UserId).SingleOrDefaultAsync();

            if (dbCommentUserId != this.userManager.GetUserId(this.User))
            {
                return this.BadRequest();
            }

            var serviceModel = AutoMapperConfig.MapperInstance.Map<VideoCommentServiceModel>(inputModel);

            // Send data to service
            var result = await this.videoCommentService.Edit(serviceModel);

            return (result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest());
        }

        [HttpPost]
        [Authorize]
        [Route("api/DeleteVideoComment")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return this.BadRequest();
            }

            // Check if creator is same as editor
            var dbCommentUserId = await this.videoCommentService.GetById(id)
                .Select(c => c.UserId).SingleOrDefaultAsync();

            if (dbCommentUserId != this.userManager.GetUserId(this.User) && !this.User.IsInRole(GlobalConstants.AdministratorRoleName))
            {
                return this.BadRequest();
            }

            // Send data to service
            var result = await this.videoCommentService.Delete(id);

            return (result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest());
        }
    }
}
