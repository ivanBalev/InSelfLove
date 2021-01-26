using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Data.ArticleComment;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.ArticleComment;
using BDInSelfLove.Web.InputModels.ArticleComment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.Controllers
{
    public class ArticleCommentController : BaseController
    {
        private readonly IArticleCommentService articleCommentService;
        private readonly UserManager<ApplicationUser> userManager;

        public ArticleCommentController(IArticleCommentService articleCommentService, UserManager<ApplicationUser> userManager)
        {
            this.articleCommentService = articleCommentService;
            this.userManager = userManager;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(ArticleCommentInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = "Error";
                return this.RedirectToAction("Single", "Article", new { id = inputModel.ArticleId });
            }

            var user = await this.userManager.GetUserAsync(this.User);
            var serviceModel = AutoMapperConfig.MapperInstance.Map<ArticleCommentServiceModel>(inputModel);
            serviceModel.UserId = user.Id;

            var commentId = await this.articleCommentService.Create(serviceModel);

            if (commentId == 0)
            {
                this.TempData["Error"] = "Error";
                return this.RedirectToAction("Single", "Article", new { id = inputModel.ArticleId });
            }

            // TODO: Send email to admin when new comment added
            return this.RedirectToAction("Single", "Article", new { id = inputModel.ArticleId });
        }

        [HttpPost]
        [Authorize]
        [Route("api/EditComment")]
        public async Task<IActionResult> Edit(ArticleCommentEditInputModel inputModel)
        {
            if (!this.ModelState.IsValid)
            {
                this.TempData["Error"] = "Error";
                return this.RedirectToAction("Single", "Article", new { id = inputModel.ArticleId });
            }

            // Check if creator is same as editor
            var dbCommentUserId = await this.articleCommentService.GetById(inputModel.Id)
                .Select(c => c.UserId).SingleOrDefaultAsync();

            if (dbCommentUserId != this.userManager.GetUserId(this.User))
            {
                this.TempData["Error"] = "Error";
                return this.RedirectToAction("Single", "Article", new { id = inputModel.ArticleId });
            }

            var serviceModel = AutoMapperConfig.MapperInstance.Map<ArticleCommentServiceModel>(inputModel);

            // Send data to service
            var result = await this.articleCommentService.Edit(serviceModel);

            return (result > 0 ? this.Ok() : this.BadRequest());
        }
    }
}
