using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Data.Models
{
    public class VideoComment : BaseDeletableModel<int>
    {
        public string Content { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int VideoId { get; set; }

        public virtual Video Video { get; set; }


        public int? ParentCommentId { get; set; }

        public virtual VideoComment ParentComment { get; set; }

        public virtual IList<VideoComment> SubComments { get; set; }
    }
}
