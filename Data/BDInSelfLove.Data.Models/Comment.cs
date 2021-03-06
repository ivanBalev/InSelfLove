﻿using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Data.Models
{
    public class Comment : BaseDeletableModel<int>
    {
        public string Content { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int? ArticleId { get; set; }

        public virtual Article Article { get; set; }

        public int? VideoId { get; set; }

        public virtual Video Video { get; set; }

        public int? ParentCommentId { get; set; }

        public virtual Comment ParentComment { get; set; }

        public virtual IList<Comment> SubComments { get; set; }
    }
}
