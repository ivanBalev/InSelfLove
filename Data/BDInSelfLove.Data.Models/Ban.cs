namespace BDInSelfLove.Data.Models
{
    using System;

    using BDInSelfLove.Data.Common.Models;

    public class Ban : BaseDeletableModel<int>
    {
        public Ban()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public string UserId { get; set; }

        public virtual ApplicationUser User { get; set; }

        public string Reason { get; set; }
    }
}
