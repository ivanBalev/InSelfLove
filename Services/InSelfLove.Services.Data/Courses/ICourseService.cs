namespace InSelfLove.Services.Data.Courses
{
    using System.Linq;
    using System.Threading.Tasks;

    using InSelfLove.Data.Models;

    public interface ICourseService
    {
        Task<string> CreateCourse(string title, string thumbnailLink, string priceId, long price);

        IQueryable<Course> GetAll();

        IQueryable<Course> GetById(string id);

        Task<int> CreateCourseVideo(string guid, string title, string courseId);

        IQueryable<CourseVideo> GetCoursePreviewVideo(string courseVideoId);

        IQueryable<CourseVideo> GetCourseVideo(string courseVideoId);

    }
}
