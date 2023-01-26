namespace InSelfLove.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using InSelfLove.Data.Common.Models;

    public class Video : BaseDeletableModel<int>
    {
        public Video()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        [Required]
        public string Url { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }

        public string Title { get; set; }

        public string Slug { get; set; }

        public string AssociatedTerms { get; set; }
    }
}
