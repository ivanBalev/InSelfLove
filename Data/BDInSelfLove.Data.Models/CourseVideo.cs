using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Data.Models
{
    public class CourseVideo : BaseDeletableModel<string>
    {
        public CourseVideo()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string Title { get; set; }

        public string ThumbnailLink { get; set; }

        public string CourseId { get; set; }

        public bool IsPreview { get; set; }
    }
}
