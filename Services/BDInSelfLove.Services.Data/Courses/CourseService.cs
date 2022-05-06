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
        private readonly IDeletableEntityRepository<ApplicationUser> userRepository;

        public CourseService(
            IDeletableEntityRepository<Course> courseRepository,
            IDeletableEntityRepository<CourseVideo> courseVideoRepository,
            IDeletableEntityRepository<ApplicationUser> userRepository)
        {
            this.courseRepository = courseRepository;
            this.courseVideoRepository = courseVideoRepository;
            this.userRepository = userRepository;
        }

        public async Task<string> CreateCourse(string title, string thumbnailLink, string priceId, long price)
        {
            var course = new Course
            {
                Title = title,
                ThumbnailLink = thumbnailLink,
                PriceId = priceId,
                Price = price,
            };

            await this.courseRepository.AddAsync(course);
            await this.courseRepository.SaveChangesAsync();

            return course.Id;
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

            if (course.CourseVideos.Count() == 0)
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

        public IQueryable<Course> GetById(string id)
        {
            return this.courseRepository.All().Where(c => c.Id.Equals(id)).Include(c => c.CourseVideos);
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
