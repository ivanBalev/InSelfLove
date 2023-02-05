namespace InSelfLove.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;
    using InSelfLove.Data.Common.Models;

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

        public bool IsPaid { get; set; }

        [NotMapped]
        public bool IsUnavailable { get; set; }
    }
}
