﻿namespace BDInSelfLove.Web.ViewModels.Forum.Post
{
    using System;
    using System.Collections.Generic;

    using BDInSelfLove.Services.Mapping;
    using BDInSelfLove.Services.Models.Comment;
    using Ganss.XSS;

    public class PostCommentViewModel : IMapFrom<CommentServiceModel>
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public string UserProfilePhoto { get; set; }

        public DateTime UserCreatedOn { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public IList<PostCommentViewModel> SubComments { get; set; }
    }
}
