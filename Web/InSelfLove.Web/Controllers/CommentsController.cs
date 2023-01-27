namespace InSelfLove.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Data.Comments;
    using InSelfLove.Services.Data.Helpers;
    using InSelfLove.Services.Mapping;
    using InSelfLove.Services.Messaging;
    using InSelfLove.Web.InputModels.Comment;
    using InSelfLove.Web.ViewModels.Comment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;

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
            // Validate input
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            // Create
            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(inputModel);
            await this.commentService.Create(comment, this.userManager.GetUserId(this.User));

            // Inform admin
            await this.NotifyAdminViaEmail(inputModel);

            // Create comment view model
            var commentViewModel = AutoMapperConfig.MapperInstance.Map<Comment, CommentViewModel>(comment);

            // Adjust CreatedOn to user's local time
            commentViewModel.CreatedOn = TimezoneHelper.ToLocalTime(
                commentViewModel.CreatedOn, this.UserTimezoneIdFromCookie);

            // Return single comment html to client
            return this.View("_CommentSinglePartial", commentViewModel);
        }

        [HttpPut]
        [Authorize]
        [Route("api/EditComment")]
        public async Task<ActionResult> Edit(CommentEditInputModel inputModel)
        {
            // Validate input
            if (!this.ModelState.IsValid)
            {
                return this.BadRequest();
            }

            // Prepare data for service
            var comment = AutoMapperConfig.MapperInstance.Map<Comment>(inputModel);
            var userId = this.userManager.GetUserId(this.User);

            // Return respective result to client
            var result = await this.commentService.Edit(comment, userId);
            return result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest();
        }

        [HttpDelete]
        [Authorize]
        [Route("api/DeleteComment")]
        public async Task<ActionResult> Delete(int id)
        {
            // Provide service with comment id, current user id & whether current user is admin
            var result = await this.commentService.Delete(
                         id,
                         this.userManager.GetUserId(this.User),
                         this.User.IsInRole(AppConstants.AdministratorRoleName));

            // Return respective result to client
            return (result > 0 ? (ActionResult)this.Ok() : (ActionResult)this.BadRequest());
        }

        private async Task NotifyAdminViaEmail(CommentInputModel inputModel)
        {
            string userName = this.userManager.GetUserName(this.User);

            // Compose email body
                               // ResourceUrl given by addComment.js
            string emailBody = $@"<div>Имате нов коментар</div></br><br /><br />
                                  <div>Потребител:<br /><b>{userName}</b></div><br />
                                  <div>Коментар:<br /><b>{inputModel.Content}</b></div><br />
                                  <div>
                                  <a href=""{inputModel.ResourceUrl}"">Връзка към ресурс</a>
                                  </div>";

            string emailSubject = "InSelfLove Нов Коментар";
            string adminEmail = (await this.userManager.GetUsersInRoleAsync(
                               AppConstants.AdministratorRoleName)).FirstOrDefault().Email;

            // Compose email
            await this.emailSender.SendEmailAsync(
                from: adminEmail,
                fromName: AppConstants.SystemName,
                to: adminEmail,
                subject: emailSubject,
                htmlContent: emailBody);
        }
    }
}
