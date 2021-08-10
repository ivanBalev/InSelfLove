using BDInSelfLove.Common;
using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Data.CommentService;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using BDInSelfLove.Web.InputModels.Comment;
using BDInSelfLove.Web.ViewModels.Comment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Controllers
{
    public class CommentsController : BaseController
    {
        private readonly ICommentService commentService;
        private readonly UserManager<ApplicationUser> userManager;

        public CommentsController(ICommentService commentService, UserManager<ApplicationUser> userManager)
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

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<CommentServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            int commentId = await this.commentService.Create(serviceModel);
            CommentViewModel commentViewModel = AutoMapperConfig.MapperInstance.Map<CommentViewModel>
                (await this.commentService.GetById(commentId).FirstOrDefaultAsync());

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

            var serviceModel = AutoMapperConfig.MapperInstance.Map<CommentServiceModel>(inputModel);

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
