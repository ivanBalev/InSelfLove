using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace InSelfLove.Web.InputModels.Courses
{
    public class CourseCreateInputModel
    {
        [Required]
        public string Title { get; set; }

        public string ThumbnailLink { get; set; }

        public IFormFile ThumbnailImage { get; set; }

        [Required]
        public long Price { get; set; }
    }
}
