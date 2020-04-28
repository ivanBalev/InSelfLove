namespace BDInSelfLove.Web.Areas.Forum.Controllers
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using BDInSelfLove.Common;
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Data.Comment;
    using BDInSelfLove.Services.Data.User;
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
        private const string BanErrorMessage = "You have been banned from submitting comments. Default ban length is 3 days.";
        private const string InvalidReportAssessmentMessage = "Invalid assessment. Please try again.";
        private const string CommentCreateError = "An error occurred while creating your comment. Please try again.";
        private const string ReportCreateError = "An error occurred while creating your report. Please try again.";

        private readonly ICommentService commentService;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IEmailSender emailSender;
        private readonly IUserService userService;

        public CommentController(ICommentService commentService, UserManager<ApplicationUser> userManager, IEmailSender emailSender, IUserService userService)
        {
            this.commentService = commentService;
            this.userManager = userManager;
            this.emailSender = emailSender;
            this.userService = userService;
        }

        [HttpPost]
        [Authorize]

        public async Task<IActionResult> Create(CommentCreateInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = CommentCreateError;
                return this.RedirectToAction("Index", "Post", new { id = inputModel.ParentPostId });
            }

            var user = await this.userManager.GetUserAsync(this.User);

            if (user.IsBanned)
            {
                var doesBanNeedToBeLifted = await this.userService.CheckIfBanNeedsToBeLifted(user.Id);

                if (!doesBanNeedToBeLifted)
                {
                    this.TempData["Error"] = BanErrorMessage;
                    return this.RedirectToAction("Index", "Post", new { id = inputModel.ParentPostId });
                }

                await this.commentService.ClearUserReports(user.Id);
            }

            var serviceModel = AutoMapperConfig.MapperInstance.Map<CommentServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            var commentId = await this.commentService.Create(serviceModel);

            if (commentId == 0)
            {
                this.TempData["Error"] = CommentCreateError;
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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Report(ReportCommentViewModel viewModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = ReportCreateError;
                return this.View(viewModel);
            }

            var comment = await this.commentService.GetById(viewModel.Id).To<ReportCommentInputModel>().FirstOrDefaultAsync();
            var offendingUser = await this.userManager.FindByIdAsync(comment.UserId);
            var reportSubmitter = await this.userManager.GetUserAsync(this.User);

            var report = new ReportServiceModel
            {
                Reason = viewModel.Reason,
                CommentId = comment.Id,
                SubmitterId = reportSubmitter.Id,
                OffenderId = offendingUser.Id,
            };

            var reportId = await this.commentService.SubmitReport(report);

            await this.emailSender.SendEmailAsync(
                offendingUser.Email,
                offendingUser.UserName,
                GlobalConstants.SystemEmail,
                GlobalConstants.SystemName + " " + GlobalConstants.ReportEmailSubject,
                @$"{GlobalConstants.ReportEmailSubject} by <strong>{reportSubmitter.UserName}</strong>
                                                        against <strong>{offendingUser.UserName}</strong>'s comment <br>
                                                        <strong>Comment text:</strong> <br>
                                                        {Regex.Replace(comment.Content, @"<[^>]+>", string.Empty)} <br>
                                                        Click on the link below to assess this report: <br>
                                                        {GlobalConstants.SystemAddress}{ReportBaseAddress}{reportId}");


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
        public async Task<IActionResult> AssessReport(AssessCommentReportInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = InvalidReportAssessmentMessage;
                return this.View(inputModel);
            }

            await this.commentService.AddReportAssessment(inputModel.ReportId, inputModel.AssessmentValue, inputModel.OffenderId);

            return this.RedirectToAction("Index", "Post", new { id = inputModel.ParentPostId });
        }
    }
}
