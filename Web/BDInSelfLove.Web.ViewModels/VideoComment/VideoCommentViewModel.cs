using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.VideoComment;
using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.VideoComment
{
    public class VideoCommentViewModel : IMapFrom<VideoCommentServiceModel>
    {
        public int Id { get; set; }

        public int VideoId { get; set; }

        public string Content { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserId { get; set; }

        public string UserUserName { get; set; }

        public string UserProfilePhoto { get; set; }

        public string SanitizedContent => new HtmlSanitizer().Sanitize(this.Content);

        public int? ParentCommentId { get; set; }

        public IList<VideoCommentViewModel> SubComments { get; set; }
    }
}
