using BDInSelfLove.Data.Models;
using BDInSelfLove.Services.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BDInSelfLove.Web.ViewModels.Courses
{
    public class CourseVideoViewModal : IMapFrom<CourseVideo>
    {
        public string Id { get; set; }
    }
}
