using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BDInSelfLove.Web.InputModels.Forum.Comment
{
    public class AssessCommentReportinputModel
    {
        [Required]
        public int ParentPostId { get; set; }

        [Required]
        public int ReportId { get; set; }

        [Required]
        public bool AssessmentValue { get; set; }
    }
}
