using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Comment
{
    public class CommentViewModel
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public string UserId { get; set; }

        public DateTime CreatedOn { get; set; }

        public string UserUserName { get; set; }

        public int ParentCommentId { get; set; }
    }
}
