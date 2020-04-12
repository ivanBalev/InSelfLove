using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Data.Models
{
    public class Report : BaseDeletableModel<int>
    {
        public Report()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public string Reason { get; set; }

        public int CommentId { get; set; }

        public virtual Comment Comment { get; set; }

        public string SubmitterId { get; set; }

        public virtual ApplicationUser Submitter { get; set; }

        public bool IsApproved { get; set; }
    }
}
