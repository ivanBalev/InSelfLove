using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    public class ReportCommentSubmitModel : IMapFrom<CommentServiceModel>
    {
        public int Id { get; set; }

        public int ParentPostId { get; set; }

        public string UserId { get; set; }

        public string Content { get; set; }
    }
}
