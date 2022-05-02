using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Mapping;

namespace BDInSelfLove.Web.ViewModels.Courses
{
    public class CourseViewModel : IMapFrom<Course>
    {
        public string Id { get; set; }

        public string ThumbnailLink { get; set; }

        public string Title { get; set; }
    }
}
