namespace BDInSelfLove.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Services.Data.Helpers;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Comments;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Web.InputModels.Comment;
    using BDInSelfLove.Web.ViewModels.Comment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class CommentsController : BaseController
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly ICommentService commentService;

        public CommentsController(
            ICommentService commentService,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender)
        {
            this.commentService = commentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
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

            var commentId = await this.commentService.Create(comment, this.userManager.GetUserId(this.User));

            await this.NotifyAdminViaEmail(inputModel);

            // Create comment view model
            var commentViewModel = await this.commentService.GetById(commentId)
                .To<CommentViewModel>().FirstOrDefaultAsync();
            commentViewModel.CreatedOn = TimezoneHelper
                .ToLocalTime(commentViewModel.CreatedOn, this.UserTimezoneIdFromCookie);

            return this.View("_CommentSinglePartial", commentViewModel);
        }

        [HttpPut]
        [Authorize]
        [Route("api/EditComment")]
        public async Task<ActionResult> Edit(CommentEditInputModel inputModel)
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
            var isUserAdmin = this.User.IsInRole(AppConstants.AdministratorRoleName);
            var result = await this.commentService.Delete(id, userId, isUserAdmin);
            return (result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest());
        }

        private async Task NotifyAdminViaEmail(CommentInputModel inputModel)
        {
            string adminEmail = (await this.userManager.GetUsersInRoleAsync(AppConstants.AdministratorRoleName)).FirstOrDefault().Email;
            string userName = this.userManager.GetUserName(this.User);

            string emailBody = $@"<div>Имате нов коментар</div></br><br /><br />
                                  <div>Потребител:<br /><b>{userName}</b></div><br />
                                  <div>Коментар:<br /><b>{inputModel.Content}</b></div><br />
                                  <div>
                                  <a href=""{inputModel.ResourceUrl}"">Връзка към ресурс</a>
                                  </div>";

            string emailSubject = "InSelfLove Нов Коментар";
            await this.emailSender.SendEmailAsync(
                from: adminEmail,
                fromName: AppConstants.SystemName,
                to: adminEmail,
                subject: emailSubject,
                htmlContent: emailBody);
        }
    }
}
