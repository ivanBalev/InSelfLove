using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using BDInSelfLove.Services.Models.Post;
using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Forum.Post
{
    public class PostCommentViewModel : IMapFrom<CommentServiceModel>
    {
        public string UserUserName { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        //public ICollection<PostCommentViewModel> Comments { get; set; }
    }
}
