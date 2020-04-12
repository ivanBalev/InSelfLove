using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Data.Models
{
    public class Category : BaseDeletableModel<int>
    {
        public Category()
        {
            this.CreatedOn = DateTime.UtcNow;
            this.Posts = new HashSet<Post>();
        }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public ICollection<Post> Posts { get; set; }
    }
}
