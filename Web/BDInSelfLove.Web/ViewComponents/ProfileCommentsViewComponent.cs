using BDInSelfLove.Services.Data.Comment;
using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Web.ViewModels.Forum.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.ViewComponents
{
    public class ProfileCommentsViewComponent : ViewComponent
    {
        private const int DefaultProfileCommentsCount = 5;

        private readonly ICommentService commentService;

        public ProfileCommentsViewComponent(ICommentService commentService)
        {
            this.commentService = commentService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string userId)
        {
            var viewModel = await this.commentService
                .GetAllByUserId(userId, DefaultProfileCommentsCount)
                .To<ProfileCommentViewModel>()
                .ToListAsync();

            return this.View(viewModel);
        }
    }
}
