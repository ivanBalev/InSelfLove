using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Data.Models
{
    public class ArticleComment : BaseDeletableModel<int>
    {
        public string Content { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int ArticleId { get; set; }

        public virtual Article Article { get; set; }


        public int? ParentCommentId { get; set; }

        public virtual ArticleComment ParentComment { get; set; }

        public virtual IList<ArticleComment> SubComments { get; set; }
    }
}
