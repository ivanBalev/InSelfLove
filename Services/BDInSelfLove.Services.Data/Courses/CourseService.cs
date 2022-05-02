namespace BDInSelfLove.Services.Data.Courses
{
    using System.Linq;
    using System.Threading.Tasks;

    using BDInSelfLove.Data.Common.Repositories;
    using BDInSelfLove.Data.Models;
    using Microsoft.EntityFrameworkCore;

    public class CourseService : ICourseService
    {
        private readonly IDeletableEntityRepository<Course> courseRepository;
        private readonly IDeletableEntityRepository<CourseVideo> courseVideoRepository;

        public CourseService(
            IDeletableEntityRepository<Course> courseRepository,
            IDeletableEntityRepository<CourseVideo> courseVideoRepository)
        {
            this.courseRepository = courseRepository;
            this.courseVideoRepository = courseVideoRepository;
        }

        public async Task<int> CreateCourse(string guid, string title)
        {
            var course = new Course
            {
                Id = guid,
                Title = title,
            };

            await this.courseRepository.AddAsync(course);
            return await this.courseRepository.SaveChangesAsync();
        }

        public async Task<int> CreateCourseVideo(string guid, string title, string courseId)
        {
            var courseVideo = new CourseVideo
            {
                Id = guid,
                Title = title,
                CourseId = courseId,
            };

            var course = await this.courseRepository.All().FirstOrDefaultAsync(x => x.Id.Equals(courseId));

            if(course.CourseVideos.Count() == 0)
            {
                courseVideo.IsPreview = true;
            }

            course.CourseVideos.Add(courseVideo);
            return await this.courseRepository.SaveChangesAsync();
        }

        public IQueryable<Course> GetAll()
        {
            return this.courseRepository.All();
        }

        public IQueryable<CourseVideo> GetById(string id)
        {
            return this.courseVideoRepository.All().Where(x => x.CourseId.Equals(id));
        }

        public IQueryable<CourseVideo> GetCoursePreviewVideo(string courseVideoId)
        {
            return this.courseRepository.All()
                .Include(c => c.CourseVideos)
                .Where(c => c.CourseVideos.Any(cv => cv.Id.Equals(courseVideoId)))
                .Select(c => c.CourseVideos.First(cv => cv.IsPreview));
        }

        public IQueryable<CourseVideo> GetCourseVideo(string courseVideoId)
        {
            return this.courseVideoRepository.All()
                .Where(x => x.Id.Equals(courseVideoId));
        }
    }
}
