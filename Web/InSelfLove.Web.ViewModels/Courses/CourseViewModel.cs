namespace InSelfLove.Web.ViewModels.Courses
{
    using System.Collections.Generic;

    using InSelfLove.Data.Models;
    using InSelfLove.Services.Mapping;

    public class CourseViewModel : IMapFrom<Course>
    {
        public List<CourseVideoPreviewViewModel> CourseVideos { get; set; }

        public string Title { get; set; }

        public string Id { get; set; }

        public string PriceId { get; set; }

        public long Price { get; set; }
    }
}
