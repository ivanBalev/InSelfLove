namespace InSelfLove.Web.ViewModels.Courses
{
    using System.Collections.Generic;
    using InSelfLove.Web.InputModels.Courses;

    public class CoursesIndexViewModel
    {
        public CourseCreateInputModel CourseCreateInputModel { get; set; }

        public List<CoursePreviewViewModel> Courses { get; set; }
    }
}
