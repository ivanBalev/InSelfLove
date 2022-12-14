namespace BDInSelfLove.Data.Models
{
    using System;

    using BDInSelfLove.Data.Common.Models;

    public class Appointment : BaseDeletableModel<int>
    {
        public Appointment()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public string Description { get; set; }

        public DateTime UtcStart { get; set; }

        public ApplicationUser User { get; set; }

        public string UserId { get; set; }

        public bool IsApproved { get; set; }

        public bool CanBeOnSite { get; set; }

        public bool IsOnSite { get; set; }
    }
}
