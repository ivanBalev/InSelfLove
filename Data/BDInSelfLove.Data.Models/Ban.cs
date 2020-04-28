using BDInSelfLove.Data.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace BDInSelfLove.Data.Models
{
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
