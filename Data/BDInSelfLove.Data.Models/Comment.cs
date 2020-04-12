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
            this.Reports = new HashSet<Report>();
        }

        [Required]
        public string Content { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public int? ParentPostId { get; set; }

        public virtual Post ParentPost { get; set; }

        public int? ParentCommentId { get; set; }

        public virtual Comment ParentComment { get; set; }

        public virtual IList<Comment> SubComments { get; set; }

        public virtual ICollection<Report> Reports { get; set; }
    }
}
