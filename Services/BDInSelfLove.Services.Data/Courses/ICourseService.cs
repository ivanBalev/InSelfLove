namespace BDInSelfLove.Services.Data.Courses
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Models;

    public interface ICourseService
    {
        Task<int> CreateCourse(string guid, string title);

        IQueryable<Course> GetAll();

        IQueryable<CourseVideo> GetById(string id);

        Task<int> CreateCourseVideo(string guid, string title, string courseId);

        IQueryable<CourseVideo> GetCoursePreviewVideo(string courseVideoId);

        IQueryable<CourseVideo> GetCourseVideo(string courseVideoId);
    }
}
