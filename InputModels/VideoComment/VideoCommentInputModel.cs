using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.VideoComment;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.InputModels.VideoComment
{
    public class VideoCommentInputModel : IMapTo<VideoCommentServiceModel>
    {
        public string Content { get; set; }

        public int VideoId { get; set; }

        public int? ParentCommentId { get; set; }
    }
}
