namespace BDInSelfLove.Web.ViewModels.Courses
{
    using System.Collections.Generic;
    using BDInSelfLove.Web.InputModels.Courses;

    public class CoursesIndexViewModel
    {
        public CourseCreateInputModel CourseCreateInputModel { get; set; }

        public List<CoursePreviewViewModel> Courses { get; set; }
    }
}
