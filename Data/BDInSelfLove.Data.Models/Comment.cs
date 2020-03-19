namespace BDInSelfLove.Data.Models
{
    using BDInSelfLove.Data.Common.Models;
    using System.ComponentModel.DataAnnotations;

    public class Comment : BaseDeletableModel<int>
    {
        [Required]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int? ParentCommentId { get; set; }

        // Can have one of either pair below?
        public virtual Comment ParentComment { get; set; }

        public int? ParentVideoId { get; set; }

        public virtual Video ParentVideo { get; set; }

        public int? ParentArticleId { get; set; }

        public virtual Article ParentArticle { get; set; }
    }
}
