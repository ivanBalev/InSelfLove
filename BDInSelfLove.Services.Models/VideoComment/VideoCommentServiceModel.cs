using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.User;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Services.Models.VideoComment
{
    public class VideoCommentServiceModel : IMapFrom<Data.Models.VideoComment>, IMapTo<Data.Models.VideoComment>
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUserServiceModel User { get; set; }

        public int VideoId { get; set; }

        public int? ParentCommentId { get; set; }

        public DateTime CreatedOn { get; set; }

        public virtual VideoCommentServiceModel ParentComment { get; set; }

        public virtual IList<VideoCommentServiceModel> SubComments { get; set; }
    }
}
