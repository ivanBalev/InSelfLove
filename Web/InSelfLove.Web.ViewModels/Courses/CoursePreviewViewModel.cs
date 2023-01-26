using InSelfLove.Data.Models;
using InSelfLove.Services.Mapping;

namespace InSelfLove.Web.ViewModels.Courses
{
    public class CoursePreviewViewModel : IMapFrom<Course>
    {
        public string Id { get; set; }

        public string ThumbnailLink { get; set; }

        public string Title { get; set; }
    }
}
