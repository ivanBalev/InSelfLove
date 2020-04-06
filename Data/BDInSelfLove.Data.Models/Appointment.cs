using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Data.Models
{
    public class Appointment : BaseDeletableModel<int>
    {
        public Appointment()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public string Description { get; set; }

        public DateTime Start { get; set; }

        public ApplicationUser User { get; set; }

        public string UserId { get; set; }

        public bool IsApproved { get; set; }
    }
}
