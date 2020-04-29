﻿namespace BDInSelfLove.Web.ViewModels.Forum.Profile
{
    using System;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using Ganss.XSS;

    public class ProfileCommentViewModel : IMapFrom<CommentServiceModel>
    {
        public int? ParentPostId { get; set; }

        public string ParentPostTitle { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public DateTime CreatedOn { get; set; }
    }
}
