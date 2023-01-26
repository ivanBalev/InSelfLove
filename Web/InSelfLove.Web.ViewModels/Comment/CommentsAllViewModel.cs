using System.Collections.Generic;

namespace InSelfLove.Web.ViewModels.Comment
{
    public class CommentsAllViewModel
    {
        public int? ArticleId { get; set; }

        public int? VideoId { get; set; }

        public ICollection<CommentViewModel> Comments { get; set; }
    }
}
