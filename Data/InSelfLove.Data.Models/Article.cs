namespace InSelfLove.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    using InSelfLove.Data.Common.Models;

    public class Article : BaseDeletableModel<int>
    {
        public Article()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        [Required]
        public string Title { get; set; }

        public string Slug { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string ImageUrl { get; set; }

        public int ImageWidth { get; set; }

        public int ImageHeight { get; set; }

        public string PreviewImageUrl { get; set; }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
    }
}
