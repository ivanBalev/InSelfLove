namespace BDInSelfLove.Data.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Data.Common.Models;

    public class Comment : BaseDeletableModel<int>
    {
        public Comment()
        {
            this.SubComments = new List<Comment>();
        }

        [Required]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }


        public int? ParentPostId { get; set; }

        public virtual Post ParentPost { get; set; }

        public int? MainCommentId { get; set; }

        public virtual Comment MainComment { get; set; }

        public int? ParentCommentId { get; set; }

        public virtual Comment ParentComment { get; set; }

        public int? ParentVideoId { get; set; }

        public virtual Video ParentVideo { get; set; }

        public int? ParentArticleId { get; set; }

        public virtual Article ParentArticle { get; set; }

        public virtual IList<Comment> SubComments { get; set; }
    }
}
