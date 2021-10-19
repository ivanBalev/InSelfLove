namespace BDInSelfLove.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.CommentService;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.InputModels.Comment;
    using BDInSelfLove.Web.ViewModels.Comment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using TimeZoneConverter;

    public class CommentsController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ICommentService commentService;

        public CommentsController(
            ICommentService commentService,
            UserManager<ApplicationUser> userManager)
        {
            this.commentService = commentService;
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        [Route("api/CreateComment")]
        public async Task<IActionResult> Create([FromBody] CommentInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            // Create comment
            var dataForDb = AutoMapperConfig.MapperInstance.Map<Comment>(inputModel);
            dataForDb.UserId = this.userManager.GetUserId(this.User);
            var commentId = await this.commentService.Create(dataForDb);

            // Create comment view model
            var commentViewModel = await this.commentService.GetById(commentId)
                .To<CommentViewModel>().FirstOrDefaultAsync();
            commentViewModel.CreatedOn = TimezoneHelper
                .ToLocalTime(commentViewModel.CreatedOn, this.TimezoneCookieValue);

            // TODO: Send email to admin when new comment added
            return this.View("_CommentSinglePartial", commentViewModel);
        }

        [HttpPost]
        [Authorize]
        [Route("api/EditComment")]
        public async Task<ActionResult> Edit(CommentEditInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            // Check if creator is same as editor
            var dbCommentUserId = await this.commentService.GetById(inputModel.Id)
                .Select(c => c.UserId).SingleOrDefaultAsync();

            if (dbCommentUserId != this.userManager.GetUserId(this.User))
            {
                return this.BadRequest();
            }

            var serviceModel = AutoMapperConfig.MapperInstance.Map<Comment>(inputModel);

            // Send data to service
            var result = await this.commentService.Edit(serviceModel);

            return result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest();
        }

        [HttpPost]
        [Authorize]
        [Route("api/DeleteComment")]
        public async Task<ActionResult> Delete(int id)
        {
            if (id < 1)
            {
                return this.BadRequest();
            }

            // Check if creator is same as editor
            var dbCommentUserId = await this.commentService.GetById(id)
                .Select(c => c.UserId).SingleOrDefaultAsync();

            if (dbCommentUserId != this.userManager.GetUserId(this.User) && !this.User.IsInRole(GlobalValues.AdministratorRoleName))
            {
                return this.BadRequest();
            }

            // Send data to service
            var result = await this.commentService.Delete(id);

            return (result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest());
        }
    }
}
