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
        public int Id { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public string UserProfilePhoto { get; set; }

        public DateTime CreatedOn { get; set; }

        public string Content { get; set; }

        // TODO: remove sanitation from all view models. it's now done in the service model
        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public IList<PostCommentViewModel> SubComments { get; set; }
    }
}
