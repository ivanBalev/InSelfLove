namespace BDInSelfLove.Data.Models
{
    using System.Collections.Generic;

    using BDInSelfLove.Data.Common.Models;

    public class Course : BaseDeletableModel<string>
    {
        public Course()
        {
            this.Users = new HashSet<ApplicationUser>();
            this.CourseVideos = new HashSet<CourseVideo>();
        }

        public string Title { get; set; }

        public string ThumbnailLink { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; }

        public virtual ICollection<CourseVideo> CourseVideos { get; set; }
    }
}
