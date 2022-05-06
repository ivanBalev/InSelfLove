namespace BDInSelfLove.Data.Models
{
    using BDInSelfLove.Data.Common.Models;
    using System;

    public class Payment : BaseDeletableModel<string>
    {
        public Payment()
        {
            this.Id = Guid.NewGuid().ToString();
        }

        public string ApplicationUserId { get; set; }

        public string StripeCustomerId { get; set; }

        public long AmountTotal { get; set; }

        public string CourseId { get; set; }

        public int? AppointmentId { get; set; }
    }
}
