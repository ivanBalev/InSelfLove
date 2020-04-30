namespace BDInSelfLove.Data.Models
{
    using System;

    using BDInSelfLove.Data.Common.Models;

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

        public string OffenderId { get; set; }

        public virtual ApplicationUser Offender { get; set; }

        public bool IsApproved { get; set; }
    }
}
