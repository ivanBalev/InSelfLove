namespace InSelfLove.Data.Models
{
    using System;
    using System.Collections.Generic;

    using InSelfLove.Data.Common.Models;

    public class Course : BaseDeletableModel<string>
    {
        public Course()
        {
            this.ApplicationUsers = new HashSet<ApplicationUser>();
            this.CourseVideos = new HashSet<CourseVideo>();
            this.Id = Guid.NewGuid().ToString();
        }

        public string Title { get; set; }

        public string ThumbnailLink { get; set; }

        public string PriceId { get; set; }

        public long Price { get; set; }

        public virtual ICollection<ApplicationUser> ApplicationUsers { get; set; }

        public virtual ICollection<CourseVideo> CourseVideos { get; set; }
    }
}
