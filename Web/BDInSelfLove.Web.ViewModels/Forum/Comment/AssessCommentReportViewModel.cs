using BDInSelfLove.Services.Mapping;
using BDInSelfLove.Services.Models.Comment;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Web.ViewModels.Forum.Comment
{
    public class AssessCommentReportViewModel : IMapFrom<ReportServiceModel>
    {
        public int Id { get; set; }

        public string Reason { get; set; }

        public string SubmitterProfilePhoto { get; set; }

        public string SubmitterUserName { get; set; }
    }
}
