namespace BDInSelfLove.Web.Controllers
{
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Comments;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Web.InputModels.Comment;
    using BDInSelfLove.Web.ViewModels.Comment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

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
            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(inputModel);
            comment.UserId = this.userManager.GetUserId(this.User);
            var commentId = await this.commentService.Create(comment);

            // Create comment view model
            var commentViewModel = await this.commentService.GetById(commentId)
                .To<CommentViewModel>().FirstOrDefaultAsync();
            commentViewModel.CreatedOn = TimezoneHelper
                .ToLocalTime(commentViewModel.CreatedOn, this.TimezoneIdFromCookie);

            return this.View("_CommentSinglePartial", commentViewModel);
        }

        [HttpPut]
        [Authorize]
        [Route("api/EditComment")]
        public async Task<ActionResult> Edit([FromBody] CommentEditInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(inputModel);
            var userId = this.userManager.GetUserId(this.User);

            var result = await this.commentService.Edit(comment, userId);
            return result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest();
        }

        [HttpDelete]
        [Authorize]
        [Route("api/DeleteComment")]
        public async Task<ActionResult> Delete(int id)
        {
            var userId = this.userManager.GetUserId(this.User);
            var isUserAdmin = this.User.IsInRole(GlobalValues.AdministratorRoleName);
            var result = await this.commentService.Delete(id, userId, isUserAdmin);
            return (result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest());
        }
    }
}
