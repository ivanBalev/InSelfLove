namespace BDInSelfLove.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using BDInSelfLove.Data.Common.Models;

    public class Video : BaseDeletableModel<int>
    {
        public Video()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        [Required]
        public string Url { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<VideoComment> VideoComments { get; set; }

        public string Title { get; set; }

        public string AssociatedTerms { get; set; }
    }
}
