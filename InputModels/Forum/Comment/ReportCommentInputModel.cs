using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Forum.Comment
{
    public class ReportCommentInputModel : IMapFrom<CommentServiceModel>
    {
        public int Id { get; set; }

        public int ParentPostId { get; set; }

        public string UserId { get; set; }

        public string Content { get; set; }
    }
}
