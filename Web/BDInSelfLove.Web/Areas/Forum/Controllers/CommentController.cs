namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Messaging;
    using BDInSelfLove.Services.Models.Comment;
    using BDInSelfLove.Web.InputModels.Forum.Comment;
    using BDInSelfLove.Web.ViewModels.Forum.Comment;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class CommentController : BaseForumController
    {
        private const string ReportBaseAddress = "Forum/Comment/AssessReport/";

        private readonly ICommentService commentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;

        public CommentController(ICommentService commentService, UserManager<ApplicationUser> userManager, IEmailSender emailSender)
        {
            this.commentService = commentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
        }
        // TODO: MAKE SURE ALL ASYNC SERVICE METHODS ARE AWAITED REGARDLESS OF WHETHER THEY RETURN AN OBJECT OR IQUERYABLE!!!!


        [HttpPost]
        [Authorize]

        public async Task<IActionResult> Create(CommentCreateInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(inputModel);
            }

            var user = await this.userManager.GetUserAsync(this.User);

            var serviceModel = AutoMapperConfig.MapperInstance.Map<CommentServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            var commentId = await this.commentService.Create(serviceModel);

            if (commentId == 0)
            {
                this.TempData["Error"] = "Error";

                return this.View(inputModel);
            }

            return this.RedirectToAction("Index", "Post", new { id = inputModel.ParentPostId });
        }

        [Authorize]
        public async Task<IActionResult> Report(int id)
        {
            var comment = await this.commentService.GetById(id).To<ReportCommentViewModel>().FirstOrDefaultAsync();
            return this.View(comment);
        }

        // TODO: MAKE SURE ALL METHODS HAVE THE CORRECT AUTHORIZATION. ALL EVERYWHERE!

        // TODO: CREATE REPORT APPROVAL BY ADMIN FUNCTIONALITY
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Report(ReportCommentViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(viewModel);
            }

            // TODO: could not get automapper to get parentpost.title. works with comment entity and include in service model but here it doesn't. wasted 3 days.
            var comment = await this.commentService.GetById(viewModel.Id).To<ReportCommentInputModel>().FirstOrDefaultAsync();
            var offendingUser = await this.userManager.FindByIdAsync(comment.UserId);
            var reportSubmitter = await this.userManager.GetUserAsync(this.User);

            var report = new ReportServiceModel
            {
                Reason = viewModel.Reason,
                CommentId = comment.Id,
                SubmitterId = reportSubmitter.Id,
            };

            var reportId = await this.commentService.SubmitReport(report);

            await this.emailSender.SendEmailAsync(
                                            offendingUser.Email,
                                            offendingUser.UserName,
                                            GlobalConstants.SystemEmail,
                                            GlobalConstants.SystemName + " " + GlobalConstants.ReportEmailSubject,
                                            @$"{GlobalConstants.ReportEmailSubject} by {reportSubmitter.UserName} against {offendingUser.UserName}'s comment{Environment.NewLine}
                                            Comment text: {Environment.NewLine}{comment.Content} /n {GlobalConstants.SystemAddress}{ReportBaseAddress}{reportId}");


            return this.RedirectToAction("Index", "Post", new { id = comment.ParentPostId });
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        public async Task<IActionResult> AssessReport(int id)
        {
            var comment = await this.commentService.GetCommentWithReport(id)
                .To<AssessCommentViewModel>()
                .FirstOrDefaultAsync();

            return this.View(comment);
        }

        [Authorize(Roles = GlobalConstants.AdministratorRoleName)]
        [HttpPost]
        public async Task<IActionResult> AssessReport(AssessCommentReportinputModel inputModel)
        {
            // TODO: Why checking modelstate when there are no validation attributes triggers an internal server error asking for migration updates?
            if (!this.ModelState.IsValid)
            {
                return this.RedirectToAction("Index", "Post", new { id = inputModel.ParentPostId });
            }

            await this.commentService.AddReportAssessment(inputModel.ReportId, inputModel.AssessmentValue);

            return this.RedirectToAction("Index", "Post", new { id = inputModel.ParentPostId });
        }
    }
}
