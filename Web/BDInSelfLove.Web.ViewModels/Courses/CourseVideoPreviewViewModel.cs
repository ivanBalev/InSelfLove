namespace BDInSelfLove.Web.ViewModels.Courses
{
    using BDInSelfLove.Data.Models;
    using BDInSelfLove.Services.Mapping;

    public class CourseVideoPreviewViewModel : IMapFrom<CourseVideo>
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string ThumbnailLink { get; set; }

        public string CourseId { get; set; }
    }
}
