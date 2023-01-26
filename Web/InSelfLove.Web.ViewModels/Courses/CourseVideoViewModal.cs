using InSelfLove.Data.Models;
using InSelfLove.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSelfLove.Web.ViewModels.Courses
{
    public class CourseVideoViewModal : IMapFrom<CourseVideo>
    {
        public string Id { get; set; }
    }
}
